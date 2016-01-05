type TaskInfo =
    { ID : int
      User : string }

type TaskContent =
    | AllocateTime of System.TimeSpan
    | RemoveUser

type Task =
    { TaskInfo : TaskInfo
      Content : TaskContent }