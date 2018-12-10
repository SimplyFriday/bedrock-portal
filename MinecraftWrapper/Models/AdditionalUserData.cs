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
        
        [StringLength(255)]
        public string GamerTag { get; set; }

        public string MinecraftId { get; set; }

        [Required]
        [ForeignKey("User")]
        public virtual string UserId { get; set; }

        public virtual AuthorizedUser User { get; set; }
    }
}
