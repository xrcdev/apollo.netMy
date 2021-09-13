﻿using Com.Ctrip.Framework.Apollo;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using System;

namespace Apollo.Configuration.Demo
{
    public class ConfigurationDemo
    {
        private const string DefaultValue = "undefined";
        public readonly IConfiguration _config;
        public readonly ConfigurationRoot? configRoot;
        private readonly IConfiguration _anotherConfig;

        public ConfigurationDemo()
        {
            var host = Host.CreateDefaultBuilder()

                   //.AddApollo()
                   .ConfigureAppConfiguration((hostingContext, builder) =>
                   {
                       //var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                       //builder.AddXmlFile(!isWindows ? "/CommonConfig/CommonConfig.xml" : "C:\\CommonConfig\\CommonConfig.xml");
                       //if (File.Exists(!isWindows ? "/CommonConfig/HtCommonConfig_Core.xml" : "C:\\CommonConfig\\HtCommonConfig_Core.xml"))
                       //{
                       //    builder.AddXmlFile(!isWindows ? "/CommonConfig/HtCommonConfig_Core.xml" : "C:\\CommonConfig\\HtCommonConfig_Core.xml");
                       //}
                       var acb = builder.AddApollo(builder.Build().GetSection("apollo"));

                       //acb.ConfigRepositoryFactory.GetConfigRepository("").AddChangeListener();
                       //.AddDefault();
                       //foreach (var item in acb.Sources)
                       //{
                       //    if (item is ApolloConfigurationProvider aProvider)
                       //    {
                       //        //aProvider.con
                       //    }
                       //}

                   })
                  .ConfigureServices((context, services) =>
                  {
                      services.AddOptions()
                          .Configure<Value>(context.Configuration)
                          .Configure<Value>("other", context.Configuration.GetSection("a"));
#pragma warning disable 618
                      services.AddSingleton<ApolloConfigurationManager>();
#pragma warning restore 618
                  })
                  .Build();

            _config = host.Services.GetRequiredService<IConfiguration>();
            configRoot = _config as ConfigurationRoot;

            ChangeToken.OnChange(() => _config.GetReloadToken(), () =>
            {
                Console.WriteLine($"★this._config:{_config.GetHashCode()}");
                Console.WriteLine($"★{_config.GetReloadToken().GetHashCode()}");
            });
            _anotherConfig = _config.GetSection("a");

            var optionsMonitor = host.Services.GetService<IOptionsMonitor<Value>>();

            optionsMonitor.OnChange(OnChanged);

            //new ConfigurationManagerDemo( host.Services.GetService<ApolloConfigurationManager>());
        }


        public string GetConfig(string key)
        {
            var result = _config.GetValue(key, DefaultValue);
            if (result.Equals(DefaultValue))
            {
                result = _anotherConfig.GetValue(key, DefaultValue);
            }
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Loading key: {0} with value: {1}", key, result);
            Console.ForegroundColor = color;

            return result;
        }

        private static void OnChanged(Value value, string name)
        {
            Console.WriteLine(name + " has changed: " + JsonConvert.SerializeObject(value));
        }

        private class Value
        {
            public string Timeout { get; set; } = "";
        }
    }
}
