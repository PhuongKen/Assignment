using System;

namespace SpringHeroBank.utility
{
    public class Utility
    {
        // Yêu cầu người dùng nhập vào một số nguyên,
        // trong trường hợp nhập sai thì yêu cầu nhập lại.       
        public static int GetInt32Number()
        {
            var number = 0;
            while (true)
            {
                try
                {
                    number = Int32.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }

            return number;
        }
        
        public static decimal GetDecimalNumber()
        {
            decimal number = 0;
            while (true)
            {
                try
                {
                    number = Decimal.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }
            return number;
        }
    }
}