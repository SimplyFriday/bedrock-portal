using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MinecraftWrapper.Services
{
    public class ConsoleApplicationWrapper<TParser> : IDisposable where TParser : IMessageParser
    {
        private bool _stopRequested = false;
        private readonly string _bdsDirectory;
        private readonly string _startDirectory;
        private readonly bool _restartOnFailure;
        private readonly int _maxOutputRetained;
        private Process _proc = null;
        private bool _wasDisposed = false;

        private readonly IServiceProvider _serviceProvider;
        public IMessageParser MessageParser { get; private set; }

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
            _bdsDirectory = options.Value.BdsPath;
            _startDirectory = options.Value.StartDirectory;
            _restartOnFailure = options.Value.RestartOnFailure;
            _maxOutputRetained = options.Value.MaxOutputRetained;
            _serviceProvider = serviceProvider;
        }

        private IMessageParser CreateMessageParser ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                return scope.ServiceProvider.GetService<TParser> ();
            }
        }

        public void Start ()
        {
            if ( _proc != null && !_wasDisposed && !_proc.HasExited )
            {
                return;
            }

            if ( MessageParser == null )
            {
                MessageParser = CreateMessageParser ();
            }

            _wasDisposed = false;
            _stopRequested = false;
            _proc = new Process ();

            _proc.EnableRaisingEvents = true;
            var procStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = _startDirectory,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            if ( RuntimeInformation.IsOSPlatform ( OSPlatform.Linux ) ) 
            {
                procStartInfo.FileName = $@"{_bdsDirectory}/bedrock_server";
                procStartInfo.EnvironmentVariables.Add ( "LD_LIBRARY_PATH", _bdsDirectory );
            } else if ( RuntimeInformation.IsOSPlatform ( OSPlatform.Windows ) )
            {
                procStartInfo.FileName = $@"{_bdsDirectory}\bedrock_server.exe";
            }
            else
            {
                throw new InvalidOperationException ( $"{RuntimeInformation.OSDescription} is not supported!" );
            }

            _proc.StartInfo = procStartInfo;

            _proc.OutputDataReceived += new DataReceivedEventHandler ( ( s, e ) =>
            {
                HandleOutput ( e.Data, ApplicationLogType.Stdout );
            } );

            _proc.ErrorDataReceived += new DataReceivedEventHandler ( ( s, e ) =>
            {
                HandleOutput ( e.Data, ApplicationLogType.Stderr );
            } );

            _proc.Exited += _proc_Exited;

            Console.WriteLine ( $"Starting dbs server from {_proc.StartInfo.FileName}" );

            _proc.Start ();
            _proc.BeginErrorReadLine ();
            _proc.BeginOutputReadLine ();
        }

        private void HandleOutput ( string output, ApplicationLogType type )
        {
            if ( !string.IsNullOrEmpty ( output ) )
            {
                var log = new ApplicationLog
                {
                    ApplicationLogType = type,
                    LogTime = DateTime.UtcNow,
                    LogText = output
                };

                _standardOutputQueue.Enqueue ( log );

                LogInputOutput ( log );

                if ( MessageParser != null )
                {
                    MessageParser.HandleOutput ( output );
                }
            }

            while ( _standardOutputQueue.Count > _maxOutputRetained )
            {
                _standardOutputQueue.Dequeue ();
            }
        }

        private void _proc_Exited ( object sender, EventArgs e )
        {
            if ( _restartOnFailure && !_stopRequested )
            {
                Start ();
            }
        }

        public void SendInput ( string input, string userId )
        {
            if ( MessageParser == null || MessageParser.FilterInput ( input ) )
            {
                var log = new ApplicationLog
                {
                    ApplicationLogType = ApplicationLogType.Stdin,
                    LogTime = DateTime.UtcNow,
                    LogText = input,
                    UserId = userId
                };

                LogInputOutput ( log );

                _proc.StandardInput.WriteLine ( input );
                _proc.StandardInput.Flush ();

                _standardOutputQueue.Enqueue ( log );
            }
        }

        public void Dispose ()
        {
            _stopRequested = true;

            if ( !_wasDisposed && !_proc.HasExited )
            {
                // Try to stop properly
                SendInput ( "stop", null );
                Thread.Sleep ( 2000 );

                if ( !_proc.HasExited )
                {
                    _proc.Kill ();
                }

                _proc.Dispose ();
                _wasDisposed = true;
            }
        }

        private void LogInputOutput ( ApplicationLog log )
        {
            LogInputOutput ( new List<ApplicationLog> { log } );
        }

        private void LogInputOutput ( IEnumerable<ApplicationLog> logs )
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var repo = scope.ServiceProvider.GetService<SystemRepository> ();
                repo.SaveApplicationLogs ( logs );
            }
        }

        /// <summary>
        /// This is only used to provide user feedback and is never actually sent to the underlying console application
        /// </summary>
        /// <param name="message">The message to add to the input/output queue</param>
        public void AddEphemeralMessage ( string message, string userId )
        {
            if ( !string.IsNullOrEmpty ( message ) )
            {
                var log = new ApplicationLog
                {
                    ApplicationLogType = ApplicationLogType.Stdin,
                    LogTime = DateTime.UtcNow,
                    LogText = message,
                    UserId = userId
                };

                _standardOutputQueue.Enqueue ( log );
                LogInputOutput ( log );
            }
        }

        public List<string> GetStdoutSinceTime ( DateTime cutoff )
        {
            return _standardOutputQueue
                .Where ( item => item.LogTime >= cutoff )
                .Select ( item => item.LogText ).ToList ();

        }
    }
}
