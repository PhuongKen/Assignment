using System.Collections.Generic;
using System.Transactions;
using MySql.Data.MySqlClient;
using SpringHeroBank.entity;

namespace SpringHeroBank.model
{
    public class YYTransactionModel
    {
        public List<YYTransaction> TransactionHistory(string accountNumber)
        {
            DbConnection.Instance().OpenConnection();
            var list = new List<YYTransaction>();

            var sqlQuery =
                "select * from `transactions` where receiverAccountNumber = @accountnumber or senderAccountNumber = @accountnumber";
            var cmd = new MySqlCommand(sqlQuery, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountnumber", accountNumber);
            var transactionReader = cmd.ExecuteReader();
            while (transactionReader.Read())
            {
                YYTransaction transaction = new YYTransaction()
                {
                    Amount = transactionReader.GetDecimal("amount"),
                    Content = transactionReader.GetString("content"),
                    Id = transactionReader.GetString("id"),
                    Type = (YYTransaction.TransactionType) transactionReader.GetInt32("type"),
                    SenderAccountNumber = transactionReader.GetString("senderAccountNumber"),
                    ReceiverAccountNumber = transactionReader.GetString("receiverAccountNumber"),
                    Status = (YYTransaction.ActiveStatus) transactionReader.GetInt32("status"),
                    CreatedAt = transactionReader.GetMySqlDateTime("createdAt").ToString()
                };
                list.Add(transaction);
            }

            DbConnection.Instance().CloseConnection();
            return list;
        }
    }
}