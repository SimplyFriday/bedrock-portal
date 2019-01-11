using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class UserPreference
    {
        public Guid UserPreferenceId { get; set; }
        public UserPreferenceType UserPreferenceType { get; set; }
        public string Value { get; set; }

        [ForeignKey("User")]
        public virtual string UserId { get; set; }
        public virtual AuthorizedUser User { get; set; }
    }

    public enum UserPreferenceType
    {
        SavedTickingArea = 1
    }
}
