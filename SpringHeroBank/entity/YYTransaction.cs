namespace SpringHeroBank.entity
{
    public class YYTransaction
    {
        
        public enum ActiveStatus
        {
            PROCESSING = 1,
            DONE = 2,
            REJECT = 0,
            DELETED = -1,
        }

        public enum TransactionType
        {
            DEPOSIT = 1,
            WITHDRAW = 2,
            TRANSFER = 3
        }
        
        private string _id;
        private decimal _amount;
        private string _content;
        private string _senderAccountNumber;
        private string _receiverAccountNumber;
        private TransactionType _type; // 1. withdraw | 2. deposit | 3. transfer.
        private string _createdAt;
        private ActiveStatus _status; // 0. deleted | 1. success | 2.processing.

        public YYTransaction()
        {
        }


        public YYTransaction(string id, decimal amount, string content, string senderAccountNumber, string receiverAccountNumber, TransactionType type, string createdAt, ActiveStatus status)
        {
            _id = id;
            _amount = amount;
            _content = content;
            _senderAccountNumber = senderAccountNumber;
            _receiverAccountNumber = receiverAccountNumber;
            _type = type;
            _createdAt = createdAt;
            _status = status;
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public decimal Amount
        {
            get => _amount;
            set => _amount = value;
        }

        public string Content
        {
            get => _content;
            set => _content = value;
        }

        public string SenderAccountNumber
        {
            get => _senderAccountNumber;
            set => _senderAccountNumber = value;
        }

        public string ReceiverAccountNumber
        {
            get => _receiverAccountNumber;
            set => _receiverAccountNumber = value;
        }

        public TransactionType Type
        {
            get => _type;
            set => _type = value;
        }

        public string CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        public ActiveStatus Status
        {
            get => _status;
            set => _status = value;
        }
    }
}