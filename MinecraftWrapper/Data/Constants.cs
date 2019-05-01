using System;

namespace MinecraftWrapper.Data.Constants
{
    public static class SystemConstants
    {
        public const string CONTACT_US_EMAIL_ADDRESS = "mcadmin@greenlaw.software";
        public const int CLEAR_MOBS_COOLDOWN = 14400;
        public const string NO_GAMERTAG_ERROR = "Error: A GamerTag is required to use this utility.You can add your gamertag in your profile settings.";
        public const string PORTAL_CURRENCY_NAME = "PortalCurrency";
    }

    public enum UtilityRequestType
    {
        ClearMobs = 1
    }

    public enum CurrencyTransactionReason
    {
        DiscordMessage = 1,
        Purchase = 2,
        DailyLogin = 3,
        Gift = 4
    }

    public enum CurrencyType
    {
        Normal = 1,
        Gift = 2
    }

    public enum StoreItemType
    {
        Command = 1,
        Membership = 2,
        Rank = 3,
        ScheduledCommand = 4
    }
}
