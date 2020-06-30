using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinecraftWrapper.Data.Entities
{
    public class PlaytimeEvent
    {
        [NotMapped]
        public const string LOGIN_EVENT_CODE = "LOGIN";
        [NotMapped]
        public const string LOGOUT_EVENT_CODE = "LOGOUT";

        public int PlaytimeEventId { get; set; }
        public DateTime EventTime { get; set; }
        
        [MaxLength(10)]
        [Required]
        private string type;
        public string Type
        {
            get { return type; }
            set 
            {
                if ( value == LOGIN_EVENT_CODE || value == LOGOUT_EVENT_CODE )
                {
                    type = value;
                }
                else
                {
                    throw new ArgumentException ( $"Type code '{value}' is not supported!" );
                }
            }
        }

        [Required]
        public virtual ApplicationUser User { get; set; }
    }
}
