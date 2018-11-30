using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MinecraftWrapper.Models
{
    public class AuthorizationKey
    {
        [Key]
        public Guid AuthorizationKeyId { get; set; }

        [Required]
        [StringLength(maximumLength:16, MinimumLength = 16)]
        public string AuthorizationToken { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual AuthorizedUser User { get; set; }
    }
}
