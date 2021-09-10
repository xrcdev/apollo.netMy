﻿using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Internals;
using Com.Ctrip.Framework.Apollo.Spi;

using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    public static class ApolloConfigurationExtensions
    {
        /// <summary>
        /// apolloConfiguration.Get<ApolloOptions> 从配置集合中获取 到配置对象 ApolloOptions
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="apolloConfiguration"> ConfigurationRoot </param>
        /// <returns></returns>
        public static IApolloConfigurationBuilder AddApollo(this IConfigurationBuilder builder, IConfiguration apolloConfiguration) =>
            builder.AddApollo(apolloConfiguration.Get<ApolloOptions>());

        public static IApolloConfigurationBuilder AddApollo(this IConfigurationBuilder builder, string appId, string metaServer) =>
            builder.AddApollo(new ApolloOptions { AppId = appId, MetaServer = metaServer });

        public static IApolloConfigurationBuilder AddApollo(this IConfigurationBuilder builder, IApolloOptions options)
        {
            var repositoryFactory = new ConfigRepositoryFactory(options ?? throw new ArgumentNullException(nameof(options)));
#pragma warning disable 618
            ApolloConfigurationManager.SetApolloOptions(repositoryFactory);
#pragma warning restore 618
            var acb = new ApolloConfigurationBuilder(builder, repositoryFactory);
            if (options is ApolloOptions ao && ao.Namespaces != null)
                foreach (var ns in ao.Namespaces) acb.AddNamespace(ns);

            return acb;
        }
    }
}

namespace Com.Ctrip.Framework.Apollo
{
    public static class ApolloConfigurationBuilderExtensions
    {
        /// <summary>添加默认namespace: application，等价于AddNamespace(ConfigConsts.NamespaceApplication)</summary>
        public static IApolloConfigurationBuilder AddDefault(this IApolloConfigurationBuilder builder, ConfigFileFormat format = ConfigFileFormat.Properties) =>
            builder.AddNamespace(ConfigConsts.NamespaceApplication, null, format);

        /// <summary>添加其他namespace</summary>
        public static IApolloConfigurationBuilder AddNamespace(this IApolloConfigurationBuilder builder, string @namespace, ConfigFileFormat format = ConfigFileFormat.Properties) =>
            builder.AddNamespace(@namespace, null, format);

        /// <summary>添加其他namespace。如果sectionKey为null则添加到root中，可以直接读取，否则使用Configuration.GetSection(sectionKey)读取</summary>
        public static IApolloConfigurationBuilder AddNamespace(this IApolloConfigurationBuilder builder, string @namespace, string? sectionKey, ConfigFileFormat format = ConfigFileFormat.Properties)
        {
            if (string.IsNullOrWhiteSpace(@namespace)) throw new ArgumentNullException(nameof(@namespace));
            if (format < ConfigFileFormat.Properties || format > ConfigFileFormat.Txt) throw new ArgumentOutOfRangeException(nameof(format), format, $"最小值{ConfigFileFormat.Properties}，最大值{ConfigFileFormat.Txt}");

            if (format != ConfigFileFormat.Properties) @namespace += "." + format.GetString();

            var configRepository = builder.ConfigRepositoryFactory.GetConfigRepository(@namespace);
            var previous = builder.Sources.FirstOrDefault(source =>
                source is ApolloConfigurationProvider apollo &&
                apollo.SectionKey == sectionKey &&
                apollo.ConfigRepository == configRepository);
            if (previous != null)
            {
                builder.Sources.Remove(previous);
                builder.Sources.Add(previous);
            }
            else
            {
                builder.Add(new ApolloConfigurationProvider(sectionKey, configRepository));
#pragma warning disable 618
                ApolloConfigurationManager.Manager.Registry.Register(@namespace, new DefaultConfigFactory(builder.ConfigRepositoryFactory));
#pragma warning restore 618
            }

            return builder;
        }
    }
}
