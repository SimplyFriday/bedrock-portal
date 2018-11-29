using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MinecraftWrapper.Models
{
    public class AdditionalUserData
    {
        [Key]
        public Guid AdditionalUserDataId { get; set; }

        public string Bio { get; set; }

        public string GamerTag { get; set; }

        [Required]
        [ForeignKey("User")]
        public virtual string UserId { get; set; }

        public virtual IdentityUser User { get; set; }
    }
}
