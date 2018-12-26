using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data
{
    public class WhiteListEntry
    {
        [Required]
        public string name { get; set; }

        public string xuid { get; set; }

        public bool ignoresPlayerLimit { get; set; } = false;
    }
}
