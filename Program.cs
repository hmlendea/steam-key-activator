﻿using System;
using System.Security.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciSecurity.HMAC;
using NuciWeb;
using NuciWeb.Steam;

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
        static NuciLoggerSettings loggerSettings;

        static IWebDriver webDriver;
        static ILogger logger;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
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
                LogInnerExceptions(ex);
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

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(botSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(productKeyManagerSettings)
                .AddSingleton(loggerSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IHmacEncoder<GetProductKeyRequest>, GetProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<UpdateProductKeyRequest>, UpdateProductKeyRequestEncoder>()
                .AddSingleton<IHmacEncoder<ProductKeyResponse>, ProductKeyResponseEncoder>()
                .AddSingleton<IProductKeyManagerClient, ProductKeyManagerClient>()
                .AddSingleton<IWebDriver>(s => webDriver)
                .AddSingleton<IWebProcessor, WebProcessor>()
                .AddSingleton<ISteamProcessor, SteamProcessor>()
                .AddSingleton<IKeyHandler, KeyUpdater>()
                .AddSingleton<IKeyActivator, KeyActivator>()
                .BuildServiceProvider();
        }

        static void SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddExcludedArgument("--enable-logging");
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
			options.AddArgument("--disable-logging");

            if (debugSettings.IsHeadless)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1366,768");
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

        static void LogInnerExceptions(AggregateException exception)
        {
            foreach (Exception innerException in exception.InnerExceptions)
            {
                AggregateException innerAggregateException = innerException as AggregateException;

                if (innerAggregateException is null)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                }
                else
                {
                    LogInnerExceptions(innerException as AggregateException);
                }
            }
        }
    }
}
