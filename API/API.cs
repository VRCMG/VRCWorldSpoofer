using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VRC
{
    public class API
    {
        public API()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://vrchat.com/api/1/")
            };
            _client.DefaultRequestHeaders.Clear();
        }

        public async Task Start()
        {
            Utils.SetConsoleTitle("Not Logged In");
            if (await GetAPIKey())
            {
                if (Config.Current.RememberLogin && File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/auth"))
                {
                    string contents = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/auth");
                    if (!string.IsNullOrWhiteSpace(contents))
                    {
                        string[] auth = Utils.Base64Decode(contents).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (await HandleAuthResponse(await Login(auth[0], auth[1], true)))
                        {
                            return;
                        }
                    }
                }
                await PromptLogin();
            }
        }

        private async Task PromptLogin()
        {
            LoginTitle();
            string username = Input.GetInput("Username: ");
            string password = Input.GetInput("Password: ", true);
            Utils.SetConsoleTitle("Logging In...");
            Console.Clear();
            if (!await HandleAuthResponse(await Login(username, password)))
            {
                await PromptLogin();
                return;
            }
        }

        private async Task<bool> GetAPIKey()
        {
            HttpResponseMessage response = await _client.GetAsync("config?organization=vrchat");
            if (response.IsSuccessStatusCode)
            {
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
                if (data.TryGetValue("clientApiKey", out object key))
                {
                    _apiKey = key.ToString();
                    return true;
                }
            }
            Utils.WriteLine("Error getting API key", ConsoleColor.Red);
            return false;
        }

        private async Task<AuthResponse> Login(string username, string password, bool auto = false)
        {
            _username = username.ToLower();

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", CreateAuthHeader(username, password));

            HttpResponseMessage response = await _client.GetAsync($"auth/user?apiKey={_apiKey}&organization=vrchat");
            if (!response.IsSuccessStatusCode)
            {
                if (!auto) Utils.WriteLine($"{await response.Content.ReadAsStringAsync()}\n", ConsoleColor.Red);
                return AuthResponse.Fail;
            }
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
            if (Config.Current.RememberLogin)
            {
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/auth", Utils.Base64Encode(_username.ToLower() + "\n" + password));
            }
            if (data.ContainsKey("requiresTwoFactorAuth"))
            {
                return AuthResponse.Requires2FA;
            }
            UserInfo = JsonConvert.DeserializeObject<AuthUserInfo>(await response.Content.ReadAsStringAsync());
            return AuthResponse.Success;
        }

        private async Task<AuthResponse> Verify2FA(string code)
        {
            HttpResponseMessage response = await _client.PostAsync($"auth/twofactorauth/totp/verify?apiKey={_apiKey}&organization=vrchat", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code }
            }));
            if (!response.IsSuccessStatusCode) return AuthResponse.Requires2FA;
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
            if (!data.TryGetValue("verified", out object verified))
            {
                Utils.WriteLine($"{await response.Content.ReadAsStringAsync()}\n", ConsoleColor.Red);
                return AuthResponse.Fail;
            }
            if ((bool)verified)
            {
                response = await _client.GetAsync($"auth/user?apiKey={_apiKey}&organization=vrchat");
                if (!response.IsSuccessStatusCode) return AuthResponse.Fail;
                UserInfo = JsonConvert.DeserializeObject<AuthUserInfo>(await response.Content.ReadAsStringAsync());
                return AuthResponse.Success;
            }
            return AuthResponse.Requires2FA;
        }

        public async Task<bool> JoinWorld(string world)
        {
            HttpResponseMessage response = await _client.PutAsync($"joins?apiKey={_apiKey}&organization=vrchat", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "userId", UserInfo.id },
                { "worldId", world }
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Utils.Error($"[Join] Error spoofing world.");
                return false;
            }
            Utils.Log($"[Join] Spoofing world: {world}");
            return true;
        }

        public async Task<bool> VisitWorld(string world)
        {
            HttpResponseMessage response = await _client.PutAsync($"visits?apiKey={_apiKey}&organization=vrchat", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "userId", UserInfo.id },
                { "worldId", world }
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Utils.Error($"[Visit] Error spoofing world.");
                return false;
            }
            Utils.Log($"[Visit] Spoofing world: {world}");
            return true;
        }

        private async Task<bool> HandleAuthResponse(AuthResponse authResponse)
        {
            switch (authResponse)
            {
                case AuthResponse.Success:
                    Console.Clear();
                    Utils.SetConsoleTitle($"Logged In: {UserInfo.displayName} ({UserInfo.id})");
                    return true;

                case AuthResponse.Fail:
                default:
                    Utils.SetConsoleTitle("Not Logged In");
                    return false;

                case AuthResponse.Requires2FA:
                    Console.Clear();
                    LoginTitle();
                    Utils.SetConsoleTitle($"Logging In: {_username} • Enter 2FA Code");
                    string code = Input.GetInput("Enter 2FA code: ");
                    Console.Clear();
                    Utils.SetConsoleTitle("Checking 2FA Code...");
                    return await HandleAuthResponse(await Verify2FA(code));
            }
        }

        private static string CreateAuthHeader(string username, string password)
        {
            return "Basic " + Utils.Base64Encode($"{username}:{Utils.Escape(password)}");
        }

        private static void LoginTitle(bool clear = false)
        {
            if (clear) Console.Clear();
            Utils.WriteLine(@"-----------");
            Utils.WriteLine(@"   LOGIN   ");
            Utils.WriteLine(@"-----------");
            Utils.WriteLine();
        }

        public enum AuthResponse
        {
            Success,
            Fail,
            Requires2FA
        }

        public string Token { get; private set; }
        public AuthUserInfo UserInfo { get; private set; }

        private string _apiKey;
        private string _username;
        private readonly HttpClient _client;
    }
}
