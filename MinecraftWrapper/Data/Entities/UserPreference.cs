using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinecraftWrapper.Data.Entities
{
    public class UserPreference
    {
        public Guid UserPreferenceId { get; set; }
        public UserPreferenceType UserPreferenceType { get; set; }
        public string Value { get; set; }

        [ForeignKey("User")]
        public virtual string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public enum UserPreferenceType
    {
        SavedTickingArea = 1
    }
}
