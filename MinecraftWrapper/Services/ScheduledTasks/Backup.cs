// Adapted from https://github.com/clarkx86/papyrus-automation/blob/master/BackupManager.cs with permission

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public partial class ScheduledTaskService
    {
        public async Task CreateBackup ( bool fullCopy )
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var wrapper = scope.ServiceProvider.GetRequiredService<ConsoleApplicationWrapper<MinecraftMessageParser>> ();
                var worldPath = Path.Combine ( _applicationSettings.BdsPath, "worlds", _applicationSettings.WorldName );

                await CreateWorldBackup ( wrapper, worldPath, _applicationSettings.TempPath, fullCopy );
            }
        }

        public async Task CreateWorldBackup ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, string worldPath, string tempPath, bool fullCopy, bool archive = true )
        {
            if ( fullCopy )
            {
                if ( Directory.Exists ( tempPath ) )
                {
                    wrapper.AddEphemeralMessage ( "Clearing local world backup directory...\t", null);

                    Directory.Delete ( tempPath, true );
                    Directory.CreateDirectory ( tempPath );

                    Console.WriteLine ( "Done!" );
                }
                else
                {
                    Directory.CreateDirectory ( tempPath );
                }


                if ( Directory.Exists ( worldPath ) )
                {
                    wrapper.AddEphemeralMessage ( "Attempting to create full world backup...\t", null );

                    CopyDirectory ( worldPath, tempPath );

                    Console.WriteLine ( "Done!" );
                }
                else
                {
                    wrapper.AddEphemeralMessage ( "Invalid world directory. Could not create full world backup!", null );
                }
            }
            else
            {
                wrapper.AddEphemeralMessage ( "Creating backup...", null );
                wrapper.AddEphemeralMessage ( "Holding world saving...", null );

                // Send tellraw message 1/2
                wrapper.SendInput ( "say Creating backup...", null );

                wrapper.SendInput ( "save hold", null );
                var holdStart = DateTime.UtcNow;

                var matchPattern = new Regex ( "^(" + Path.GetFileName ( worldPath ) + @"[\/]{1})" );
                var files = new List<string> ();

                while ( files.Count == 0 )
                {
                    wrapper.SendInput ( "save query", null );
                    await Task.Delay ( 2000 );

                    files = wrapper.GetStdoutSinceTime (holdStart)
                        .Where ( item => matchPattern.IsMatch ( item ) )
                        .ToList ();
                }

                Regex fileListRegex = new Regex("(" + Path.GetFileName(worldPath) + @"[\/]{1}.+?)\:{1}(\d+)");
                MatchCollection matches = fileListRegex.Matches ( string.Join ( Environment.NewLine, files ) );

                string[,] sourceFiles = new string[matches.Count, 2];

                for ( int i = 0; i < matches.Count; i++ )
                {
                    sourceFiles[i, 0] = matches[i].Groups[1].Value.Replace ( Path.GetFileName ( worldPath ), "" );
                    sourceFiles[i, 1] = matches[i].Groups[2].Value;
                }

                wrapper.AddEphemeralMessage ( $"Copying {sourceFiles.GetLength ( 0 )} files... ", null );

                // ACTUAL COPYING BEGINS HERE
                if ( !Directory.Exists(Path.Combine(tempPath, "db" ) ) )
                {
                    Directory.CreateDirectory ( Path.Combine ( tempPath, "db" ) );
                }

                for ( uint i = 0; i < sourceFiles.GetLength ( 0 ); i++ )
                {
                    var subdir =
                        File.Exists(Path.Join(worldPath, "db", Path.GetFileName(sourceFiles[i, 0])))
                        ? $"{Path.DirectorySeparatorChar}db" : "";

                    string filePath = Path.Join(worldPath, subdir, Path.GetFileName(sourceFiles[i, 0]));
                    string targetPath = tempPath + sourceFiles[i, 0];

                    wrapper.AddEphemeralMessage ( $"Copying from '{filePath}' to '{targetPath}'", null );

                    WriteFileCopy ( filePath, targetPath, int.Parse ( sourceFiles[i, 1] ) );
                }

                #region FILE INTEGRITY CHECK

                wrapper.AddEphemeralMessage ( "Veryfing file-integrity... ", null );

                string[] sourceDbFiles = Directory.GetFiles(worldPath + "/db/");
                string[] targetDbFiles = Directory.GetFiles(tempPath + "/db/");

                foreach ( string tFile in targetDbFiles )
                {
                    bool found = false;
                    foreach ( string sFile in sourceDbFiles )
                    {
                        if ( Path.GetFileName ( tFile ) == Path.GetFileName ( sFile ) )
                        {
                            found = true;
                            break;
                        }
                    }

                    // File isn't in the source world directory anymore, delete!
                    if ( !found )
                    {
                        File.Delete ( tFile );
                    }
                }

                #endregion

                Console.WriteLine ( "Resuming world saving..." );

                wrapper.SendInput ( "save resume", null );
                var resumeRegex = new Regex ( "^Changes to the (level|world) are resumed.?" );

                while ( !wrapper.StandardOutput.Any ( item => resumeRegex.IsMatch ( item ) ) )
                {
                    await Task.Delay ( 500 );
                }

                string tellrawMsg = "Finished creating backup!";

                // Archive
                if ( archive )
                {
                    wrapper.AddEphemeralMessage ( "Archiving world backup...", null );
                    if ( ArchiveBackup ( tempPath, _applicationSettings.ArchivePath, _applicationSettings.BackupsToKeep, _applicationSettings.WorldName, fullCopy ? "FULL" : "" ) ) 
                    {
                        wrapper.AddEphemeralMessage ( "Archiving done!", null );
                    }
                    else
                    {
                        wrapper.AddEphemeralMessage ( "Archiving failed!", null );
                        tellrawMsg = "Could not archive backup!";
                    }
                }

                // Send tellraw message 2/2
                wrapper.SendInput ( $"say {tellrawMsg}", null );

                wrapper.AddEphemeralMessage ( "Backup done!", null );
            }
        }

        private void WriteFileCopy (string filePath, string targetPath, int fileLength)
        {
            using ( FileStream sourceStream = File.Open ( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            using ( FileStream targetStream = File.Open ( targetPath, FileMode.Create, FileAccess.Write ) )
            {
                // Read bytes until truncate indicator
                for ( int j = 0; j < fileLength; j++ )
                {
                    targetStream.WriteByte ( (byte) sourceStream.ReadByte () );
                }
            }
        }

        public static bool ArchiveBackup ( string sourcePath, string destinationPath, uint backupsToKeep, string worldName, string tag = "" )
        {
            bool result = false;

            if ( !Directory.Exists ( destinationPath ) ) { Directory.CreateDirectory ( destinationPath ); }

            string[] files = Directory.GetFiles(destinationPath);
            DateTime[] creationTimes = new DateTime[files.Length];

            for ( int i = 0; i < files.Length; i++ )
            {
                creationTimes[i] = File.GetCreationTime ( files[i] );
            }

            Array.Sort ( files, creationTimes );

            if ( files.Length > backupsToKeep )
            {
                for ( uint i = 0; i < Math.Abs ( backupsToKeep - files.Length ); i++ )
                {
                    try
                    {
                        File.Delete ( files[i] );
                    }
                    catch
                    {
                        Log.Warning ( $"Could not delete {files[i]}" );
                    }
                }
            }

            string archiveName = string.Format("{0}_{1}{3}.{2}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm"), worldName, "zip", tag );
            string archivePath = Path.Join(destinationPath, archiveName);

            if ( !File.Exists ( archivePath ) )
            {
                try
                {
                    ZipFile.CreateFromDirectory ( sourcePath, archivePath, CompressionLevel.Optimal, false );
                    result = true;
                }
                catch
                {
                    Log.Error ( $"Could not create archive \"{archiveName}\"!" );
                    result = false;
                }
            }
            else
            {
                Log.Error ( $"Could not create archive \"{archiveName}\" because it already exists!" );
                result = false;
            }

            return result;
        }

        public static void CopyDirectory ( String sourceDir, String targetDir )
        {
            // Create root directory
            if ( !Directory.Exists ( targetDir ) )
            {
                Directory.CreateDirectory ( targetDir );
            }

            string[] sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            foreach ( string sFile in sourceFiles )
            {
                string tFile = sFile.Replace(sourceDir, targetDir);

                // Create sub-directory if needed
                string subDir = Path.GetDirectoryName(tFile);
                if ( !Directory.Exists ( subDir ) )
                {
                    Directory.CreateDirectory ( subDir );
                }

                File.Copy ( sFile, tFile, true );

            }
        }
    }
}
