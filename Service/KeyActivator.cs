using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator : IKeyActivator
    {
        static string HomePageUrl => "https://store.steampowered.com";
        public string LoginUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";

        readonly IWebProcessor webProcessor;
        readonly IWebDriver webDriver;
        readonly BotSettings botSettings;
        readonly ILogger logger;

        public KeyActivator(
            IWebProcessor webProcessor,
            IWebDriver webDriver,
            BotSettings botSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.webDriver = webDriver;
            this.botSettings = botSettings;
            this.logger = logger;
        }

        public void ActivateRandomPkmKey()
        {
            bool isLogInRequired = CheckIfLogInIsRequired();
            
            if (isLogInRequired)
            {
                LogIn();
            }
        }

        bool CheckIfLogInIsRequired()
        {
            webProcessor.GoToUrl("https://steamcommunity.com/id/Agractifi/");

            webProcessor.Wait(5000);

            By notificationAreaSelector = By.Id("header_notification_area");

            if (webProcessor.IsElementVisible(notificationAreaSelector))
            {
                return false;
            }

            return true;
        }

        void LogIn()
        {
            logger.Info(
                MyOperation.SteamLogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));

            webProcessor.GoToUrl(LoginUrl);

            By usernameInputSelector = By.Id("input_username");
            By passwordInputSelector = By.Id("input_password");
            By steamGuardCodeInputSelector = By.Id("twofactorcode_entry");
            By steamGuardSubmitButtonSelector = By.XPath("//*[@id='login_twofactorauth_buttonset_entercode']/div[1]");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");

            webProcessor.SetText(usernameInputSelector, botSettings.SteamUsername);
            webProcessor.SetText(passwordInputSelector, botSettings.SteamPassword);
            
            webProcessor.Click(By.XPath(@"//*[@id='login_btn_signin']/button"));
            webProcessor.WaitForAnyElementToBeVisible(steamGuardCodeInputSelector, avatarSelector);

            if (webProcessor.IsElementVisible(steamGuardCodeInputSelector))
            {
                webProcessor.SetText(steamGuardCodeInputSelector, botSettings.SteamGuardCode);
                webProcessor.Click(steamGuardSubmitButtonSelector);
            }

            webProcessor.WaitForElementToBeVisible(avatarSelector);
            webProcessor.Wait(2000000);

            logger.Debug(
                MyOperation.SteamLogIn,
                OperationStatus.Success);
        }
    }
}