using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        public List<Block> Blocks = new List<Block>();
        public List<Transaction> transactionPool = new List<Transaction>();
        int transactionsPerBlock = 1;

        public Blockchain()
        {
            Blocks.Add(new Block());
        }

        public String getBlockAsString(int index)
        {
            return Blocks[index].ToString();
        }

        public Block GetLastBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        public List<Transaction> getPendingTransactions()
        {
            int n = Math.Min(transactionsPerBlock, transactionPool.Count);
            List<Transaction> transactions = transactionPool.GetRange(0, n);
            transactionPool.RemoveRange(0, n);

            return transactions;
        }

        public List<Transaction> gettransactionPool(int method,string minerAddress)
        {
            
            List<Transaction> transactions = new List<Transaction>();
            
            //Greedy highest Fee
             if(method == 0)
            {
                double maxFee = 0;
                Transaction max;
                foreach(var trans in transactionPool)
                {
                    if (trans.fee > maxFee)
                    {
                        transactions = new List<Transaction>();
                        maxFee = trans.fee;
                        max = trans;
                        transactions.Add(max);
                    }
                }
                
            }
            //Altruistic Longest Wait
            else if (method == 1)
            {
                double maxtime = 0;
                Transaction max;
                foreach (var trans in transactionPool)
                {
                    if ((DateTime.Now - trans.timestamp).TotalMilliseconds > maxtime)
                    {
                        transactions = new List<Transaction>();
                        maxtime = (DateTime.Now - trans.timestamp).TotalMilliseconds;
                        max = trans;
                        transactions.Add(max);
                    }
                }

            }
             //Random 
            else if(method == 2)
            {
                Random r = new Random();
                int rInt = r.Next(0, transactionPool.Count-1);
                transactions.Add(transactionPool[rInt]);
            }
             //address
             else if(method == 3)
            {
                foreach (var trans in transactionPool)
                {
                    if (trans.senderAddress == minerAddress || trans.recipientAddress == minerAddress)
                    {
                        
                        transactions.Add(trans);

                    }
                }
            }else
                transactions.Add(transactionPool[0]);


             foreach(var trans in transactions)
            {
                transactionPool.Remove(trans);
            }
            return transactions;
        }
        public override string ToString()
        {
            String output = String.Empty;
            Blocks.ForEach(b => output += (b.ToString() + "\n"));
            return output;
        }

        public double GetBalance(String address)
        {
            double balance = 0.0;
            foreach(Block b in Blocks)
            {
                foreach(Transaction t in b.transactionList)
                {
                    if (t.recipientAddress.Equals(address))
                    {
                        balance += t.amount;
                    }
                    if (t.senderAddress.Equals(address))
                    {
                        balance -= (t.amount + t.fee);
                    }
                }
            }

            return balance;
        }

        public bool validateMerkleRoot(Block b)
        {
            String reMerkle = Block.MerkleRoot(b.transactionList);
            return reMerkle.Equals(b.merkleRoot);
        }
    }
}
