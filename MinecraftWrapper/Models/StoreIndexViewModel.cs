using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class StoreIndexViewModel
    {
        public decimal UserCurrencyTotel { get; set; }

        public IEnumerable<StoreItem> StoreItems { get; set; }
    }
}
