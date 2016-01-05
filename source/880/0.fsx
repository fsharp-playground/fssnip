open System
open Quartz
open Quartz.Impl

let schedulerFactory = StdSchedulerFactory()
let scheduler = schedulerFactory.GetScheduler()
scheduler.Start()

type Job =

    interface IJob with

        member x.Execute(context: IJobExecutionContext) =
            Console.WriteLine(DateTime.Now)


let job = JobBuilder.Create<Job>().Build()

let trigger =
    TriggerBuilder.Create()
        .WithSimpleSchedule(fun x ->
            x.WithIntervalInSeconds(1).RepeatForever() |> ignore)
        .Build()

scheduler.ScheduleJob(job, trigger) |> ignore
//scheduler.Shutdown()

// Output:
// 09/10/2012 11:08:03
// 09/10/2012 11:08:04
// 09/10/2012 11:08:05
// 09/10/2012 11:08:06
// 09/10/2012 11:08:07
// 09/10/2012 11:08:08
// 09/10/2012 11:08:09
// 09/10/2012 11:08:10
// 09/10/2012 11:08:11
// 09/10/2012 11:08:12
// ...