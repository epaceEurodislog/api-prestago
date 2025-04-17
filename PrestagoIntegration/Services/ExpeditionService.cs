// Services/ExpeditionService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PrestagoIntegration.Models;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Services
{
    public class ExpeditionService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ExpeditionService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<bool> SendExpeditionAsync(string stockOutletCode, ExpeditionRequest expeditionRequest)
        {
            try
            {
                Logger.Log($"Envoi d'expédition pour le dépôt {stockOutletCode}");

                // S'assurer que nous sommes authentifiés
                if (!_authService.HasValidTokens())
                {
                    bool success = await _authService.AuthenticateAsync();
                    if (!success)
                    {
                        Logger.Log("Échec de l'authentification, impossible d'envoyer l'expédition");
                        return false;
                    }
                }

                // Créer la requête POST
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/removal-send");

                // Ajouter les en-têtes d'authentification
                _authService.AddAuthHeadersToRequest(request);

                // Ajouter le contenu JSON
                var json = JsonSerializer.Serialize(expeditionRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoyer la requête
                var response = await _httpClient.SendAsync(request);

                // Vérifier le code de statut (204 = succès)
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Logger.Log("Expédition réussie (statut 204)");
                    return true;
                }
                else
                {
                    // En cas d'erreur, essayer de récupérer le message d'erreur
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Échec de l'expédition. Statut: {response.StatusCode}, Détails: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception lors de l'envoi de l'expédition", ex);
                return false;
            }
        }
    }
}