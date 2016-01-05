//#r "Microsoft.TeamFoundation.Client"
//#r "Microsoft.TeamFoundation.VersionControl.Client"
//#r "Microsoft.TeamFoundation.VersionControl.Common"
open Microsoft.TeamFoundation.Client
open Microsoft.TeamFoundation.VersionControl.Client

let tfsCheckOut filename =
    let workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo filename
    let workspace = RegisteredTfsConnections.GetProjectCollections().First()
                    |> TfsTeamProjectCollectionFactory.GetTeamProjectCollection
                    |> workspaceInfo.GetWorkspace
    workspace.PendEdit filename
