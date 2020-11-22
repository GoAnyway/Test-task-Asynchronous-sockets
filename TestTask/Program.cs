using System;
using System.Linq;
using System.Text;
using TestTask.EventArguments;

namespace TestTask
{
    public class Program
    {
        public static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var solver = new Solver();
            solver.AllDataReceived += Solver_AllDataReceived;
            solver.Start();
            Console.ReadLine();
        }

        private static void Solver_AllDataReceived(object sender, AllDataReceivedEventArgs e)
        {
            var values = e.Values.OrderBy(_ => _).ToList();
            var length = values.Count;
            var median = length % 2 == 0 ? (values[length / 2] + values[length / 2 - 1]) / 2d : values[length / 2];
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The answer is: {median}");
            Console.ResetColor();
        }
    }
}