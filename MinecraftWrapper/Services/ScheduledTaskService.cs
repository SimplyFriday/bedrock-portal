using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using NCrontab;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public partial class ScheduledTaskService
    {
        public Dictionary<string,Delegate> RapidTasks = new Dictionary<string, Delegate> ();
        public Dictionary<string,Delegate> Tasks = new Dictionary<string, Delegate>  ();
        public Dictionary<string,Delegate> SlowTasks = new Dictionary<string, Delegate>  ();

        private bool _stopRequested = false;
        private IEnumerable<ScheduledTask> _scheduledTasks = new List<ScheduledTask> ();

        private readonly WhiteListService _whiteListService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationSettings _applicationSettings;

        public ScheduledTaskService ( WhiteListService whiteListService, IServiceProvider serviceProvider, IOptions<ApplicationSettings> options )
        {
            _whiteListService = whiteListService;
            _serviceProvider = serviceProvider;
            _applicationSettings = options.Value;
        }

        public void Start ()
        {
            _ = RunRapidTasks ();
            _ = RunTasks ();
            _ = RunSlowTasks ();
        }

        private async Task RunRapidTasks ()
        {
            do
            {
                foreach ( var t in RapidTasks )
                {
                    t.Value.DynamicInvoke ();
                }

                await Task.Delay ( 10000 );
            } while ( !_stopRequested );
        }

        private async Task RunTasks ()
        {
            do
            {
                foreach ( var t in Tasks )
                {
                    t.Value.DynamicInvoke ();
                }

                await Task.Delay ( 60000 );
            } while ( !_stopRequested );
        }

        private async Task RunSlowTasks ()
        {
            do
            {
                foreach ( var t in SlowTasks )
                {
                    t.Value.DynamicInvoke ();
                }

                await Task.Delay ( 120000 );
            } while ( !_stopRequested );
        }

        public void Stop ()
        {
            _stopRequested = true;
        }

        public void RegisterTasks ()
        {
            Tasks.Add ( "UpdateWhiteList", new Action ( async () => await UpdateWhiteList () ) );
            SlowTasks.Add ( "RefreshDbTasks", new Action ( async () => await RefreshDbTasks () ) );
            RapidTasks.Add ( "RunDbTasks", new Action ( async () => await RunDbTasks () ) );
        }

        private async Task RunDbTasks ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var taskRepository = scope.ServiceProvider.GetRequiredService<ScheduledTaskRepository> ();
                var applicationWrapper = scope.ServiceProvider.GetRequiredService<ConsoleApplicationWrapper<MinecraftMessageParser>> ();

                foreach ( var task in _scheduledTasks )
                {
                    CrontabSchedule schedule = null;

                    try
                    {
                        schedule = CrontabSchedule.Parse ( task.CronString );
                    } catch (CrontabException ex)
                    {
                        Log.Error ( ex, $"Error occurred while running ScheduledTaskId={task.ScheduledTaskId}" );
                        continue;
                    }

                    // In this one instance we're going to use server local time, because that's
                    // how an end user would expect the app to work and timezones are hard.
                    var next = schedule.GetNextOccurrence ( DateTime.Now.AddMinutes(-1) );

                    if ( DateTime.Now >= next )
                    {
                        var lastLog = await taskRepository.GetLastLogForTask ( task.ScheduledTaskId );

                        if ( lastLog == null || lastLog.StartTime < next )
                        {
                            var nextLog = new ScheduledTaskLog
                            {
                                StartTime = DateTime.Now,
                                ScheduledTaskId = task.ScheduledTaskId
                            };

                            await taskRepository.SaveScheduledTaskLogAsync ( nextLog );

                            try
                            {
                                switch ( task.ScheduledTaskType )
                                {
                                    case ScheduledTaskType.Backup:
                                        // TODO
                                        break;
                                    case ScheduledTaskType.Command:
                                        applicationWrapper.SendInput ( task.Command, null );
                                        break;
                                }

                                nextLog.CompletedTime = DateTime.Now;
                                nextLog.CompletionStatus = "SUCCESS";

                                await taskRepository.SaveScheduledTaskLogAsync ( nextLog );
                            }
                            catch ( Exception ex )
                            {
                                Log.Error ( ex, $"Exception occurred while running ScheduledTaskId={task.ScheduledTaskId}" );
                                
                                nextLog.CompletedTime = DateTime.Now;
                                nextLog.CompletionStatus = "FAILED";

                                await taskRepository.SaveScheduledTaskLogAsync ( nextLog );
                            }
                        }
                    }
                }
            }
        }

        public async Task RefreshDbTasks ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var taskRepository = scope.ServiceProvider.GetRequiredService<ScheduledTaskRepository> ();
                _scheduledTasks = await taskRepository.GetAllScheduledTasksAsync ();
            }
        }
    }
}
