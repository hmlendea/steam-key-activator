using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciSecurity.HMAC;

using SteamKeyActivator.Client;
using SteamKeyActivator.Client.Models;
using SteamKeyActivator.Client.Security;
using SteamKeyActivator.Configuration;

namespace SteamKeyActivator
{
    public sealed class Program
    {
        static DebugSettings debugSettings;
        static ProductKeyManagerSettings productKeyManagerSettings;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
            serviceProvider = CreateIOC();

            Console.WriteLine("Hello World!");
        }
        
        static IConfiguration LoadConfiguration()
        {
            debugSettings = new DebugSettings();
            productKeyManagerSettings = new ProductKeyManagerSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(ProductKeyManagerSettings), productKeyManagerSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(debugSettings)
                .AddSingleton(productKeyManagerSettings)
                .AddSingleton<IHmacEncoder<GetProductKeyRequest>, GetProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<UpdateProductKeyRequest>, UpdateProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<ProductKeyResponse>, ProductKeyResponseEncoder>()
                .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
                .BuildServiceProvider();
        }
    }
}
