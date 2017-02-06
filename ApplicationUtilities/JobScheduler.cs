using System;
using Quartz;
using Quartz.Impl;

namespace ApplicationUtilities
{
    public class JobScheduler
    {
        public static void ScheduleJob(string moduleName)
        {
            // construct a scheduler factory
            var schedFact = new StdSchedulerFactory();
            var sched = schedFact.GetScheduler();
            sched.Start();

            // Create a job here
            var job = JobBuilder.Create<SchedulerJob>()
                .WithIdentity("ModuleScheduleJob", "JobGroupModule")
                .Build();

            // Create trigger, the job is triggered every 1 minute
            var trigger = TriggerBuilder.Create()
                .WithIdentity("ModuleScheduleTrigger", "JobGroupModule")
                //  Use Cron schedule for Calender scheduling
                // .WithCronSchedule("0 0 10 * * ?")
                // .RepeatForever() for looping it indefinitely
                .WithSimpleSchedule(i => i.WithIntervalInMinutes(1).RepeatForever())
                .ForJob("ModuleScheduleJob", "JobGroupModule")
                .Build();

            // Schedule the job using the job and trigger 
            sched.ScheduleJob(job, trigger);
        }
    }

    public class SchedulerJob : IJob
    {
        void IJob.Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Job Done..");
            // Do what your job needs to do here
            // Task.Run(() => ((IModule)AtkUtils.GetInstance(moduleName))?.Run());
        }
    }
}
