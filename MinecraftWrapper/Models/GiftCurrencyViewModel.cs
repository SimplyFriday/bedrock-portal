using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Models
{
    public class GiftCurrencyViewModel
    {
        public decimal GiftCurrency { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<TransactionDisplay> Transactions { get; set; } = new List<TransactionDisplay> ();

        public class TransactionDisplay
        {
            public DateTime TransactionDate { get; set; }
            public decimal Amount { get; set; }
            
            [MaxLength(75)]
            public string Notes { get; set; }
        }
    }
}

