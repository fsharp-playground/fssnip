module State.EventSource.DomainTypes

module Data =
    type User =
        | User of string

module Command =
    open Data

    type Command =
        | CreateNotification of User
        | ApproveRetrieve of User
        | ApproveSend of User

module Events =
    open Data
    open Fleece
    open Fleece.Operators
    open FSharpPlus

    type NotificationEvent =
        | NotificationCreated of User
        | RetrieveApproved of User
        | SendApproved of User
        | RetrieveStarted
        | RetrieveCompleted
        | SendStarted
        | SendCompleted
        static member ToJSON (x : NotificationEvent) =
            match x with
            | NotificationCreated (User u) ->
                jobj [
                    "eventType" .= "notificationCreated"
                    "value" .= u
                ]
            | RetrieveApproved (User u) ->
                jobj [
                    "eventType" .= "retrieveApproved"
                    "value" .= u
                ]
            | SendApproved (User u) ->
                jobj [
                    "eventType" .= "sendApproved"
                    "value" .= u
                ]
            | RetrieveStarted ->
                jobj [ "eventType" .= "retrieveStarted" ]
            | RetrieveCompleted ->
                jobj [ "eventType" .= "retrieveCompleted" ]
            | SendStarted ->
                jobj [ "eventType" .= "sendStarted" ]
            | SendCompleted ->
                jobj [ "eventType" .= "sendCompleted" ]
        static member FromJSON (_ : NotificationEvent) =
            function
            | JObject o ->
                monad {
                    let! eventType = o .@ "eventType"
                    match eventType with
                    | "notificationCreated" ->
                        let! value = o .@ "value"
                        return NotificationCreated (User value)
                    | "retrieveApproved" ->
                        let! value = o .@ "value"
                        return RetrieveApproved (User value)
                    | "sendApproved" ->
                        let! value = o .@ "value"
                        return SendApproved (User value)
                    | "retrieveStarted" ->
                        return RetrieveStarted
                    | "retrieveCompleted" ->
                        return RetrieveCompleted
                    | "sendStarted" ->
                        return SendStarted
                    | "sendCompleted" ->
                        return SendCompleted
                    | x ->
                        return! Failure (sprintf "Unknown notification event type: %s." x)
                }
            | x -> Failure (sprintf "Expected notification event, found %A." x)
            

module Notification =
    open Events
    type RetrieveState =
        | InProgress
        | Error

    type SendApprovalState =
        | WaitingApproval
        | Approved

    type SendState =
        | InProgress
        | Error

    type NotificationState =
        | Nothing
        | Held of SendApprovalState
        | ReadyForRetrieve of SendApprovalState
        | Retrieving of RetrieveState * SendApprovalState
        | ReadyForSend of SendApprovalState
        | Sending
        | Complete
        | Error of string
        static member fold state event =
            match event with
            | NotificationCreated _ ->
                match state with
                | Nothing ->
                    Held WaitingApproval
                | _ ->
                    Error "A notification with this ID already exists"
            | RetrieveApproved _ ->
                match state with
                | Held sendApproval ->
                    ReadyForRetrieve sendApproval
                | Nothing ->
                    Error "No notification with this ID has been created?"
                | _ ->
                    state
            | RetrieveStarted ->
                match state with
                | ReadyForRetrieve sendApproval ->
                    Retrieving (RetrieveState.InProgress, sendApproval)
                | _ ->
                    Error "A notification should not start retrieving until it's ready to retrieve."
            | RetrieveCompleted ->
                match state with
                | Retrieving (RetrieveState.InProgress, sendApproval) ->
                    ReadyForSend sendApproval
                | _ ->
                    Error "Can't complete retrieve if it hasn't started."
            | SendApproved _ ->
                match state with
                | Held _ -> Held Approved
                | ReadyForRetrieve _ -> ReadyForRetrieve Approved
                | Retrieving (retrieveState, _) -> Retrieving (retrieveState, Approved)
                | ReadyForSend _ -> ReadyForSend Approved
                | Nothing -> Error "Notification doesn't exist."
                | _ -> Error "Send already started!"
            | SendStarted ->
                match state with
                | ReadyForSend Approved ->
                    Sending
                | _ ->
                    Error "Can't start sending unless it was waiting to send."
            | SendCompleted ->
                match state with
                | Sending ->
                    Complete
                | _ ->
                    Error "Can't complete if it hasn't started sending."
            
module Audit =
    open Data
    open Events
    type Auditors =
        {
            Creator : User
            RetrieveAuthorisor : User option
            SendAuthorisor : User option
        }

    type AuditState =
        | Nothing
        | Error of string
        | Auditors of Auditors
        static member fold state event =
            match event with
            | NotificationCreated user ->
                match state with
                | Nothing ->
                    Auditors { Creator = user; RetrieveAuthorisor = None; SendAuthorisor = None }
                | _ ->
                    Error "A audit trail with this ID already exists"
            | RetrieveApproved user ->
                match state with
                | Nothing ->
                    Error "No notification with this ID has been created?"
                | Auditors { Creator = c; RetrieveAuthorisor = None; SendAuthorisor = s } ->
                    Auditors { Creator = c; RetrieveAuthorisor = Some user; SendAuthorisor = s }
                | Auditors _
                | Error _ ->
                    Error "Retrieve cannot be authorised twice."
            | SendApproved user ->
                match state with
                | Nothing ->
                    Error "No notification with this ID has been created?"
                | Auditors { Creator = c; RetrieveAuthorisor = r; SendAuthorisor = None } ->
                    Auditors { Creator = c; RetrieveAuthorisor = r; SendAuthorisor = Some user }
                | Auditors _
                | Error _ ->
                    Error "Retrieve cannot be authorised twice."
            | RetrieveStarted
            | RetrieveCompleted
            | SendStarted
            | SendCompleted ->
                state
