using Microsoft.AspNetCore.Identity;
using MinecraftWrapper.Data.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MinecraftWrapper.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Bio { get; set; }


        private string _gamerTag;
        [PersonalData]
        [StringLength ( 255 )]
        public string GamerTag 
        { 
            get
            {
                return _gamerTag;
            }

            set
            {
                _gamerTag = value.Trim ();
            }
        }

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

        [NotMapped]
        public decimal? CurrentMoney => CurrencyLog?.Where ( c => c.CurrencyTypeId == CurrencyType.Normal )?.Sum ( c => c.Amount );
    }
}
