using System;
using System.Threading;

namespace VRC
{
    public class Spoofer
    {
        public async void Start()
        {
            _outputLog = new OutputLog();
            if (_outputLog.File == null)
            {
                Utils.Error("VRChat output log not found.");
                Utils.SetConsoleTitle("Output log not found");
                return;
            }

            _api = new API();
            await _api.Start();

            SpooferTitle(true);
            Utils.WriteLine("Enter world to spoof your location as, or leave blank to use a default world.");
            string world = Input.GetInput("> ");
            if (string.IsNullOrWhiteSpace(world)) world = $"{DEFAULT_WORLD}:{Utils.Random.Next(1, 100000)}~private({_api.UserInfo.id})~nonce({Utils.GenerateNonce()})";
            SpoofWorld = world;
            SpooferTitle(true);

            _outputLog = new OutputLog();
            _outputLog.TextWritten += OnTextWritten;
            _outputLog.StartWatching();

            await _api.JoinWorld(SpoofWorld);
            Spoofing = true;
        }

        private void OnTextWritten(object sender, string text)
        {
            text = text.ToLower();
            if (text.Contains("sending put request") && Spoofing)
            {
                if (text.Contains("api/1/joins"))
                {
                    Thread.Sleep(1000);
                    SendJoinRequest();
                }
                else if (text.Contains("api/1/visits"))
                {
                    Thread.Sleep(1000);
                    SendVisitRequest();
                }
            }
        }

        private async void SendJoinRequest()
        {
            await _api.JoinWorld(SpoofWorld);
        }

        private async void SendVisitRequest()
        {
            await _api.VisitWorld(SpoofWorld);
        }

        private static void SpooferTitle(bool clear = false)
        {
            if (clear) Console.Clear();
            Utils.WriteLine(@"-----------");
            Utils.WriteLine(@"  SPOOFER  ");
            Utils.WriteLine(@"-----------");
            Utils.WriteLine();
        }

        public string SpoofWorld { get; private set; }
        public bool Spoofing { get; private set; }

        private API _api;
        private OutputLog _outputLog;

        private const string DEFAULT_WORLD = "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd";
    }
}
