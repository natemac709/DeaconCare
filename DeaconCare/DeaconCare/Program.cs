using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeaconCare
{
    class Program
    {
        static void Main(string[] args)
        {
            //#region: RegionOne
            /*
            Console.WriteLine("Play a game?");
            Console.Write("Pick a number: 1, 2, or 3: ");
            string userValue = Console.ReadLine();
            string myMessage = "";

            string message = (userValue == "1") ? "boat" : "strand of lint";

            Console.WriteLine("You picked {0}. You won a {1}", userValue, myMessage);

            Console.ReadLine();
            */
            /*
            int numArrayElements = 5;
            int[] arrayNums = new int[numArrayElements];
            for (int i = 1; i < numArrayElements; i++)
            {
                arrayNums[i] = i + 1;
            }
            Console.WriteLine(arrayNums.Length);
            Console.ReadLine();
            */
            /* #region: While Iteration
            string firstName, lastName, city;
            string results;

            Console.Write("Results: " + results);

            DisplayResult(ReverseString(firstName), 
                ReverseString(lastName), 
                ReverseString(city) );

            Console.WriteLine();
            Console.ReadLine();

            public static void DisplayResult(
                string reversedFirstName,
                string reversedLastName,
                string reversedCity
                ){
                Console.Write("Results: ");
                Console.Write(String.Format("{0} {1} {2} ",
                    reversedFirstName,
                    reversedLastName,
                    reversedCity));
            }
            public static void DisplayResult(string message)
            {
                Console.Write("Results: ");
                Console.Write(String.Format("{0} {1} {2} ",
                    reversedFirstName,
                    reversedLastName,
                    reversedCity));
            }
            public static string ReverseString(string message)
            {
                char[] messageArray = message.ToCharArray();
                Array.Reverse(messageArray);
                return String.Concat(messageArray);
            }

            private static void MainMenu()
            {
                Console.WriteLine("Choose an option: ");
                Console.WriteLine("1");

            }

            private static void PrintNumbers()
            {
                Console.WriteLine("Print numbers!");
                Console.Write("Type a number: ");
                int result = int.Parse(Console.ReadLine());
                int counter = 1;
                while()
            }

    */
    }
    }
}
