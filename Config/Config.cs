using System;
using System.IO;
using Newtonsoft.Json;

namespace VRC
{
    public class Config
    {
        static Config()
        {
            // Load config
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/Config.json"))
            {
                File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}/Config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                _current = null;
                return;
            }
            string json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/Config.json");
            _current = JsonConvert.DeserializeObject<Config>(json);
        }

        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new Config();
                }
                return _current;
            }
        }

        public bool RememberLogin { get; set; }

        private static Config _current;
    }
}