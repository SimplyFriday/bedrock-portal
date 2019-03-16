using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MinecraftWrapper.Models
{
    public class AuthorizedUser : IdentityUser
    {
        [Required]
        public virtual AuthorizationKey AuthorizationKey { get; set; }
        
        public string Bio { get; set; }

        [StringLength ( 255 )]
        public string GamerTag { get; set; }

        [StringLength ( 255 )]
        public string DiscordId { get; set; }


        public virtual IEnumerable<UserPreference> UserPreferences { get; set; }
    }
}
