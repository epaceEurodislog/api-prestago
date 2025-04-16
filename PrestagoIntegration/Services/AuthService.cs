using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PrestagoIntegration.Utils;

namespace PrestagoIntegration.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfig _config;
        private string _xsrfToken;
        private string _jsessionId;

        public AuthService(HttpClient httpClient, AppConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <summary>
        /// Authentification à l'API Prestago
        /// </summary>
        /// <returns>True si l'authentification a réussi, False sinon</returns>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                Logger.Log("Authentification à l'API Prestago");

                // Création de la requête d'authentification
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        login = _config.Login,
                        password = _config.Password
                    }),
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

                bool isAuthenticated = !string.IsNullOrEmpty(_xsrfToken) && !string.IsNullOrEmpty(_jsessionId);
                Logger.Log($"Authentification {(isAuthenticated ? "réussie" : "échouée")}");

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                Logger.LogError("Authentification Prestago", ex);
                return false;
            }
        }

        /// <summary>
        /// Extraction de la valeur d'un cookie à partir de sa chaîne complète
        /// </summary>
        /// <param name="cookie">Chaîne du cookie</param>
        /// <param name="cookieName">Nom du cookie à extraire</param>
        /// <returns>Valeur du cookie</returns>
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

        /// <summary>
        /// Ajoute les headers d'authentification à une requête HTTP
        /// </summary>
        /// <param name="request">Requête HTTP à modifier</param>
        public void AddAuthHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_xsrfToken) && !string.IsNullOrEmpty(_jsessionId))
            {
                request.Headers.Add("Cookie", $"XSRF-TOKEN={_xsrfToken}; JSESSIONID={_jsessionId}");
                request.Headers.Add("X-XSRF-TOKEN", _xsrfToken);
            }
        }

        /// <summary>
        /// Vérifie si les tokens d'authentification sont présents
        /// </summary>
        public bool HasValidTokens()
        {
            return !string.IsNullOrEmpty(_xsrfToken) && !string.IsNullOrEmpty(_jsessionId);
        }
    }
}