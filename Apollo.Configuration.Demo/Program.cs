﻿using Com.Ctrip.Framework.Apollo.ConfigAdapter;
using Com.Ctrip.Framework.Apollo.Logging;

using Microsoft.Extensions.Primitives;

using System;

namespace Apollo.Configuration.Demo
{
    public class Program
    {
        public static ConfigurationDemo? demo;
        private static void Main()
        {
            LogManager.UseConsoleLogging(LogLevel.Trace);

            //YamlConfigAdapter.Register();

            demo = new ConfigurationDemo();

            Console.WriteLine("Apollo Config Demo. Please input key to get the value. Input quit to exit.");


            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                input = input.Trim();
                if (input.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                {
                    Environment.Exit(0);
                }
                demo.GetConfig(input);
            }
        }
    }
}
