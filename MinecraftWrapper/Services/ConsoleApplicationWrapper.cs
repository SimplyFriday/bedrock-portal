using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;

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

        private readonly IServiceProvider _serviceProvider;

        private Queue<ApplicationLog> _standardOutputQueue = new Queue<ApplicationLog> ();
        public IEnumerable<string> StandardOutput
        {
            get { return _standardOutputQueue.Select ( item => item.LogText ).ToList (); }
        }

        private Queue<ApplicationLog> _errorOutputQueue = new Queue<ApplicationLog> ();
        public IEnumerable<string> ErrorOutput
        {
            get { return _errorOutputQueue.Select ( item => item.LogText ).ToList (); }
        }

        public ConsoleApplicationWrapper ( IOptions<ApplicationSettings> options, SystemRepository systemRepository, IServiceProvider serviceProvider )
        {
            _exePath = options.Value.ExePath;
            _startDirectory = options.Value.StartDirectory;
            _restartOnFailure = options.Value.RestartOnFailure;
            _maxOutputRetained = options.Value.MaxOutputRetained;
            _serviceProvider = serviceProvider;

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
                    var log = new ApplicationLog
                    {
                        ApplicationLogType = ApplicationLogType.Stdout,
                        LogTime = DateTime.UtcNow,
                        LogText = e.Data
                    };

                    _standardOutputQueue.Enqueue ( log );

                    LogInputOutput ( log );
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
                    var log = new ApplicationLog
                    {
                        ApplicationLogType = ApplicationLogType.Stderr,
                        LogTime = DateTime.UtcNow,
                        LogText = e.Data
                    };

                    _errorOutputQueue.Enqueue ( log );

                    LogInputOutput ( log );
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
            var log = new ApplicationLog
            {
                ApplicationLogType = ApplicationLogType.Stdin,
                LogTime = DateTime.UtcNow,
                LogText = input
            };

            LogInputOutput ( log );

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

        private void LogInputOutput (ApplicationLog log )
        {
            LogInputOutput ( new List<ApplicationLog> { log } );
        }

        private void LogInputOutput ( IEnumerable<ApplicationLog> logs )
        {
            using (var scope = _serviceProvider.CreateScope () )
            {
                var repo = scope.ServiceProvider.GetService<SystemRepository> ();
                repo.SaveApplicationLogs ( logs );
            }
        }
    }
}
