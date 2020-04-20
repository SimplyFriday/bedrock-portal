using System.Collections.Generic;
using System.Net;

namespace MinecraftWrapper.Data
{
    public class ApplicationSettings
    {
        public string BdsPath { get; set; }
        public string StartDirectory { get; set; }
        public bool RestartOnFailure { get; set; }
        public int MaxOutputRetained { get; set; }
        public string SendGridApiKey { get; set; }
        public string SystemFromEmailAddress { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string DiscordUserName { get; set; }
        public string DiscordBotSecret { get; set; }
        public ConsoleRoleWhitelist [] CommandWhitelistByRole { get; set; }
        public string [] MobsToClear { get; set; }
        public string ApplicationTitle { get; set; }
        public string MinecraftCurrencyName { get; set; }
        public List<MenuLink> StaticMenuLinks{ get; set; }
        public int DailyLoginBonus { get; set; }
        public decimal PointsPerSecond { get; set; }
        public int DiscordPointCooldownInSeconds { get; set; }
        public decimal GiftPointPercentage { get; set; }
        public decimal DiscountPercentPerRank { get; set; }
        public decimal DiscountRankCap { get; set; }
        public bool MembershipEnabled { get; set; }
        public bool StoreEnabled { get; set; }
        public uint BackupsToKeep { get; set; }
        public string ArchivePath { get; set; }
        public string TempPath { get; set; }
        public string WorldName { get; set; }
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