using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;

namespace MinecraftWrapper.Services
{
    public class ConsoleApplicationWrapper : IDisposable
    {
        private bool _stopRequested = false;
        private readonly string _exePath;
        private readonly string _startDirectory;
        private readonly bool _restartOnFailure;
        private readonly int _maxOutputRetained;
        private Process _proc = null;

        private Queue<string> _standardOutputQueue = new Queue<string> ();
        public IEnumerable<string> StandardOutput
        {
            get { return _standardOutputQueue.ToList(); }
        }

        private Queue<string> _errorOutputQueue = new Queue<string> ();
        public IEnumerable<string> ErrorOutput
        {
            get { return _errorOutputQueue.ToList (); }
        }

        public ConsoleApplicationWrapper ( IOptions<ApplicationSettings> options )
        {
            _exePath = options.Value.ExePath;
            _startDirectory = options.Value.StartDirectory;
            _restartOnFailure = options.Value.RestartOnFailure;
            _maxOutputRetained = options.Value.MaxOutputRetained;

            Start ();
        }

        public void Start ()
        {
            _proc = new Process ();

            _proc.EnableRaisingEvents = true;
            _proc.StartInfo = new ProcessStartInfo ( "cmd.exe", "/c " + _exePath );
            _proc.StartInfo.CreateNoWindow = true;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardError = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.WorkingDirectory = _startDirectory;

            _proc.OutputDataReceived += new DataReceivedEventHandler ( ( s, e ) =>
            {
                if ( !string.IsNullOrEmpty ( e.Data ) )
                {
                    _standardOutputQueue.Enqueue ( e.Data );
                }

                while ( _standardOutputQueue.Count > _maxOutputRetained )
                {
                    _standardOutputQueue.Dequeue ();
                }
            } );

            _proc.ErrorDataReceived += new DataReceivedEventHandler ( ( s, e ) =>
            {
                if ( !string.IsNullOrEmpty ( e.Data ) )
                {
                    _errorOutputQueue.Enqueue ( e.Data );
                }

                while ( _errorOutputQueue.Count > _maxOutputRetained )
                {
                    _errorOutputQueue.Dequeue ();
                }
            } );

            _proc.Exited += _proc_Exited;

            _proc.Start ();
            _proc.BeginErrorReadLine ();
            _proc.BeginOutputReadLine ();
        }

        private void _proc_Exited ( object sender, EventArgs e )
        {
            if ( _restartOnFailure && !_stopRequested )
            {
                Start ();
            }
        }

        public void SendInput (string input )
        {
            _proc.StandardInput.Write ( input );
            _proc.StandardInput.Flush ();
        }

        public void Dispose ()
        {
            _stopRequested = true;

            // Try to stop properly
            SendInput ( "stop" );
            Thread.Sleep ( 2000 );
            _proc.Kill ();
            _proc.Dispose ();
        }
    }
}
