using System;
using SpringHeroBank.entity;
using SpringHeroBank.view;
using SpringHeroBank.entity;

namespace SpringHeroBank
{
    class Program
    {
        public static YYAccount currentLoggedInYyAccount;

        static void Main(string[] args)
        {
            ApplicationView view = new ApplicationView();
            while (true)
            {
                if (Program.currentLoggedInYyAccount != null)
                {
                    view.GenerateCustomerMenu();
                }
                else
                {
                    view.GenerateDefaultMenu();
                }
            }
        }
        
    }
}