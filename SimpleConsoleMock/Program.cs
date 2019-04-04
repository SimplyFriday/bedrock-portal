using System;
using System.Threading;

namespace SimpleConsoleMock
{
    /// <summary>
    /// This purpose of this project is to provide a simple console app that provides occasional output whith
    /// which you can test the main app with. The main reason to use this over an actual BDS instance is that
    /// BDS has a ton of overhead which isn't needed for much of the development items.
    /// </summary>
    class Program
    {
        static void Main ( string [] args )
        {
            while ( true )
            {
                try
                {
                    Console.WriteLine ( "This is very simple" );

                    // We probably don't need input very opten. If specific output is needed this can either
                    // be changed and recompiled or the output can be caught in the portal and modified there.
                    Thread.Sleep ( 10000 );
                } catch ( Exception )
                {
                    // Literally do not care
                }
            }
        }
    }
}
