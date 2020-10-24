using System;

namespace Test
{
    class Program
    {
        private static Feature _feature;

        static void Main(string[] args)
        {
            Random random = new Random();
            for (int i = 0; i < 50; i++)
            {
                int channels = 2;
                int purgeNumber = i;

                Console.WriteLine(
                    $"Channels: {channels}, Purge n: {purgeNumber}, Channel selected: {Wrap(purgeNumber, channels)}");
            }
        }

        static int Wrap(int value, int maxValue)
        {
            if (maxValue == 0) return 0;
            value = value % maxValue;

            return value;
        }
    }
}
