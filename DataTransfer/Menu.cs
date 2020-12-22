using System;

namespace DataTransfer
{
    internal class Menu
    {
        public static int PickOption(params string[] options)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Enter a number\n\n");

                for (int i = 1; i <= options.Length; i++)
                {
                    Console.WriteLine($"{i} - {options[i - 1]}");
                }

                string input = Console.ReadLine();
                if (int.TryParse(input, out int option) && option > 0 && option <= options.Length)
                {
                    return option - 1;
                }
            }
        }

        public static ulong GetUlong(string type)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Enter a {type} id\n\n");

                string input = Console.ReadLine();
                if (ulong.TryParse(input, out ulong id))
                {
                    return id;
                }
            }
        }

        public static string GetString(string type)
        {
            Console.Clear();
            Console.WriteLine($"Enter a {type} string\n\n");

            return Console.ReadLine();
        }

        public static void Continue()
        {
            Console.WriteLine($"\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
