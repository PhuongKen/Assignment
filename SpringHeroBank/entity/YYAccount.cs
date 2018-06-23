using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SpringHeroBank.model;
using SpringHeroBank.utility;

namespace SpringHeroBank.entity
{
    public enum ActiveStatus
    {
        INACTIVE = 0,
        ACTIVE = 1,
        LOCKED = 2
    }
    
    public class YYAccount
    {
        private string _accountNumber; // id
        private string _username; // unique
        private string _password;
        private string _cpassword;
        private string _salt;
        private decimal _balance;
        private string _identityCard; // unique
        private string _fullName;
        private string _email; // unique
        private string _phoneNumber; // unique
        private string _address;
        private string _dob;
        private int _gender; // 1. male | 2. female | 3. rather not say.
        private string _createdAt;
        private string _updatedAt;
        private ActiveStatus _status; // 1. active | 2. locked | 0. inactive.

        public YYAccount()
        {
            GenerateAccountNumber();
            GenerateSalt();
        }

        // Tham số là chuỗi password chưa mã hoá mà người dùng nhập vào.
        public bool CheckEncryptedPassword(string password) 
        {         
            // Tiến hành mã hoá password người dùng nhập vào kèm theo muối được lấy từ db.
            // Trả về một chuỗi password đã mã hoá.            
            var checkPassword = Hash.EncryptedString(password, _salt);
            // So sánh hai chuỗi password đã mã hoá. Nếu trùng nhau thì trả về true.
            // Nếu không trùng nhau thì trả về false.
            return (checkPassword == _password);
        }        
        public void EncryptPassword()
        {
            if (string.IsNullOrEmpty(_password))
            {
                throw new ArgumentNullException("Password is null or empyt.");
            }
            _password = Hash.EncryptedString(_password, _salt);
        }

        private void GenerateAccountNumber()
        {
            _accountNumber = Guid.NewGuid().ToString(); // unique
        }

        private void GenerateSalt()
        {
            _salt = Guid.NewGuid().ToString().Substring(0, 7); // !unique
        }

        public string Cpassword
        {
            get => _cpassword;
            set => _cpassword = value;
        }

        public string Salt
        {
            get => _salt;
            set => _salt = value;
        }

        public string FullName
        {
            get => _fullName;
            set => _fullName = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string AccountNumber
        {
            get => _accountNumber;
            set => _accountNumber = value;
        }

        public decimal Balance
        {
            get => _balance;
            set => _balance = value;
        }

        public string IdentityCard
        {
            get => _identityCard;
            set => _identityCard = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value;
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
        }

        public string Address
        {
            get => _address;
            set => _address = value;
        }

        public string Dob
        {
            get => _dob;
            set => _dob = value;
        }

        public int Gender
        {
            get => _gender;
            set => _gender = value;
        }

        public string CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        public string UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = value;
        }

        public ActiveStatus Status
        {
            get => _status;
            set => _status = value;
        }

        // Làm nhiệm vụ validate account, trả về một dictionary các lỗi.
        public Dictionary<string, string> CheckValidate()
        {
            YYAccountModel model = new YYAccountModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(this._username))
            {
                errors.Add("username", "Username can not be null or empty.");
            } else if (this._username.Length < 6)
            {
                errors.Add("username", "Username is too short. At least 6 characters.");
            }else if (model.CheckExistUsername(this._username))
            {
                // Check trùng username.
                errors.Add("username", "Username is exist. Please try another one.");
            }
            if (_cpassword != _password)
            {
                errors.Add("password", "Confirm password does not match.");
            }

            // if else if else ...
            return errors;
        }
    }
}