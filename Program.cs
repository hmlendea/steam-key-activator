using System;
using System.IO;
using System.Security.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Core;
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
        static CacheSettings cacheSettings;
        static DebugSettings debugSettings;
        static ProductKeyManagerSettings productKeyManagerSettings;

        static IWebDriver webDriver;
        static ILogger logger;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
            PrepareCache();
            SetupDriver();

            serviceProvider = CreateIOC();
            logger = serviceProvider.GetService<ILogger>();

            logger.Info(Operation.StartUp, "Application started");

            try
            {
                RunApplication();
            }
            catch (AuthenticationException) { }
            catch (AggregateException ex)
            {
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
            }
            finally
            {
                if (!(webDriver is null))
                {
                    webDriver.Quit();
                }

                logger.Info(Operation.ShutDown, "Application stopped");
            }
        }

        static void RunApplication()
        {
            IKeyActivator keyActivator = serviceProvider.GetService<IKeyActivator>();
            keyActivator.ActivateRandomPkmKey();

            webDriver.Quit();
        }
        
        static IConfiguration LoadConfiguration()
        {
            botSettings = new BotSettings();
            cacheSettings = new CacheSettings();
            debugSettings = new DebugSettings();
            productKeyManagerSettings = new ProductKeyManagerSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(CacheSettings), cacheSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(ProductKeyManagerSettings), productKeyManagerSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(cacheSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(productKeyManagerSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IHmacEncoder<GetProductKeyRequest>, GetProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<UpdateProductKeyRequest>, UpdateProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<ProductKeyResponse>, ProductKeyResponseEncoder>()
                .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
                .AddSingleton<IWebDriver>(s => webDriver)
                .AddSingleton<IWebProcessor, WebProcessor>()
                .AddSingleton<IKeyHandler, KeyUpdater>()
                .AddSingleton<IKeyActivator, KeyActivator>()
                .BuildServiceProvider();
        }

        static void SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddExcludedArgument("--enable-loggin");
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
			options.AddArgument("--disable-logging");

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

            webDriver = new ChromeDriver(service, options, TimeSpan.FromSeconds(botSettings.PageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)webDriver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                webDriver.Quit();
                webDriver = new ChromeDriver(service, options);
            }

            webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(botSettings.PageLoadTimeout);
            webDriver.Manage().Window.Maximize();
        }

        static void PrepareCache()
        {
            if (string.IsNullOrWhiteSpace(cacheSettings.CacheDirectoryPath))
            {
                throw new DirectoryNotFoundException("The cache directory path is invalid");
            }

            if (!Directory.Exists(cacheSettings.CacheDirectoryPath))
            {
                Directory.CreateDirectory(cacheSettings.CacheDirectoryPath);
            }
        }
    }
}
