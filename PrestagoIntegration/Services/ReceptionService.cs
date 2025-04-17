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
                // IMPORTANT: Le corps est directement la liste des NSE (pas d'objet parent)
                var json = JsonSerializer.Serialize(nseItems);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoi de la requête
                var response = await _httpClient.SendAsync(request);

                var statusCode = response.StatusCode;
                Logger.Log($"Réponse de l'API : {statusCode}");

                // Succès si code 204 (No Content)
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Logger.Log("Réception envoyée avec succès");
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
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