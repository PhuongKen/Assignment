using System;
using System.Text;
using System.Transactions;
using SpringHeroBank.entity;
using SpringHeroBank.model;

namespace SpringHeroBank.controller
{
    public class YYTransactionController
    {
        private YYTransactionModel TransactionModel = new YYTransactionModel();
        private YYAccountModel AccountModel = new YYAccountModel();
        
        public void GetListTransaction()
        {
            Console.Clear();
            StringBuilder builder = new StringBuilder();
            YYAccount account = null;
            var list =
                TransactionModel.TransactionHistory(Program.currentLoggedInYyAccount.AccountNumber);
            builder.Append("Transaction history.");
            builder.AppendLine();
            builder.AppendFormat("{0,-40} {1,-10} {2,-15} {3,-30} {4,-30} {5,-25} {6,-15}", "Transaction ID",
                "Type",  "Amount", "Content", "From", "To", "Created date");
            builder.AppendLine();
            foreach (var transaction in list)
            {
                if (transaction.Type == YYTransaction.TransactionType.DEPOSIT)
                {
                    builder.AppendFormat("{0,-40} {1,-10} {2,-15} {3,-30} {4,-30} {5,-25} {6,-15}",
                        transaction.Id, "Deposit",transaction.Amount, transaction.Content , "You", "You",
                        transaction.CreatedAt);
                    builder.AppendLine();
                }
                else if (transaction.Type == YYTransaction.TransactionType.WITHDRAW)
                {
                    builder.AppendFormat("{0,-40} {1,-10} {2,-15} {3,-30} {4,-30} {5,-25} {6,-15}",
                        transaction.Id, "Withdraw", transaction.Amount, transaction.Content , "You", "You",
                        transaction.CreatedAt);
                    builder.AppendLine();
                }
                else
                {
                    if (Program.currentLoggedInYyAccount.AccountNumber == transaction.SenderAccountNumber)
                    {
                        account = AccountModel.GetByAccountNumber(transaction.ReceiverAccountNumber);
                        builder.AppendFormat("{0,-40} {1,-10} {2,-15} {3,-30} {4,-30} {5,-25} {6,-15}",
                            transaction.Id, "Transfer", transaction.Amount, transaction.Content , "You", account.FullName,
                            transaction.CreatedAt);
                    }
                    else
                    {
                        account = AccountModel.GetByAccountNumber(transaction.SenderAccountNumber);
                        builder.AppendFormat("{0,-40} {1,-10} {2,-15} {3,-30} {4,-30} {5,-25} {6,-15}",
                            transaction.Id, "Transfer",transaction.Amount, transaction.Content ,account.FullName,"You",
                            transaction.CreatedAt);
                    }

                    builder.AppendLine();
                }
            }

            Console.WriteLine(builder);
        }
    }
}