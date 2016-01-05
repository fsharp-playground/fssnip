#r @"Microsoft.Build"
#r @"Microsoft.Build.Engine"
#r @"Microsoft.Build.Framework"
let buildSolution slnFile (configuration, platform) = 
    let parms = new Microsoft.Build.Execution.BuildParameters(new Microsoft.Build.Evaluation.ProjectCollection())
    let props = new System.Collections.Generic.Dictionary<string, string>()
    props.Add("Configuration", "Debug")
    props.Add("Platform", "x86")
    let req = new Microsoft.Build.Execution.BuildRequestData(slnFile, props, null, [| "Build" |], null)
    Microsoft.Build.Execution.BuildManager.DefaultBuildManager.Build(parms, req);
