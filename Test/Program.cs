using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        private List<string> _values = new List<string>();

        private static async Task Main()
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

            lock (_values)
            {
                _values.Add("hello");
                foreach (string value in _values)
                {
                    Task.Delay(5000).GetAwaiter().GetResult();
                }
            }
            Console.WriteLine("Unlocked");

            await Task.WhenAll(tasks);

            Console.WriteLine("Finished");

            foreach (string value in _values)
            {
                Console.WriteLine(value);
            }
        }

        private async Task ProcessAsync(int number)
        {
            Console.WriteLine($"{number} started");
            await Task.Delay(1000);
            _values.Add($"{number} added by task");
            Console.WriteLine($"{number} finished");
        }
    }
}
