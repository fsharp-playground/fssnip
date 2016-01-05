open System
open System.Text.RegularExpressions

let StringOfLengthConstructor<'c> (length:int) (defaultConstructor:string->'c) (input:string) =
    match input with
    | x when x<>null && x.Length = length -> Some(defaultConstructor(input))
    | _ -> None

type String4 = | String4 of string
let String4 = StringOfLengthConstructor 4 String4

type String5 = | String5 of string
let String5 = StringOfLengthConstructor 5 String5
type DoB = | DoB of DateTime

let DoB (dt:DateTime) = // shadow constructor
    if(dt.Year>1910) //what does the business say a min valid dob should be? dealspot says younger than 100
    then Some(DoB dt) 
    else None
//let (|DoB|) (DoB dt) = dt

type ZipCode = 
    | US of String5 * (String4 option)
    | Canada of string // 3 digit Forward Sortation Area 3 digit Local Delivery Unit
    
type Email = | Email of string
    with member this.Value = match this with Email x -> x

let Email (input) = //shadow the constructor
    if Regex.IsMatch(input,@"^\S+@\S+\.\S+$")
        then Some (Email input)
        else None
type Gender = 
            | M
            | F
[<Measure>]
type OrgIdentifier
type Member = { 
        Gender: Gender option
        ZipCode:ZipCode
        DoB:DoB
        Email:Email option
        IsBounce:bool
        IsEnabled:bool
        IsDoi:bool
        IsDnc:bool
        IsFraud:bool
        IsActiveOverride:bool
        IsDeleted:bool
        CountDemographic:int
        OrgId:int<OrgIdentifier>
        IsScrub:bool
        } with 
        member self.IsPanelMemberActive =
            let emailIs684TrialPay = self.OrgId=684<OrgIdentifier> && self.Email.IsSome && self.Email.Value.Value.StartsWith("trialpay:")
            let isNon494DoiOrDemo = self.OrgId<>494<OrgIdentifier> && (self.IsDoi || self.CountDemographic>0)
            match self with
            | x when x.IsFraud -> false
            | x when x.IsActiveOverride -> true
            | x when x.IsEnabled = false -> false
            | x when x.IsDeleted -> false
            | x when x.IsBounce -> false
            | x when x.IsScrub -> false

          //| x when not (x.IsDnc=false || emailIs684TrialPay) -> false // if dnc && ! trialpay then false
            | x when x.IsDnc && emailIs684TrialPay=false -> false // if dnc && ! trialpay then false
            | _ as x ->  isNon494DoiOrDemo || x.IsDoi || emailIs684TrialPay
        member self.IsActive memberTypeCode (isPartialMember:bool option) =
            if memberTypeCode="P" && isPartialMember.IsNone || isPartialMember.Value=false 
                then Some(self.IsPanelMemberActive) 
                else None
                
        member x.IsContactable memberTypeCode isPartialMember = // is allowed to be contacted (not accounting for initial DoI email)
            let isActive = x.IsActive memberTypeCode isPartialMember
            isActive.IsSome && isActive.Value && x.IsEnabled && x.IsDoi && not x.IsDnc && x.Email.IsSome