using System;
using Ozeki.Media;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;

namespace Conference_Call
{
    class Program
    {
        static ISoftPhone softphone;
        static IPhoneLine phoneLine;

        static ConferenceRoom conferenceRoom;

        private static void Main(string[] args)
        {
            softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);

            var registrationRequired = true;
            var userName = "715";
            var displayName = "715";
            var authenticationId = "715";
            var registerPassword = "715";
            var domainHost = "192.168.115.100";
            var domainPort = 5060;

            var account = new SIPAccount(registrationRequired, displayName, userName, authenticationId, registerPassword, domainHost, domainPort);

            RegisterAccount(account);

            Console.ReadLine();
        }

        static void RegisterAccount(SIPAccount account)
        {
            try
            {
                phoneLine = softphone.CreatePhoneLine(account);
                phoneLine.RegistrationStateChanged += line_RegStateChanged;
                softphone.IncomingCall += softphone_IncomingCall;
                softphone.RegisterPhoneLine(phoneLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration: " + ex);
            }
        }

        static void InitializeConferenceRoom()
        {
            conferenceRoom = new ConferenceRoom();
            conferenceRoom.StartConferencing();
        }

        static void softphone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            IPhoneCall call = e.Item;
            call.CallStateChanged += call_CallStateChanged;
            call.Answer();
        }

        static void line_RegStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            if (e.State == RegState.NotRegistered || e.State == RegState.Error)
                Console.WriteLine("Registration failed!");

            if (e.State == RegState.RegistrationSucceeded)
            {
                Console.WriteLine("Registration succeeded - Online!");
                InitializeConferenceRoom();
            }
        }

        static void call_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            IPhoneCall call = sender as IPhoneCall;

            if (e.State == CallState.Answered)
                conferenceRoom.AddToConference(call);
            else if (e.State.IsCallEnded())
                conferenceRoom.RemoveFromConference(call);
        }

    }
}
