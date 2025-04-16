using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PrestagoIntegration.Models;
using PrestagoIntegration.Services;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== INTEGRATION PRESTAGO - RECEPTION NSE ===");

            // Chargement de la configuration
            var config = AppConfig.Instance;

            // Si la configuration n'est pas définie, demander à l'utilisateur
            if (string.IsNullOrEmpty(config.Login) || string.IsNullOrEmpty(config.Password))
            {
                Console.WriteLine("Configuration initiale nécessaire:");

                Console.Write("Login Prestago: ");
                config.Login = Console.ReadLine();

                Console.Write("Mot de passe Prestago: ");
                config.Password = ReadPassword();

                Console.Write("Code dépôt par défaut [600048]: ");
                string depotCode = Console.ReadLine();
                if (!string.IsNullOrEmpty(depotCode))
                {
                    config.DefaultStockOutletCode = depotCode;
                }

                // Sauvegarder la configuration
                AppConfig.SaveConfig(config);
            }

            // Création du HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(config.ApiUrl)
            };

            // Initialisation des services
            var authService = new AuthService(httpClient, config);
            var receptionService = new ReceptionService(httpClient, authService, config);

            // Menu principal
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nMenu Principal:");
                Console.WriteLine("1. Envoyer une réception NSE");
                Console.WriteLine("2. Tester l'authentification");
                Console.WriteLine("3. Modifier la configuration");
                Console.WriteLine("4. Quitter");

                Console.Write("\nVotre choix: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await EnvoyerReceptionNSE(receptionService, config);
                        break;
                    case "2":
                        await TesterAuthentification(authService);
                        break;
                    case "3":
                        ModifierConfiguration(config);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Choix non valide. Veuillez réessayer.");
                        break;
                }
            }

            Console.WriteLine("Au revoir!");
        }

        static async Task EnvoyerReceptionNSE(ReceptionService receptionService, AppConfig config)
        {
            Console.WriteLine("\n=== ENVOI RECEPTION NSE ===");

            // Demander le code dépôt
            Console.Write($"Code dépôt [{config.DefaultStockOutletCode}]: ");
            string stockOutletCode = Console.ReadLine();
            if (string.IsNullOrEmpty(stockOutletCode))
            {
                stockOutletCode = config.DefaultStockOutletCode;
            }

            // Création de la liste des NSE
            var nseItems = new List<NSEItem>();
            bool continueAdding = true;

            while (continueAdding)
            {
                Console.WriteLine("\nAjouter un NSE:");

                var nseItem = new NSEItem
                {
                    StockOutletCode = stockOutletCode
                };

                Console.Write("Code équipement: ");
                nseItem.EquipmentCode = Console.ReadLine();

                Console.Write("Nom équipement: ");
                nseItem.EquipmentName = Console.ReadLine();

                Console.Write("Numéro de série: ");
                nseItem.SerialNumber = Console.ReadLine();

                Console.Write("Status [AVAILABLE]: ");
                string status = Console.ReadLine();
                if (!string.IsNullOrEmpty(status))
                {
                    nseItem.Status = status;
                }

                Console.Write("Numéro d'intervention: ");
                nseItem.InterventionNumber = Console.ReadLine() ?? "";

                nseItems.Add(nseItem);

                Console.Write("\nAjouter un autre NSE? (O/N): ");
                continueAdding = Console.ReadLine()?.ToUpper() == "O";
            }

            // Envoi de la réception
            Console.WriteLine("\nEnvoi de la réception en cours...");
            bool result = await receptionService.SendReceptionAsync(stockOutletCode, nseItems);

            if (result)
            {
                Console.WriteLine("Réception envoyée avec succès!");
            }
            else
            {
                Console.WriteLine("Échec de l'envoi de la réception. Consultez les logs pour plus de détails.");
            }
        }

        static async Task TesterAuthentification(AuthService authService)
        {
            Console.WriteLine("\nTest d'authentification en cours...");
            bool result = await authService.AuthenticateAsync();

            if (result)
            {
                Console.WriteLine("Authentification réussie!");
            }
            else
            {
                Console.WriteLine("Échec de l'authentification. Vérifiez vos identifiants.");
            }
        }

        static void ModifierConfiguration(AppConfig config)
        {
            Console.WriteLine("\n=== MODIFICATION CONFIGURATION ===");

            Console.Write($"Login Prestago [{config.Login}]: ");
            string login = Console.ReadLine();
            if (!string.IsNullOrEmpty(login))
            {
                config.Login = login;
            }

            Console.Write("Mot de passe Prestago (laisser vide pour ne pas changer): ");
            string password = ReadPassword();
            if (!string.IsNullOrEmpty(password))
            {
                config.Password = password;
            }

            Console.Write($"Code dépôt par défaut [{config.DefaultStockOutletCode}]: ");
            string depotCode = Console.ReadLine();
            if (!string.IsNullOrEmpty(depotCode))
            {
                config.DefaultStockOutletCode = depotCode;
            }

            Console.Write($"URL API [{config.ApiUrl}]: ");
            string apiUrl = Console.ReadLine();
            if (!string.IsNullOrEmpty(apiUrl))
            {
                config.ApiUrl = apiUrl;
            }

            // Sauvegarder la configuration
            AppConfig.SaveConfig(config);
            Console.WriteLine("Configuration sauvegardée!");
        }

        static string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}