using Microsoft.AspNetCore.Mvc.Rendering;
using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class ManageUserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public int UserRole { get; set; }
        public List<SelectListItem> UserRoles = new List<SelectListItem> {
                                                    new SelectListItem { Text = "Player",       Value = "0", Selected = false},
                                                    new SelectListItem { Text = "Moderator",    Value = "1", Selected = false},
                                                    new SelectListItem { Text = "Admin",        Value = "2", Selected = false} };
        public List<PlaytimeEvent> RecentEvents { get; set; }
        public float RecentHours { get; set; }
        public DateTime SearchCutoff { get; set; }
    }
}
