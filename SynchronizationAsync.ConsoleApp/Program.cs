using System;
using System.Threading.Tasks;

namespace SynchronizationAsync.ConsoleApp
{
    class Program
    {
        static ManualResetEventAsync mre = new ManualResetEventAsync(false);
        static void Main(string[] args)
        {
            Go();
            Console.ReadLine();
        }

        static async void Go()
        {
            for (int i = 0; i < 100; i++)
            {
                var k = i;
                Task.Run(async () =>
                {
                    await mre.WaitOneAsync();
                    Console.WriteLine($"Task id:{k}");
                });
            }

            await Task.Delay(new TimeSpan(0, 0, 1));

            mre.Set();

            mre.Reset();

            for (int i = 0; i < 100; i++)
            {
                var k = i;
                Task.Run(async () =>
                {
                    await mre.WaitOneAsync();
                    Console.WriteLine($"Task id:{k}");

                });
            }

            await Task.Delay(new TimeSpan(0, 0, 1));

            mre.Set();
        }
    }
}