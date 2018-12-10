using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public interface IMessageParser
    {
        void HandleOutput ( string output );
        bool FilterInput ( string input );
    }
}
