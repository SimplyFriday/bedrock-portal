using System.Collections.Generic;
using System.Net;

namespace MinecraftWrapper.Data
{
    public class ApplicationSettings
    {
        //string exePath, string startDirectory, bool restartOnFailure, int maxOutputRetained
        public string ExePath { get; set; }
        public string StartDirectory { get; set; }
        public bool RestartOnFailure { get; set; }
        public int MaxOutputRetained { get; set; }
        public string SendGridApiKey { get; set; }
        public string SystemFromEmailAddress { get; set; }
        public string WhiteListPath { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string DiscordUserName { get; set; }
        public ConsoleRoleWhitelist [] CommandWhitelistByRole { get; set; }
        public string [] MobsToClear { get; set; }
        public string ApplicationTitle { get; set; }

        public List<MenuLink> StaticMenuLinks{ get; set; }
    }

    public class MenuLink
    {
        public string Display { get; set; }
        public string FileName { get; set; }
    }

    public class ConsoleRoleWhitelist
    {
        public string RoleName { get; set; }
        public string[] Commands { get; set; }
    }
}