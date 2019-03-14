using System.ComponentModel.DataAnnotations;

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
