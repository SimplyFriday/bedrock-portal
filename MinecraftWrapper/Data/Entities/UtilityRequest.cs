using MinecraftWrapper.Data.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Data.Entities
{
    public class UtilityRequest
    {
        public Guid UtilityRequestId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime RequestTime { get; set; }

        [Required]
        public UtilityRequestType UtilityRequestType { get; set; }
    }
}
