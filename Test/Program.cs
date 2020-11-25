using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        private List<string> values = new List<string>();

        private static async Task Main(string[] args)
        {
            await new Program().ProcessAllAsync();
        }

        private async Task ProcessAllAsync()
        {
            List<int> numbers = Enumerable.Range(0, 5).ToList();
            List<Task> tasks = new List<Task>();

            foreach (int number in numbers)
            {
                tasks.Add(ProcessAsync(number));
            }

            lock (values)
            {
                values.Add("hello");
                foreach (string value in values)
                {
                    Task.Delay(5000).GetAwaiter().GetResult();
                }
            }
            Console.WriteLine("Unlocked");

            await Task.WhenAll(tasks);

            Console.WriteLine("Finished");

            foreach (string value in values)
            {
                Console.WriteLine(value);
            }
        }

        private async Task ProcessAsync(int number)
        {
            Console.WriteLine($"{number} started");
            await Task.Delay(1000);
            values.Add($"{number} added by task");
            Console.WriteLine($"{number} finished");
        }
    }
}
