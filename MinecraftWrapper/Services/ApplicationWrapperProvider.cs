using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class ApplicationWrapperProvider
    {
        private Dictionary<string,ConsoleApplicationWrapper<MinecraftMessageParser>> _minecraftInstances = 
            new Dictionary<string, ConsoleApplicationWrapper<MinecraftMessageParser>>();

        public ConsoleApplicationWrapper<MinecraftMessageParser> GetInstanceByName ( string name )
        {
            try
            {
                return _minecraftInstances[name];
            } catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public void AddInstance ( string name, ConsoleApplicationWrapper<MinecraftMessageParser> wrapper )
        {
            _minecraftInstances.Add ( name, wrapper );
        }
    }
}
