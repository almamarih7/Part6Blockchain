using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockchainAssignment
{
    class Block : Form
    {
        static ReaderWriterLock _object = new ReaderWriterLock();
        static public long Staticnonce = 0;
         public string Eachblocktime = "";
         static public string blocktime = "";
        static string ActualHash = "";
        static int ActualDiff = 0;
        static long ActualNonce = 0L;
        public DateTime timestamp;
        public int index;
        public static String Staichash;
        public String hash;
        public String prevHash;

        public List<Transaction> transactionList = new List<Transaction>();
        public String merkleRoot;
        public static BlockchainApp _Form1;
        // Proof-of-work
        public long Statinonce = 0;
        public static  long nonce = 0;
        public int difficulty = 2;

        //Rewards and fees
        public double reward = 1.0; // Fixed logic 
        public double fees = 0.0;

        public String minerAddress = String.Empty;


        /* Genesis Block Constructor */
        public Block()
        {
            timestamp = DateTime.Now;
            index = 0;
            prevHash = String.Empty;
            reward = 0;
            hash = String.Empty;
        }

        public Block(String prevHash, int index)
        {
            timestamp = DateTime.Now;
            this.prevHash = prevHash;
            this.index = index + 1;
            hash = String.Empty;
            _Form1.update(hash);
        }

        public Block(Block lastBlock, List<Transaction> transactions, BlockchainApp _form1, String address = "")
        {
            timestamp = DateTime.Now;
            prevHash = lastBlock.hash;
            index = lastBlock.index + 1;
            _Form1 = _form1;
            minerAddress = address;

            transactions.Add(CreateRewardTransaction(transactions));
            transactionList = transactions;

            merkleRoot = MerkleRoot(transactionList);

            hash = Mine(lastBlock);
            difficulty = ActualDiff;
            Eachblocktime = blocktime;
        }

        public Transaction CreateRewardTransaction(List<Transaction> transactions)
        {
            // Sum the fees in the list of transactions in the mined block
            fees = transactions.Aggregate(0.0, (acc, t) => acc + t.fee);

            // Create the "Reward Transaction" being the sum of fees and reward being transferred from "Mine Rewards" (Coin Base) to miner
            return new Transaction("Mine Rewards", minerAddress, (reward + fees), 0, "");
        }

        public static String CreateHash(Block block,long nonce)
        {
            String hash = String.Empty;
            SHA256 hasher = SHA256Managed.Create();

            //Concatenate all Block properties for hashing
            String input = block.index.ToString() + block.timestamp.ToString() + block.prevHash + nonce.ToString() + block.reward.ToString() + block.merkleRoot;
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }

            return hash;
        }

        public static String Mine(Block lastBlock)
        {
            Staticnonce = 0L;
            String hash = CreateHash(lastBlock, Staticnonce);


            DateTime timestamp = DateTime.UtcNow;
            

            int difficulty = lastBlock.difficulty;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            ThreadPool.SetMaxThreads(0, 10);
            hash = CreateHash(lastBlock,0);


            Func<bool> whileCondFn = () => !hash.StartsWith(string.Concat(Enumerable.Repeat("0", difficulty)));


            ParallelUtils.While(whileCondFn, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (ParallelLoopState) =>
            {
                
                lock (_object)
                {

                    difficulty = AdjustDifficulty(lastBlock, timestamp,3);

                    timestamp = DateTime.UtcNow;
                    
                    Staticnonce += 1;
                    hash = CreateHash(lastBlock, Staticnonce); ;
                    
                    System.Diagnostics.Debug.WriteLine(hash + Staticnonce);
                    System.Diagnostics.Debug.WriteLine(difficulty);
                    //Console.WriteLine(whileCondFn());
                    if (hash.StartsWith(string.Concat(Enumerable.Repeat("0", difficulty))))
                    {

                        Console.WriteLine("fdasfjkla;jsdfads" + hash);
                        Console.WriteLine(Staticnonce);
                        ActualHash = hash;
                        ActualDiff = difficulty;
                        //_Form1.richTextBox1.Invoke(new Action(() => _Form1.richTextBox1.Text += hash));
                        //BlockchainApp._Form1.update(hash);

                        ActualNonce = Staticnonce;
                        ParallelLoopState.Stop();

                    }
                }


            });
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            blocktime = elapsedTime;
            nonce = ActualNonce;
            return ActualHash;
        }

        public static String MerkleRoot(List<Transaction> transactionList)
        {
            List<String> hashes = transactionList.Select(t => t.hash).ToList();
            if (hashes.Count == 0)
            {
                return String.Empty;
            }
            if (hashes.Count == 1)
            {
                return HashCode.HashTools.combineHash(hashes[0], hashes[0]);
            }
            while (hashes.Count != 1)
            {
                List<String> merkleLeaves = new List<String>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    if (i == hashes.Count - 1)
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i]));
                    }
                    else
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i + 1]));
                    }
                }
                hashes = merkleLeaves;
            }
            return hashes[0];
        }


        public override string ToString()
        {
            String output = String.Empty;
            transactionList.ForEach(t => output += (t.ToString() + "\n"));

            return "Index: " + index.ToString() +
                "\tTimestamp: " + timestamp.ToString() +
                "\nPrevious Hash: " + prevHash +
                "\nHash: " + hash +
                "\nNonce: " + nonce.ToString() +
                "\nDifficulty: " + difficulty.ToString() +
                "\nReward: " + reward.ToString() +
                "\nFees: " + fees.ToString() +
                "\nMiner's Address: " + minerAddress +
                "\nTransactions:\n" + output +
                "\nBlock Time: "+Eachblocktime+ "\n";
        }


        public static int AdjustDifficulty(Block originalBlock, DateTime timestamp,int max)
        {
            Debug.WriteLine("helloadjust" + timestamp + "    "+originalBlock.timestamp+"      +    "   +originalBlock.difficulty);
            if (originalBlock.difficulty <= 1)
                return 1;

            if ((timestamp - originalBlock.timestamp).TotalMilliseconds > 60) return originalBlock.difficulty - 1;
            if (max > originalBlock.difficulty)
                return originalBlock.difficulty + 1;
            else
                return originalBlock.difficulty;
        }
        public class ParallelUtils
        {
            public static void While(Func<bool> whileCondFn, ParallelOptions parallelOptions, Action<ParallelLoopState> body)
            {

                Parallel.ForEach(Infinite(), parallelOptions, (ignored, loopState) =>
                {
                    if (!loopState.IsStopped)
                    {
                        //Console.WriteLine(Task.CurrentId + "  " + currentBlockHash);
                        if (whileCondFn())
                        {

                            //Console.WriteLine(hash);
                            body(loopState);
                            //new Thread(()=> ).Start();
                        }
                        else
                        {

                            loopState.Stop();


                        }
                    }
                });
                //Console.WriteLine("h====      " + hash);
            }

            private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
            {
                while (condition()) yield return true;
            }
            private static IEnumerable<bool> Infinite()
            {
                while (true) yield return true;
            }
        }

    }
}

