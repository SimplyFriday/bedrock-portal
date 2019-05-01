using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Bio { get; set; }

        [PersonalData]
        [StringLength ( 255 )]
        public string GamerTag { get; set; }

        [PersonalData]
        [StringLength ( 255 )]
        public string DiscordId { get; set; }

        [PersonalData]
        public ulong? Xuid { get; set; }

        public DateTime? LastMinecraftLogin { get; set; }

        public DateTime? LastLoginReward { get; set; }
        public DateTime? MembershipExpirationTime { get; set; }
        public bool IsActive { get; set; }
        public int Rank { get; set; }

        public virtual IEnumerable<UserPreference> UserPreferences { get; set; }
        public virtual IEnumerable<UserCurrency> CurrencyLog { get; internal set; }
    }
}
