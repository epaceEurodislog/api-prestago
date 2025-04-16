
using System;
using System.IO;
using System.Text.Json;

namespace PrestagoIntegration.Utils
{
    public class AppConfig
    {
        // Configuration Prestago
        public string ApiUrl { get; set; } = "https://prestago-test.pmu.fr/";
        public string Login { get; set; }
        public string Password { get; set; }
        
        // Configuration par défaut
        public string DefaultStockOutletCode { get; set; } = "600048";

        private static AppConfig _instance;
        private static readonly string CONFIG_FILE = "config.json";

        private AppConfig() { }

        public static AppConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadConfig();
                }
                return _instance;
            }
        }

        private static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de la configuration : {ex.Message}");
            }
            
            // Configuration par défaut si fichier non trouvé ou erreur
            var config = new AppConfig();
            SaveConfig(config);
            return config;
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de la configuration : {ex.Message}");
            }
        }
    }
}
