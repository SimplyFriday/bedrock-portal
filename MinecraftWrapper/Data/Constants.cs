using System;

namespace MinecraftWrapper.Data.Constants
{
    public static class SystemConstants
    {
        public const string CONTACT_US_EMAIL_ADDRESS = "mcadmin@greenlaw.software";
        public const int CLEAR_MOBS_COOLDOWN = 14400;
        public const string NO_GAMERTAG_ERROR = "Error: A GamerTag is required to use this utility.You can add your gamertag in your profile settings.";
    }

    public enum UtilityRequestType
    {
        ClearMobs = 1
    }
}
