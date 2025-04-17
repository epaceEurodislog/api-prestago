using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PrestagoIntegration.Models;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Services
{
    public class PrestagoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly string _baseUrl;
        private readonly string _login;
        private readonly string _password;

        public PrestagoService(string baseUrl, string login, string password)
        {
            _baseUrl = baseUrl;
            _login = login;
            _password = password;

            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri(baseUrl)
            };

            _authService = new AuthService(_httpClient, new AppConfig
            {
                ApiUrl = baseUrl,
                Login = login,
                Password = password
            });
        }

        public async Task<bool> AuthenticateAsync()
        {
            return await _authService.AuthenticateAsync();
        }

        public async Task<bool> SendReceptionAsync(string stockOutletCode, List<ReceptionItem> items)
        {
            try
            {
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

                // Préparation du contenu de la requête - NOTEZ que c'est directement la liste des items
                var json = System.Text.Json.JsonSerializer.Serialize(items);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Envoi de la requête
                var response = await _httpClient.SendAsync(request);

                // Succès si code 204 (No Content)
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (System.Exception ex)
            {
                Logger.LogError("SendReceptionAsync", ex);
                return false;
            }
        }

        public async Task<bool> SendExpeditionAsync(string stockOutletCode, ExpeditionRequest expeditionRequest)
        {
            try
            {
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
                var json = System.Text.Json.JsonSerializer.Serialize(expeditionRequest);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Envoi de la requête
                var response = await _httpClient.SendAsync(request);

                // Succès si code 204 (No Content)
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (System.Exception ex)
            {
                Logger.LogError("SendExpeditionAsync", ex);
                return false;
            }
        }
    }
}