using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Bio { get; set; }

        [StringLength ( 255 )]
        public string GamerTag { get; set; }

        [StringLength ( 255 )]
        public string DiscordId { get; set; }

        public ulong xuid { get; set; }

        public virtual IEnumerable<UserPreference> UserPreferences { get; set; }
    }
}
