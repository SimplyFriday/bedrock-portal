using MinecraftWrapper.Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class AddCurrencyToUserModel
    {
        public decimal Amount { get; set; }
        public string DiscordId { get; set; }
        public CurrencyTransactionReason CurrencyTransactionReason { get; set; }
        public string Secret { get; set; }
    }
}
