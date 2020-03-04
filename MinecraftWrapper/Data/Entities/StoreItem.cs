using MinecraftWrapper.Data.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Data.Entities
{
    public class StoreItem
    {
        public Guid StoreItemId { get; set; }

        [MaxLength ( 40 )]
        [Required]
        public string Title { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        [Display( Name = "Type" )]
        public StoreItemType StoreItemTypeId { get; set; }

        public decimal Price { get; set; }

        [Display ( Name = "Minimum Rank" )]
        public int MinimumRank { get; set; } = 1;

        [MaxLength(450)]
        [Display(Name = "Effect", Prompt = "Enter console command to execute")]
        public string Effect { get; set; }
        
        [Required]
        public bool RequiresLogin { get; set; }
    }
}
