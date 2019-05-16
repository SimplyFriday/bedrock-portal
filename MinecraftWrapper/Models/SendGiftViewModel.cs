using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class SendGiftViewModel
    {
        [Required]
        public string GamerTag { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public decimal CurrentGiftCurrency { get; set; }
    }
}
