using System;
using System.Security.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciWeb;
using NuciWeb.Steam;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using SteamKeyActivator.Client;
using SteamKeyActivator.Configuration;
using SteamKeyActivator.Service;

namespace SteamKeyActivator
{
    public sealed class Program
    {
        static BotSettings botSettings;
        static DebugSettings debugSettings;
        static ProductKeyManagerSettings productKeyManagerSettings;
        static NuciLoggerSettings loggerSettings;

        static IWebDriver webDriver;
        static ILogger logger;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();

            webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(debugSettings.IsDebugMode);

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
                LogInnerExceptions(ex);
            }
            catch (Exception ex)
            {
                logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
            }
            finally
            {
                webDriver?.Quit();

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
            debugSettings = new DebugSettings();
            productKeyManagerSettings = new ProductKeyManagerSettings();
            loggerSettings = new NuciLoggerSettings();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(BotSettings), botSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(ProductKeyManagerSettings), productKeyManagerSettings);
            config.Bind(nameof(NuciLoggerSettings), loggerSettings);

            return config;
        }

        static IServiceProvider CreateIOC() => new ServiceCollection()
            .AddSingleton(botSettings)
            .AddSingleton(debugSettings)
            .AddSingleton(productKeyManagerSettings)
            .AddSingleton(loggerSettings)
            .AddSingleton<ILogger, NuciLogger>()
            .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
            .AddSingleton(s => webDriver)
            .AddSingleton<IWebProcessor, WebProcessor>()
            .AddSingleton<ISteamProcessor, SteamProcessor>()
            .AddSingleton<IKeyHandler, KeyUpdater>()
            .AddSingleton<IKeyActivator, KeyActivator>()
            .BuildServiceProvider();

        static void LogInnerExceptions(AggregateException exception)
        {
            foreach (Exception innerException in exception.InnerExceptions)
            {
                if (innerException is not AggregateException innerAggregateException)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                }
                else
                {
                    LogInnerExceptions(innerAggregateException);
                }
            }
        }
    }
}
