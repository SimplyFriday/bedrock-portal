using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MinecraftWrapper.Services;

namespace MinecraftWrapper.Controllers
{
    [Authorize ( Roles = "Admin,Moderator" )]
    public class McFilesController : Controller
    {
        private readonly BackupService _backupService;
        private readonly ScheduledTaskService _scheduledTaskService;

        public McFilesController ( BackupService backupService, ScheduledTaskService scheduledTaskService )
        {
            _backupService = backupService;
            _scheduledTaskService = scheduledTaskService;
        }

        public IActionResult Index (string statusMessage = "")
        {
            if ( !string.IsNullOrEmpty ( statusMessage ) ) 
            {
                ViewBag.Status = statusMessage;
            }

            return View ( _backupService.GetArchivedBackupList () );
        }

        // Adapted from https://stackoverflow.com/questions/41383338/how-to-download-a-zipfile-from-a-dotnet-core-webapi
        public async Task<IActionResult> DownloadBackup ( string id )
        {
            const string contentType ="application/zip";
            HttpContext.Response.ContentType = contentType;

            var result = new FileContentResult ( await _backupService.GetBackupBytes ( id ), contentType )
            {
                FileDownloadName = $"{id}.zip"
            };

            return result;
        }

        public async Task<IActionResult> NewBackup ()
        {
            await _scheduledTaskService.CreateBackup ( false );
            return RedirectToAction ( nameof ( Index ), "McFiles", new { statusMessage = "Succesfully created backup" }, "" );
        }

        [Authorize ( Roles = "Admin" )]
        public IActionResult UploadFile ()
        {
            return View ();
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> UploadWorldFile ( IFormFile file )
        {
            if ( file != null && file.Length > 0 )
            {
                await _backupService.ReplaceFiles ( false, file );
            }

            return RedirectToAction ( nameof ( Index ), "McFiles", new { statusMessage = "Succesfully updated files" }, "" );
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [ValidateAntiForgeryToken]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> UploadBdsFile ( IFormFile file )
        {
            if ( file != null && file.Length > 0 )
            {
                await _backupService.ReplaceFiles ( true, file );
            }

            return RedirectToAction ( nameof ( Index ), "McFiles", new { statusMessage = "Succesfully updated files" }, "" );
        }
    }
}