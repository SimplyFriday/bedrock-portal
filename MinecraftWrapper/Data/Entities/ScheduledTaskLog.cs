using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data.Entities
{
    public class ScheduledTaskLog
    {
        public Guid ScheduledTaskLogId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        
        [MaxLength ( 30 )]
        public string CompletionStatus { get; set; }

        [Required]
        public ScheduledTask ScheduledTask { get; set; }

        [ForeignKey("ScheduledTask")]
        public Guid ScheduledTaskId { get; set; }
    }
}
