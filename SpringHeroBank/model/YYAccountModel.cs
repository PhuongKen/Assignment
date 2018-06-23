using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Transactions;
using MySql.Data.MySqlClient;
using SpringHeroBank.entity;
using SpringHeroBank.entity;
using SpringHeroBank.error;

namespace SpringHeroBank.model
{
    public class YYAccountModel
    {
        public Boolean Save(YYAccount account)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "insert into `accounts` " +
                                 "(accountNumber, username, password, balance, identityCard, fullName, " +
                                 "email, phoneNumber, address, dob, gender, salt)" +
                                 " values " +
                                 "(@accountNumber, @username, @password, @balance, @identityCard, @fullName, " +
                                 "@email, @phoneNumber, @address, @dob, @gender, @salt)";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", account.AccountNumber);
            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@password", account.Password);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@identityCard", account.IdentityCard);
            cmd.Parameters.AddWithValue("@fullName", account.FullName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@phoneNumber", account.PhoneNumber);
            cmd.Parameters.AddWithValue("@address", account.Address);
            cmd.Parameters.AddWithValue("@dob", account.Dob);
            cmd.Parameters.AddWithValue("@gender", account.Gender);
            cmd.Parameters.AddWithValue("@salt", account.Salt);
            cmd.ExecuteNonQuery();
            DbConnection.Instance().CloseConnection();
            return true;
        }

        public Boolean CheckExistUsername(string username)
        {
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `accounts` where username = @username";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            DbConnection.Instance().CloseConnection();
            return isExist;
        }

        public YYAccount GetByUsername(string username)
        {
            YYAccount account = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `accounts` where username = @username and status = 1";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();

            if (isExist)
            {
                account = new YYAccount
                {
                    AccountNumber = reader.GetString("accountNumber"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Salt = reader.GetString("salt"),
                    FullName = reader.GetString("fullName"),
                    Balance = reader.GetInt32("balance")
                };
            }

            reader.Close();
            DbConnection.Instance().CloseConnection();
            return account;
        }

        public YYAccount GetByAccountNumber(string accountNumber)
        {
            YYAccount account = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `accounts` where accountNumber = @accountNumber and status = 1";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (isExist)
            {
                account = new YYAccount
                {
                    AccountNumber = reader.GetString("accountNumber"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Salt = reader.GetString("salt"),
                    FullName = reader.GetString("fullName"),
                    Balance = reader.GetInt32("balance")
                };
            }

            DbConnection.Instance().CloseConnection();
            reader.Close();
            return account;
        }

        public bool UpdateBalance(YYAccount account, YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.

            try
            {
                /**
                 * 1. Lấy thông tin số dư mới nhất của tài khoản.
                 * 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw.
                 *     2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.                 
                 * 3. Update số dư vào tài khoản.
                 *     3.1. Tính toán lại số tiền trong tài khoản.
                 *     3.2. Update số tiền vào database.
                 * 4. Lưu thông tin transaction vào bảng transaction.
                 */

                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `accounts` where username = @username and status = 1";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                var balanceReader = queryBalanceCommand.ExecuteReader();

                var isExist = balanceReader.Read();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.

                if (!isExist)
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }


                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                //balanceReader.Close();
                balanceReader.Close();
                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (historyTransaction.Type != YYTransaction.TransactionType.DEPOSIT
                    && historyTransaction.Type != YYTransaction.TransactionType.WITHDRAW)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (historyTransaction.Type == YYTransaction.TransactionType.WITHDRAW &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (historyTransaction.Type == YYTransaction.TransactionType.DEPOSIT)
                {
                    currentBalance += historyTransaction.Amount;
                }
                else
                {
                    currentBalance -= historyTransaction.Amount;
                }

                // 3.2. Update số dư vào dat1abase.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `accounts` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();
                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        public bool UpdateTranfers(string senderAccountNumber, string receiverAccountNumber,
            YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection();
            var transaction = DbConnection.Instance().Connection.BeginTransaction();

            try
            {
                // 1. Lấy thông tin số dư mới nhất của tài khoản gửi.
                var queryBalance =
                    "select balance from `accounts` where accountNumber = @senderAccountNumber and status = 1";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@senderAccountNumber", senderAccountNumber);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                var isExist = balanceReader.Read();
                if (!isExist)
                {
                    throw new SpringHeroTransactionException("Invalid accountNumber");
                }

                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();
                if (historyTransaction.Type != YYTransaction.TransactionType.TRANSFER)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }

                if (historyTransaction.Type == YYTransaction.TransactionType.TRANSFER &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }
                
                //1. Lấy thông tin số dư mới nhất của tài khoản người nhận
                var queryBalanceReceiver =
                    "select balance from `accounts` where accountNumber = @receiverAccountNumber and status = 1";
                MySqlCommand queryBalanceCommandReceiver = new MySqlCommand(queryBalanceReceiver, DbConnection.Instance().Connection);
                queryBalanceCommandReceiver.Parameters.AddWithValue("@receiverAccountNumber", receiverAccountNumber);
                var balanceReaderReceiver = queryBalanceCommandReceiver.ExecuteReader();
                var isExistReceiver = balanceReaderReceiver.Read();
                if (!isExistReceiver)
                {
                    throw new SpringHeroTransactionException("Invalid accountNumber");
                }

                var currentBalanceReceiver = balanceReaderReceiver.GetDecimal("balance");
                balanceReaderReceiver.Close();
                if (historyTransaction.Type != YYTransaction.TransactionType.TRANSFER)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }
                
                //2. Tính toán lại số tiền tài khoản người gửi và người nhận.
                    currentBalance -= historyTransaction.Amount;
                    currentBalanceReceiver += historyTransaction.Amount;
                
                
                // 3. Update số dư vào dat1abase tài khoản người gửi.
                var updateAccountsender = 0;
                var queryUpdateAccountsender =
                    "update `accounts` set balance = @balance where accountNumber = @senderAccountNumber and status = 1";
                var cmdUpdateAccountsender =
                    new MySqlCommand(queryUpdateAccountsender, DbConnection.Instance().Connection);
                cmdUpdateAccountsender.Parameters.AddWithValue("@senderAccountNumber", senderAccountNumber);
                cmdUpdateAccountsender.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountsender = cmdUpdateAccountsender.ExecuteNonQuery();
                
                // 4. Update  số dư vào dat1abase tài khoản người nhận.
                var updateAccountreceiver = 0;
                var queryUpdateAccountreceiver =
                    "update `accounts` set balance = @balance where accountNumber = @receiverAccountNumber and status = 1";
                var cmdUpdateAccountreceiver =
                    new MySqlCommand(queryUpdateAccountreceiver, DbConnection.Instance().Connection);
                cmdUpdateAccountreceiver.Parameters.AddWithValue("@receiverAccountNumber", receiverAccountNumber);
                cmdUpdateAccountreceiver.Parameters.AddWithValue("@balance", currentBalanceReceiver);
                updateAccountreceiver = cmdUpdateAccountreceiver.ExecuteNonQuery();

                // 5. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();
                if (updateAccountsender == 1 && updateAccountreceiver == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

       
        public List<YYTransaction> QueryTransactionHistory()
        {
            DbConnection.Instance().OpenConnection();
            List<YYTransaction> list = new List<YYTransaction>();
            DbConnection.Instance().OpenConnection();
            
            var query3 = "select * from `transactions` where " +
                         "senderAccountNumber = @senderAccountNumber and type = 2 and DAY(createdAt)";
            MySqlCommand cmd3 = new MySqlCommand(query3, DbConnection.Instance().Connection);
            cmd3.Parameters.AddWithValue("@senderAccountNumber", Program.currentLoggedInYyAccount.AccountNumber);
            var reader3 = cmd3.ExecuteReader();
            while (reader3.Read())
            {
                var id = reader3.GetString("id");
                var type = reader3.GetInt16("type");
                YYTransaction.TransactionType resultType = YYTransaction.TransactionType.DEPOSIT;
                if (type == 1)
                {
                    resultType = YYTransaction.TransactionType.DEPOSIT;
                }
                else if (type == 2)
                {
                    resultType = YYTransaction.TransactionType.WITHDRAW;
                }
                else if (type == 3)
                {
                    resultType = YYTransaction.TransactionType.TRANSFER;
                }

                var amount = reader3.GetDecimal("amount");
                var content = reader3.GetString("content");
                var senderAccountNumber = reader3.GetString("senderAccountNumber");
                var receiverAccountNumber = reader3.GetString("receiverAccountNumber");
                var status = reader3.GetInt16("status");
                YYTransaction.ActiveStatus resultStatus = YYTransaction.ActiveStatus.DONE;
                if (status == -1)
                {
                    resultStatus = YYTransaction.ActiveStatus.DELETED;
                }
                else if (status == 0)
                {
                    resultStatus = YYTransaction.ActiveStatus.REJECT;
                }
                else if (status == 1)
                {
                    resultStatus = YYTransaction.ActiveStatus.PROCESSING;
                }
                else if (status == 2)
                {
                    resultStatus = YYTransaction.ActiveStatus.DONE;
                }

                var createdAt = reader3.GetString("createdAt");
                YYTransaction transaction = new YYTransaction(id, amount, content, senderAccountNumber,
                    receiverAccountNumber, resultType, createdAt, resultStatus);
                list.Add(transaction);
            }
            reader3.Close();
            
            var query = "select * from `transactions` where senderAccountNumber = @senderAccountNumber and type = 1";
            MySqlCommand cmd = new MySqlCommand(query, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@senderAccountNumber", Program.currentLoggedInYyAccount.AccountNumber);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString("id");
                var type = reader.GetInt16("type");
                YYTransaction.TransactionType resultType = YYTransaction.TransactionType.DEPOSIT;
                if (type == 1)
                {
                    resultType = YYTransaction.TransactionType.DEPOSIT;
                }
                else if (type == 2)
                {
                    resultType = YYTransaction.TransactionType.WITHDRAW;
                }
                else if (type == 3)
                {
                    resultType = YYTransaction.TransactionType.TRANSFER;
                }

                var amount = reader.GetDecimal("amount");
                var content = reader.GetString("content");
                var senderAccountNumber = reader.GetString("senderAccountNumber");
                var receiverAccountNumber = reader.GetString("receiverAccountNumber");
                var status = reader.GetInt16("status");
                YYTransaction.ActiveStatus resultStatus = YYTransaction.ActiveStatus.DONE;
                if (status == -1)
                {
                    resultStatus = YYTransaction.ActiveStatus.DELETED;
                }
                else if (status == 0)
                {
                    resultStatus = YYTransaction.ActiveStatus.REJECT;
                }
                else if (status == 1)
                {
                    resultStatus = YYTransaction.ActiveStatus.PROCESSING;
                }
                else if (status == 2)
                {
                    resultStatus = YYTransaction.ActiveStatus.DONE;
                }

                var createdAt = reader.GetString("createdAt");
                YYTransaction transaction = new YYTransaction(id, amount, content, senderAccountNumber,
                    receiverAccountNumber, resultType, createdAt, resultStatus);
                list.Add(transaction);
            }
            reader.Close();
            
            var query1 = "select * from `transactions` where receiverAccountNumber = @receiverAccountNumber and type = 3";
            MySqlCommand cmd1 = new MySqlCommand(query1, DbConnection.Instance().Connection);
            cmd1.Parameters.AddWithValue("@receiverAccountNumber", Program.currentLoggedInYyAccount.AccountNumber);
            var reader1 = cmd1.ExecuteReader();
            while (reader1.Read())
            {
                var id = reader1.GetString("id");
                var type = reader1.GetInt16("type");
                YYTransaction.TransactionType resultType = YYTransaction.TransactionType.DEPOSIT;
                if (type == 1)
                {
                    resultType = YYTransaction.TransactionType.DEPOSIT;
                }
                else if (type == 2)
                {
                    resultType = YYTransaction.TransactionType.WITHDRAW;
                }
                else if (type == 3)
                {
                    resultType = YYTransaction.TransactionType.TRANSFER;
                }

                var amount = reader1.GetDecimal("amount");
                var content = reader1.GetString("content");
                var senderAccountNumber = reader1.GetString("senderAccountNumber");
                var receiverAccountNumber = reader1.GetString("receiverAccountNumber");
                var status = reader1.GetInt16("status");
                YYTransaction.ActiveStatus resultStatus = YYTransaction.ActiveStatus.DONE;
                if (status == -1)
                {
                    resultStatus = YYTransaction.ActiveStatus.DELETED;
                }
                else if (status == 0)
                {
                    resultStatus = YYTransaction.ActiveStatus.REJECT;
                }
                else if (status == 1)
                {
                    resultStatus = YYTransaction.ActiveStatus.PROCESSING;
                }
                else if (status == 2)
                {
                    resultStatus = YYTransaction.ActiveStatus.DONE;
                }

                var createdAt = reader1.GetString("createdAt");
                YYTransaction transaction = new YYTransaction(id, amount, content, senderAccountNumber,
                    receiverAccountNumber, resultType, createdAt, resultStatus);
                list.Add(transaction);
            }
            reader1.Close();

            var query2 = "select * from `transactions` where senderAccountNumber = @senderAccountNumber and type = 3";
            MySqlCommand cmd2 = new MySqlCommand(query2, DbConnection.Instance().Connection);
            cmd2.Parameters.AddWithValue("@senderAccountNumber", Program.currentLoggedInYyAccount.AccountNumber);
            var reader2 = cmd2.ExecuteReader();
            while (reader2.Read())
            {
                var id = reader2.GetString("id");
                var type = reader2.GetInt16("type");
                YYTransaction.TransactionType resultType = YYTransaction.TransactionType.DEPOSIT;
                if (type == 1)
                {
                    resultType = YYTransaction.TransactionType.DEPOSIT;
                }
                else if (type == 2)
                {
                    resultType = YYTransaction.TransactionType.WITHDRAW;
                }
                else if (type == 3)
                {
                    resultType = YYTransaction.TransactionType.TRANSFER;
                }

                var amount = reader2.GetDecimal("amount");
                var content = reader2.GetString("content");
                var senderAccountNumber = reader2.GetString("senderAccountNumber");
                var receiverAccountNumber = reader2.GetString("receiverAccountNumber");
                var status = reader2.GetInt16("status");
                YYTransaction.ActiveStatus resultStatus = YYTransaction.ActiveStatus.DONE;
                if (status == -1)
                {
                    resultStatus = YYTransaction.ActiveStatus.DELETED;
                }
                else if (status == 0)
                {
                    resultStatus = YYTransaction.ActiveStatus.REJECT;
                }
                else if (status == 1)
                {
                    resultStatus = YYTransaction.ActiveStatus.PROCESSING;
                }
                else if (status == 2)
                {
                    resultStatus = YYTransaction.ActiveStatus.DONE;
                }

                var createdAt = reader2.GetString("createdAt");
                YYTransaction transaction = new YYTransaction(id, amount, content, senderAccountNumber,
                    receiverAccountNumber, resultType, createdAt, resultStatus);
                list.Add(transaction);
            }
            DbConnection.Instance().CloseConnection();
            return list;
        }
    }
}