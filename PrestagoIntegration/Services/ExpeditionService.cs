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
                Logger.Log($"Envoi de l'expédition des NSE pour le dépôt {stockOutletCode}");

                // S'assurer que l'authentification est valide
                if (!_authService.HasValidTokens())
                {
                    bool authenticated = await _authService.AuthenticateAsync();
                    if (!authenticated)
                    {
                        Logger.Log("Échec de l'authentification à l'API Prestago");
                        return false;
                    }
                }

                // Préparation de la requête
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/removal-send");

                // Ajout des headers d'authentification
                _authService.AddAuthHeaders(request);

                // Préparation du contenu de la requête
                var json = JsonSerializer.Serialize(expeditionRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoi de la requête
                var response = await _httpClient.SendAsync(request);

                Logger.Log($"Réponse de l'API : {response.StatusCode}");

                // Succès si code 204 (No Content)
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Logger.Log("Expédition envoyée avec succès");
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Logger.Log($"Erreur lors de l'envoi de l'expédition : {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("SendExpeditionAsync", ex);
                return false;
            }
        }
    }
}