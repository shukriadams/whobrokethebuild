using System;

namespace MessageQueue
{
    public class Program
    {
        public static bool Persist { get;set;}
        
        public static bool Verbose { get; set; }

        static void Main(string[] args)
        {
            CommandLineSwitches switches = new CommandLineSwitches(args);
            Persist = switches.Contains("persist");
            Verbose = switches.Contains("verbose");

            HttpServer server = new HttpServer() { Verbose = Verbose };
            server.Start();

            Console.WriteLine("WBTB MessageQueue started ...");
            if (Persist)
                Console.WriteLine("Persist mode enabled");
            if (Verbose)
                Console.WriteLine("Verbose mode enabled");

        }
    }
}
