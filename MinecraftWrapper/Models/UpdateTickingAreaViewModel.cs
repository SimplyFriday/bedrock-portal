using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class UpdateTickingAreaViewModel
    {
        public float XCoord { get; set; }
        public float ZCoord { get; set; }
        public string Name { get; set; }

        public List<SavedTickingArea> SavedTickingAreas { get; set; }

        public class SavedTickingArea
        {
            public float XCoord { get; set; }
            public float ZCoord { get; set; }
            public string Name { get; set; }
        }
    }
}

