using System;
using System.IO;
using System.Security.Authentication;

using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class SteamAuthenticator : ISteamAuthenticator
    {
        const string LoginUrl = "https://store.steampowered.com/login/?redir=&redir_ssl=1";

        readonly IWebProcessor webProcessor;
        readonly BotSettings botSettings;
        readonly CacheSettings cacheSettings;
        readonly ILogger logger;

        public SteamAuthenticator(
            IWebProcessor webProcessor,
            BotSettings botSettings,
            CacheSettings cacheSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.botSettings = botSettings;
            this.cacheSettings = cacheSettings;
            this.logger = logger;
        }

        public void LogIn()
        {
            logger.Info(
                MyOperation.SteamLogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));

            webProcessor.GoToUrl(LoginUrl);

            By avatarSelector = By.XPath("//a[contains(@class,'user_avatar')]");

            if (webProcessor.IsElementVisible(avatarSelector))
            {
                logger.Info(
                    MyOperation.SteamLogIn,
                    OperationStatus.Success,
                    "Already logged in",
                    new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));
                
                return;
            }

            PerformLogIn();
            ValidateLogInResult();

            logger.Debug(
                MyOperation.SteamLogIn,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));
        }

        void PerformLogIn()
        {
            By usernameInputSelector = By.Id("input_username");
            By passwordInputSelector = By.Id("input_password");
            By captchaInputSelector = By.Id("input_captcha");
            By rememberLoginChecboxSelector = By.Id("remember_login");
            By logInButtonSelector = By.XPath(@"//*[@id='login_btn_signin']/button");
            
            if (webProcessor.IsElementVisible(captchaInputSelector))
            {
                ThrowLogInException("Captcha input required");
            }

            webProcessor.SetText(usernameInputSelector, botSettings.SteamUsername);
            webProcessor.SetText(passwordInputSelector, botSettings.SteamPassword);

            if (webProcessor.IsElementVisible(rememberLoginChecboxSelector))
            {
                webProcessor.UpdateCheckbox(rememberLoginChecboxSelector, true);
            }
            
            webProcessor.Click(logInButtonSelector);

            InputSteamGuardCodeIfRequired();
        }

        void InputSteamGuardCodeIfRequired()
        {
            By avatarSelector = By.XPath("//a[contains(@class,'user_avatar')]");
            By steamGuardCodeInputSelector = By.Id("twofactorcode_entry");
            By steamGuardSubmitButtonSelector = By.XPath("//*[@id='login_twofactorauth_buttonset_entercode']/div[1]");

            webProcessor.WaitForAnyElementToBeVisible(steamGuardCodeInputSelector, avatarSelector);

            if (webProcessor.IsElementVisible(avatarSelector))
            {
                return;
            }

            ValidateSteamGuardCode();

            webProcessor.SetText(steamGuardCodeInputSelector, botSettings.SteamGuardCode);
            webProcessor.Click(steamGuardSubmitButtonSelector);

            SaveLastUsedSgCode();
        }

        void ValidateSteamGuardCode()
        {
            string lastUsedCode = File
                .ReadAllText(cacheSettings.LastSteamGuardCodeFilePath)
                .ToUpperInvariant()
                .Trim();

            if (lastUsedCode.Equals(botSettings.SteamGuardCode, StringComparison.InvariantCultureIgnoreCase))
            {
                ThrowLogInException("The configured SteamGuard code is outdated");
            }
        }

        void ValidateLogInResult()
        {
            By avatarSelector = By.XPath("//a[contains(@class,'user_avatar')]");
            By steamGuardIncorrectMessageSelector = By.Id("login_twofactorauth_message_incorrectcode");

            webProcessor.WaitForAnyElementToBeVisible(avatarSelector, steamGuardIncorrectMessageSelector);

            if (webProcessor.IsElementVisible(steamGuardIncorrectMessageSelector))
            {
                ThrowLogInException("The provided SteamGuard code is not valid");
            }
            else if (!webProcessor.IsElementVisible(avatarSelector))
            {
                ThrowLogInException("Authentication failure");
            }
        }

        void ThrowLogInException(string message)
        {
            Exception ex = new AuthenticationException(message);

            logger.Error(
                MyOperation.SteamLogIn,
                OperationStatus.Failure,
                ex,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));

            throw ex;
        }

        void SaveLastUsedSgCode()
        {
            string steamGuardCode = botSettings.SteamGuardCode
                .ToUpperInvariant()
                .Trim();
            
            File.WriteAllText(cacheSettings.LastSteamGuardCodeFilePath, steamGuardCode);
        }
    }
}
