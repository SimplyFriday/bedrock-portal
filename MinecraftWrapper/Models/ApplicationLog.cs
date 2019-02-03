using System;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Models
{
    public class ApplicationLog
    {
        public Guid ApplicationLogId { get; set; }

        [Required]
        public DateTime LogTime { get; set; }

        [Required]
        public string LogText { get; set; }

        [Required]
        public ApplicationLogType ApplicationLogType { get; set; }

        public string UserId { get; set; }
    }

    public enum ApplicationLogType
    {
        Stdout = 1,
        Stderr = 2,
        Stdin = 3
    }
}
