using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data
{
    public class ScheduledTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public ScheduledTaskRepository (ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ScheduledTask>> GetAllScheduledTasksAsync ()
        {
            return await _context.ScheduledTask.ToListAsync ();
        }

        public async Task<ScheduledTask> GetScheduledTaskByIdAsync ( Guid id )
        {
            return await _context.ScheduledTask
                .SingleOrDefaultAsync ( m => m.ScheduledTaskId == id );
        }

        public async Task SaveScheduledTaskAsync ( ScheduledTask scheduledTask )
        {
            if ( scheduledTask.ScheduledTaskId == Guid.Empty )
            {
                _context.Add ( scheduledTask );
            }
            else
            {
                if ( ScheduledTaskExists ( scheduledTask.ScheduledTaskId ) )
                {
                    _context.Update ( scheduledTask );
                }
            }

            await _context.SaveChangesAsync ();
        }

        public bool ScheduledTaskExists ( Guid id )
        {
            return _context.ScheduledTask.Any ( e => e.ScheduledTaskId == id );
        }
        public bool ScheduledTaskLogExists ( Guid id )
        {
            return _context.ScheduledTaskLog.Any ( e => e.ScheduledTaskLogId == id );
        }

        public async Task DeleteScheduledTasksAsync ( Guid id )
        {
            var scheduledTask = await _context.ScheduledTask.FindAsync(id);
            _context.ScheduledTask.Remove ( scheduledTask );
            await _context.SaveChangesAsync ();
        }

        public async Task<ScheduledTaskLog> GetLastLogForTask ( Guid id )
        {
            return await _context.ScheduledTaskLog
                .OrderByDescending ( log => log.StartTime )
                .FirstOrDefaultAsync ( log => log.ScheduledTask.ScheduledTaskId == id );
        }

        public async Task SaveScheduledTaskLogAsync ( ScheduledTaskLog scheduledTaskLog )
        {
            if ( scheduledTaskLog.ScheduledTaskLogId == Guid.Empty )
            {
                _context.Add ( scheduledTaskLog );
            }
            else
            {
                if ( ScheduledTaskLogExists ( scheduledTaskLog.ScheduledTaskLogId ) )
                {
                    _context.Update ( scheduledTaskLog );
                }
            }

            await _context.SaveChangesAsync ();
        }
    }
}
