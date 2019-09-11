using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciSecurity.HMAC;
using NuciWeb;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using SteamKeyActivator.Client;
using SteamKeyActivator.Client.Models;
using SteamKeyActivator.Client.Security;
using SteamKeyActivator.Configuration;
using SteamKeyActivator.Service;

namespace SteamKeyActivator
{
    public sealed class Program
    {
        static BotSettings botSettings;
        static DebugSettings debugSettings;
        static ProductKeyManagerSettings productKeyManagerSettings;

        static IWebDriver webDriver;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
            webDriver = SetupDriver();

            serviceProvider = CreateIOC();

            IKeyActivator keyActivator = serviceProvider.GetService<IKeyActivator>();
            keyActivator.ActivateRandomPkmKey();

            webDriver.Quit();
            Console.WriteLine("Hello World!");
        }
        
        static IConfiguration LoadConfiguration()
        {
            botSettings = new BotSettings();
            debugSettings = new DebugSettings();
            productKeyManagerSettings = new ProductKeyManagerSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(ProductKeyManagerSettings), productKeyManagerSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(productKeyManagerSettings)
                .AddSingleton<IHmacEncoder<GetProductKeyRequest>, GetProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<UpdateProductKeyRequest>, UpdateProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<ProductKeyResponse>, ProductKeyResponseEncoder>()
                .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
                .AddSingleton<IWebDriver>(s => webDriver)
                .AddSingleton<IWebProcessor, WebProcessor>()
                .AddSingleton<IKeyActivator, KeyActivator>()
                .BuildServiceProvider();
        }

        static IWebDriver SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");

            if (debugSettings.IsHeadless)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(botSettings.PageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                driver.Quit();
                driver = new ChromeDriver(service, options);
            }

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(botSettings.PageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
