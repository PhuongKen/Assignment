using System;

namespace SpringHeroBank.error
{
    public class SpringHeroTransactionException: Exception
    {
        public SpringHeroTransactionException(string message) : base(message)
        {
        }
    }
}