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

        public virtual AdditionalUserData AdditionalUserData { get; set; }

        public virtual IEnumerable<UserPreference> UserPreferences { get; set; }
    }
}
