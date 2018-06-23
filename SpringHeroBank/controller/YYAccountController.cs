using System;
using System.Collections.Generic;
using System.Transactions;
using MySql.Data.MySqlClient;
using SpringHeroBank;
using SpringHeroBank.entity;
using SpringHeroBank.model;
using SpringHeroBank.utility;
using SpringHeroBank.entity;

namespace SpringHeroBank.controller
{
    public class YYAccountController
    {
        private YYAccountModel model = new YYAccountModel();

        public bool Register()
        {
            YYAccount yyAccount = GetAccountInformation();
            Dictionary<string, string> errors = yyAccount.CheckValidate();
            if (errors.Count > 0)
            {
                Console.WriteLine("Please fix errros below and try again.");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                return false;
            }
            else
            {
                // Lưu vào database.
                yyAccount.EncryptPassword();
                model.Save(yyAccount);
                return true;
            }
        }

        /** Xử lý đăng nhập người dùng.
         *  1. Yêu cầu người dùng nhập thông tin đăng nhập.
         *  2. Kiểm tra thông tin username người dùng vừa nhập vào.
         *  3.
        **/
        public bool Login()
        {
            // Yêu cầu nhập thông tin đăng nhập.
            Console.WriteLine("----------------LOGIN INFORMATION----------------");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            // Tìm trong database thông tin tài khoản với username vừa nhập vào.
            // Trả về null nếu không tồn tại tài khoản trùng với username trên.
            // Trong trường hợp tồn tại bản ghi thì trả về thông tin account của
            // bản ghi đấy.
            YYAccount existingAccount = model.GetByUsername(username);
            // Nếu trả về null thì hàm login trả về false.
            if (existingAccount == null)
            {
                return false;
            }

            // Nếu chạy đến đây rồi thì `existingAccount` chắc chắn khác null.
            // Tiếp tục kiểm tra thông tin password.
            // Mã hoá password người dùng vừa nhập vào kèm theo muối lấy trong database
            // của bản ghi và so sánh với password đã mã hoá trong database.
            if (!existingAccount.CheckEncryptedPassword(password))
            {
                // Nếu không trùng thì trả về false, đăng nhập thất bại.
                return false;
            }

            // Trong trường hợp chạy đến đây thì thông tin tài khoản chắc chắn khác null
            // và password đã trùng nhau. Đăng nhập thành công.
            // Lưu thông tin vừa lấy ra trong database vào biến
            // `currentLoggedInYyAccount` của lớp Program.
            Program.currentLoggedInYyAccount = existingAccount;
            // Hàm trả về true, login thành công.
            return true;
        }

        private YYAccount GetAccountInformation()
        {
            Console.WriteLine("----------------REGISTER INFORMATION----------------");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            Console.WriteLine("Confirm Password: ");
            var cpassword = Console.ReadLine();
            Console.WriteLine("Balance: ");
            var balance = Utility.GetDecimalNumber();
            Console.WriteLine("Identity Card: ");
            var identityCard = Console.ReadLine();
            Console.WriteLine("Full Name: ");
            var fullName = Console.ReadLine();
            Console.WriteLine("Birthday: ");
            var birthday = Console.ReadLine();
            Console.WriteLine("Gender (1. Male |2. Female| 3.Others): ");
            var gender = Utility.GetInt32Number();
            Console.WriteLine("Email: ");
            var email = Console.ReadLine();
            Console.WriteLine("Phone Number: ");
            var phoneNumber = Console.ReadLine();
            Console.WriteLine("Address: ");
            var address = Console.ReadLine();
            var yyAccount = new YYAccount
            {
                Username = username,
                Password = password,
                Cpassword = cpassword,
                IdentityCard = identityCard,
                Gender = gender,
                Balance = balance,
                Address = address,
                Dob = birthday,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber
            };
            return yyAccount;
        }

        public void ShowAccountInfomation()
        {
            var currentAcount = model.GetByUsername(Program.currentLoggedInYyAccount.Username);
            if (currentAcount == null)
            {
                Program.currentLoggedInYyAccount = null;
                Console.WriteLine("Sai thong tin tai khoan hoac tai khoan hoac da xoa");
                return;
            }

            Console.WriteLine("So du tai khoan: ");
            Console.WriteLine(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("So du hien tai (VND): ");
            Console.WriteLine(Program.currentLoggedInYyAccount.Balance);
        }

        /*
         * Tiến hành chuyển khoản, mặc định là ngân hàng.
         * 1. Yêu cầu nhập số tài khoản cần chuyển.
         *      1.1 Xác minh thông tin tài khoản và hiển thị người cẩn chuyển.
         * 2.Nhập số tiền cần chuyển
         *      2.1 Kiểm tra số dư tài khoản.
         * 3.Nhập nội dung chuyển tiền.
         *      3.1 Xác nhận nội dung chuyển tiền.
         * 4. Thực hiện chuyển tiền.
         *      4.1  Mở transaction. Mở block try catch
         *      4.2 Trừ tiền người gửi.
         *           4.2.1. Lấy thông tin tài khoản gửi tiền một lần nữa. Đảm bảo thông tin là mới nhất.
         *           4.2.2 Kiểm tra lại một lần nữa số dư xem có đủ tiền để chuyển không.
         *                 4.2.2.1 Nếu không đủ thì rollback.
         *                 4.2.2.2 Nếu đủ thì trả tiền và update văn bằng 'accounts'.
         *      4.3 Cộng tiền người nhận.
         *           4.3.1 Lấy thông tin tài khoản nhận, đảm bảo tài khoản không bị khóa hoặc inactive
         *                 4.3.1.1. Nếu ok thì update số tiền cho người nhận.
         *                 4.3.1.2. Nếu không ok thì rollback.
         *      4.4. Lưu lịch sử giao dịch.
         *      4.5. Kiểm tra lại trạng thái của 3 câu lệnh trên.
         *           4.5.1. Nếu cả 3 cùng thành công thì commit transaction.
         *           4.5.2. Nếu bất kỳ một câu lệnh nào bị lỗi thì rollback.
         *     4.x. Đóng, commit transaction.
         */

        public void Deposit()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to deposit: ");
            var amount = Utility.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.DEPOSIT,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.GetByAccountNumber(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
        }

        public void Withdraw()
        {
            Console.WriteLine("Withdraw.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to withdraw: ");
            var amount = Utility.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.WITHDRAW,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.GetByAccountNumber(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
        }
        public void Transfer()
        {
            Console.WriteLine("Transfer.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter recever account number.");
            var receiverAccountNumber = Console.ReadLine();
            var checkAc = model.GetByAccountNumber(receiverAccountNumber);
            Console.WriteLine("Full Name: " + checkAc.FullName);
            Console.WriteLine("Please enter amount to transfer: ");
            var amount = Utility.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.TRANSFER,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = receiverAccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateTranfers(Program.currentLoggedInYyAccount.AccountNumber, receiverAccountNumber, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.GetByAccountNumber(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
        }

        public void TransactionHistory()
        {
            List<YYTransaction> list = model.QueryTransactionHistory();
            foreach (var tran in list)
            {
                Console.WriteLine("Type: " + tran.Type + "\t\t\t Amount: " +tran.Amount
                                  + "\t\t\t Content: " + tran.Content + "\t\t\t Sender Account Number: " +tran.SenderAccountNumber);
                Console.WriteLine("Receiver Account Number: " + tran.ReceiverAccountNumber + "\t\t\t Status: " + tran.Status
                                   + "\t\t\t Created At: " + tran.CreatedAt +"\n");
            }
            DbConnection.Instance().CloseConnection();
        }
    }
}