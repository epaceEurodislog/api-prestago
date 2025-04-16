
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
        private readonly AppConfig _config;

        public ReceptionService(HttpClient httpClient, AuthService authService, AppConfig config)
        {
            _httpClient = httpClient;
            _authService = authService;
            _config = config;
        }

        /// <summary>
        /// Envoi d'une réception de NSE à l'API Prestago
        /// </summary>
        /// <param name="stockOutletCode">Code du dépôt Prestago</param>
        /// <param name="nseItems">Liste des NSE à réceptionner</param>
        /// <returns>True si l'opération a réussi, False sinon</returns>
        public async Task<bool> SendReceptionAsync(string stockOutletCode, List<NSEItem> nseItems)
        {
            try
            {
                Logger.Log($"Envoi de la réception des NSE pour le dépôt {stockOutletCode}");

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
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/reception");

                // Ajout des headers d'authentification
                _authService.AddAuthHeaders(request);

                // Préparation du contenu de la requête
                var requestBody = new
                {
                    stockOutletCode = stockOutletCode,
                    equipments = nseItems
                };

                var json = JsonSerializer.Serialize(requestBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoi de la requête
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                Logger.Log($"Réponse de l'API : {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Logger.Log("Réception envoyée avec succès");

                    // On peut analyser le contenu de la réponse pour extraire plus de détails
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (responseObject.TryGetProperty("result", out var resultProperty) &&
                        resultProperty.GetBoolean())
                    {
                        return true;
                    }
                    else
                    {
                        string errorMessage = "Erreur dans la réponse de l'API";
                        if (responseObject.TryGetProperty("message", out var messageProperty))
                        {
                            errorMessage = messageProperty.GetString();
                        }
                        Logger.Log($"Erreur de réception : {errorMessage}");
                        return false;
                    }
                }
                else
                {
                    Logger.Log($"Erreur lors de l'envoi de la réception : {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("SendReceptionAsync", ex);
                return false;
            }
        }
    }
}
