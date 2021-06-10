using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data.Entities
{
    public class MinecraftInstance
    {
        public int MinecraftInstanceId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}
