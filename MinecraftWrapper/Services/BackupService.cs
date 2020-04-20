using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class BackupService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly ConsoleApplicationWrapper<MinecraftMessageParser> _wrapper;
        private readonly ScheduledTaskService _scheduledTaskService;
        public BackupService ( IOptions<ApplicationSettings> options, ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, ScheduledTaskService scheduledTaskService )
        {
            _applicationSettings = options.Value;
            _wrapper = wrapper;
            _scheduledTaskService = scheduledTaskService;
        }

        public IEnumerable<string> GetArchivedBackupList ()
        {
            return
                Directory.GetFiles ( _applicationSettings.ArchivePath )
                .Where ( file => file.EndsWith ( ".zip" ) )
                .Select ( file => Path.GetFileName ( file ) )
                .OrderByDescending ( file => file );
        }

        public async Task<byte[]> GetBackupBytes ( string filename )
        {
            var path = $"{_applicationSettings.ArchivePath}{Path.DirectorySeparatorChar}{filename}";
            return await File.ReadAllBytesAsync ( path );
        }

        public async Task ReplaceFiles ( bool fullBds, IFormFile file )
        {
            // Make a backup first
            try
            {
                await _scheduledTaskService.CreateBackup ( fullBds );
            }
            catch 
            {
                // Nothing really to do here... Windows backups are broken. We could test for Windows before
                // backing up, but I don't want to publish a new release just to address this.
            }

            try
            {
                var tmpZipLocation = Path.GetTempFileName ();
                using ( var stream = File.Create ( tmpZipLocation ) )
                {
                    await file.CopyToAsync ( stream );
                }

                var extractPath = fullBds ?
                _applicationSettings.BdsPath :
                $"{_applicationSettings.BdsPath}{Path.DirectorySeparatorChar}worlds{Path.DirectorySeparatorChar}{_applicationSettings.WorldName}";

                _wrapper.Dispose ();

                // unzip file with clobber
                using ( var archive = ZipFile.OpenRead ( tmpZipLocation ) )
                {
                    foreach ( var entry in archive.Entries.Where ( entry => !entry.FullName.EndsWith ( '/' ) ) ) 
                    {
                        entry.ExtractToFile ( Path.Combine ( extractPath, entry.FullName ), true );
                    }
                }

                File.Delete ( tmpZipLocation );
            }
            finally
            {
                _wrapper.Start ();
            }
        }
    }
}
