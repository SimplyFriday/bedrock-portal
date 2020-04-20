using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Services;

namespace MinecraftWrapper.Controllers
{
    [Authorize ( Roles = "Admin" )]
    public class ScheduledTasksController : Controller
    {
        private readonly ScheduledTaskRepository _scheduledTaskRepository;
        private readonly ScheduledTaskService _scheduledTaskService;

        public ScheduledTasksController(ScheduledTaskRepository scheduledTaskRepository, ScheduledTaskService scheduledTaskService)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _scheduledTaskService = scheduledTaskService;
        }

        // GET: ScheduledTasks
        public async Task<IActionResult> Index()
        {
            return View(await _scheduledTaskRepository.GetAllScheduledTasksAsync());
        }

        // GET: ScheduledTasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledTask = await _scheduledTaskRepository.GetScheduledTaskByIdAsync (id.Value);

            if (scheduledTask == null)
            {
                return NotFound();
            }

            return View ( scheduledTask );
        }

        // GET: ScheduledTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ScheduledTasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ( [Bind ( "ScheduledTaskId,TaskName,CronString,Enabled,ScheduledTaskType,Command" )] ScheduledTask scheduledTask )
        {
            if ( NCrontab.CrontabSchedule.TryParse ( scheduledTask.CronString ) == null )
            {
                var statusMessage = "ERROR: Cron String must contain 5 components of a schedule in the sequence of minutes, hours, days, months, and days of week.";
                ModelState.AddModelError ( "InvalidCronString", statusMessage );
                ViewBag.Status = statusMessage;
            }

            if (ModelState.IsValid)
            {
                await _scheduledTaskRepository.SaveScheduledTaskAsync ( scheduledTask );
                // No need to waste the user's time waiting for a db refresh here, it doesn't change the view.
                _ = _scheduledTaskService.RefreshDbTasks ();
                return RedirectToAction(nameof(Index));
            }
            return View(scheduledTask);
        }

        // GET: ScheduledTasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledTask = await _scheduledTaskRepository.GetScheduledTaskByIdAsync (id.Value);

            if (scheduledTask == null)
            {
                return NotFound();
            }
            return View(scheduledTask);
        }

        // POST: ScheduledTasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit ( Guid id, [Bind ( "ScheduledTaskId,TaskName,CronString,Enabled,ScheduledTaskType,Command" )] ScheduledTask scheduledTask )
        {
            if (id != scheduledTask.ScheduledTaskId)
            {
                return NotFound();
            }

            if ( NCrontab.CrontabSchedule.TryParse ( scheduledTask.CronString ) == null )
            {
                var statusMessage = "ERROR: Cron String must contain 5 components of a schedule in the sequence of minutes, hours, days, months, and days of week.";
                ModelState.AddModelError ( "InvalidCronString", statusMessage );
                ViewBag.Status = statusMessage;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _scheduledTaskRepository.SaveScheduledTaskAsync ( scheduledTask );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if ( !_scheduledTaskRepository.ScheduledTaskExists ( scheduledTask.ScheduledTaskId ) )
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // No need to waste the user's time waiting for a db refresh here, it doesn't change the view.
                _ = _scheduledTaskService.RefreshDbTasks ();
                return RedirectToAction(nameof(Index));
            }

            return View(scheduledTask);
        }

        // GET: ScheduledTasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduledTask = await _scheduledTaskRepository.GetScheduledTaskByIdAsync(id.Value);

            if (scheduledTask == null)
            {
                return NotFound();
            }

            return View(scheduledTask);
        }

        // POST: ScheduledTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _scheduledTaskRepository.DeleteScheduledTasksAsync ( id );
            return RedirectToAction(nameof(Index));
        }
    }
}
