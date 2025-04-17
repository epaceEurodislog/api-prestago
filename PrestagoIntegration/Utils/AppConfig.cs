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

        // Ajout de la propriété manquante
        public string TargetStockCode { get; set; } = "";

        private static AppConfig? _instance;
        private static readonly string CONFIG_FILE = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "PrestagoIntegration",
    "config.json");


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
                // Assurez-vous que le dossier existe
                string directory = Path.GetDirectoryName(CONFIG_FILE);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

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
                // Assurez-vous que le dossier existe
                string directory = Path.GetDirectoryName(CONFIG_FILE);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
                Console.WriteLine($"Configuration sauvegardée dans {CONFIG_FILE}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde de la configuration : {ex.Message}");
            }
        }
    }
}