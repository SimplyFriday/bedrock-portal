using MinecraftWrapper.Data.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinecraftWrapper.Data.Entities
{
    public class UserCurrency
    {
        public Guid UserCurrencyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateNoted { get; set; }

        // These have their own reference table and foreign keys are mapped in ApplicationDbContext
        public CurrencyType CurrencyTypeId { get; set; }
        public CurrencyTransactionReason CurrencyTransactionReasonId { get; set; }

                
        [Required]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(User))]
        public virtual string UserId { get; set; }

        public virtual StoreItem StoreItem { get; set; }

        [ForeignKey(nameof(StoreItem))]
        public virtual Guid? StoreItemId { get; set; }

        public virtual UserCurrency CreatedFromTransaction { get; set; }

        [ForeignKey ( nameof ( CreatedFromTransaction ) )]
        public virtual Guid? CreatedFromTransactionId { get; set; }
    }
}
