// Services/ReceptionService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PrestagoIntegration.Models;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Services
{
    public class ReceptionService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ReceptionService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<bool> SendReceptionAsync(string stockOutletCode, List<ReceptionItem> items)
        {
            try
            {
                Logger.Log($"Envoi de réception pour le dépôt {stockOutletCode}");

                // S'assurer que nous sommes authentifiés
                if (!_authService.HasValidTokens())
                {
                    bool success = await _authService.AuthenticateAsync();
                    if (!success)
                    {
                        Logger.Log("Échec de l'authentification, impossible d'envoyer la réception");
                        return false;
                    }
                }

                // Créer la requête POST
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/reception");

                // Ajouter les en-têtes d'authentification
                _authService.AddAuthHeadersToRequest(request);

                // Ajouter le contenu JSON (un tableau d'objets)
                var json = JsonSerializer.Serialize(items);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoyer la requête
                var response = await _httpClient.SendAsync(request);

                // Vérifier le code de statut (204 = succès)
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Logger.Log("Réception réussie (statut 204)");
                    return true;
                }
                else
                {
                    // En cas d'erreur, essayer de récupérer le message d'erreur
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Échec de la réception. Statut: {response.StatusCode}, Détails: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception lors de l'envoi de la réception", ex);
                return false;
            }
        }
    }
}