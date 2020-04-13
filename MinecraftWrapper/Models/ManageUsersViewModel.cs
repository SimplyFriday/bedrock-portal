using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class ManageUsersViewModel
    {
        public List<ManageUserItem> Users { get; set; } = new List<ManageUserItem> ();

        public class ManageUserItem
        {
            public string UserId { get; set; }
            public DateTime? MembershipExpirationTime { get; set; }
            public TimeSpan MembershipLeft
            {
                get
                {
                    var membershipLeft = ((MembershipExpirationTime ?? DateTime.UtcNow) - DateTime.UtcNow);
                    return membershipLeft.TotalHours > 0 ? membershipLeft : new TimeSpan ( 0 );
                }
            }
            public decimal? CurrentMoney { get; set; }
            public string GamerTag { get; set; }
            public string DiscordId { get; set; }
            public int Rank { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsModerator { get; set; }
            public bool IsActive { get; set; }
        }
    }
}