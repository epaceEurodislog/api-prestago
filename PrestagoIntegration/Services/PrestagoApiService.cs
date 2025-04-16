using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PrestagoIntegration.Models;

namespace PrestagoIntegration.Services
{
    public class PrestagoApiService
    {
        private readonly HttpClient _httpClient;
        private string _xsrfToken;
        private string _jsessionId;
        private string _login;
        private string _password;

        public PrestagoApiService(string baseUrl, string login, string password)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _login = login;
            _password = password;
        }

        public async Task<bool> AuthenticateAsync(string login, string password)
        {
            try
            {
                _login = login;
                _password = password;

                // Création de la requête d'authentification
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(new { login, password }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Envoi de la requête d'authentification
                var response = await _httpClient.PostAsync("api/user", requestContent);
                response.EnsureSuccessStatusCode();

                // Récupération des cookies XSRF-TOKEN et JSESSIONID
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        if (cookie.Contains("XSRF-TOKEN"))
                        {
                            _xsrfToken = ExtractCookieValue(cookie, "XSRF-TOKEN");
                        }
                        else if (cookie.Contains("JSESSIONID"))
                        {
                            _jsessionId = ExtractCookieValue(cookie, "JSESSIONID");
                        }
                    }
                }

                return !string.IsNullOrEmpty(_xsrfToken) && !string.IsNullOrEmpty(_jsessionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'authentification: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendReceptionAsync(string stockOutletCode, List<ReceptionItem> items)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                    return false;

                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/reception");

                // Ajouter les headers d'authentification
                AddAuthHeaders(request);

                // Le corps de la requête est directement la liste des items
                var json = JsonSerializer.Serialize(items);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                // Succès si code 204 (No Content)
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la réception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendExpeditionAsync(string stockOutletCode, ExpeditionRequest expedition)
        {
            try
            {
                if (!await EnsureAuthenticatedAsync())
                    return false;

                var request = new HttpRequestMessage(HttpMethod.Post, $"api/stock-equipments/{stockOutletCode}/removal-send");

                // Ajouter les headers d'authentification
                AddAuthHeaders(request);

                var json = JsonSerializer.Serialize(expedition);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                // Succès si code 204 (No Content)
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'expédition: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> EnsureAuthenticatedAsync()
        {
            if (string.IsNullOrEmpty(_xsrfToken) || string.IsNullOrEmpty(_jsessionId))
            {
                // Ré-authentification nécessaire
                return await AuthenticateAsync(_login, _password);
            }
            return true;
        }

        private void AddAuthHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Cookie", $"XSRF-TOKEN={_xsrfToken}; JSESSIONID={_jsessionId}");
            request.Headers.Add("X-XSRF-TOKEN", _xsrfToken);
        }

        private string ExtractCookieValue(string cookie, string cookieName)
        {
            var parts = cookie.Split(';');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (trimmedPart.StartsWith($"{cookieName}="))
                {
                    return trimmedPart.Substring(cookieName.Length + 1);
                }
            }
            return null;
        }
    }
}