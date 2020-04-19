using MinecraftWrapper.Data.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data.Entities
{
    public class ScheduledTask
    {
        public Guid ScheduledTaskId { get; set; }
        
        [Required]
        [MaxLength ( 50 )]
        public string TaskName { get; set; }

        [Required]
        [MaxLength ( 30 )]
        public string CronString { get; set; }

        public bool Enabled { get; set; } = true;

        [Required]
        public ScheduledTaskType ScheduledTaskType { get; set; }

        [MaxLength(255)]
        public string Command { get; set; }
    }
}
