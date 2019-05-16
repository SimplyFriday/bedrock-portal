using MinecraftWrapper.Data.Entities;
using System.Collections.Generic;

namespace MinecraftWrapper.Models
{
    public class GiftCurrencyViewModel
    {
        public decimal GiftCurrancy { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}
