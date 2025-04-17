using System;
using System.Net.Http;
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
        /// Authentification à l'API Prestago en utilisant l'authentification Basic
        /// </summary>
        /// <returns>True si l'authentification a réussi, False sinon</returns>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                Logger.Log("Authentification à l'API Prestago");

                // Préparation de l'authentification Basic
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Login}:{_config.Password}"));

                // Création de la requête d'authentification
                var request = new HttpRequestMessage(HttpMethod.Get, "api/user");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                // Envoi de la requête d'authentification
                var response = await _httpClient.SendAsync(request);

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

                // Si on n'a pas de XSRF-TOKEN, on regarde dans les cookies de la réponse
                if (string.IsNullOrEmpty(_xsrfToken))
                {
                    var responseCookies = response.Headers.GetValues("Set-Cookie");
                    foreach (var cookie in responseCookies)
                    {
                        if (cookie.Contains("XSRF-TOKEN"))
                        {
                            _xsrfToken = ExtractCookieValue(cookie, "XSRF-TOKEN");
                            break;
                        }
                    }
                }

                // Une deuxième requête pour obtenir le XSRF-TOKEN si nécessaire
                if (string.IsNullOrEmpty(_xsrfToken) && !string.IsNullOrEmpty(_jsessionId))
                {
                    Logger.Log("Tentative d'obtention du XSRF-TOKEN via une seconde requête");

                    var secondRequest = new HttpRequestMessage(HttpMethod.Get, "api/user");
                    secondRequest.Headers.Add("Cookie", $"JSESSIONID={_jsessionId}");

                    var secondResponse = await _httpClient.SendAsync(secondRequest);

                    if (secondResponse.IsSuccessStatusCode &&
                        secondResponse.Headers.TryGetValues("Set-Cookie", out var secondCookies))
                    {
                        foreach (var cookie in secondCookies)
                        {
                            if (cookie.Contains("XSRF-TOKEN"))
                            {
                                _xsrfToken = ExtractCookieValue(cookie, "XSRF-TOKEN");
                                break;
                            }
                        }
                    }
                }

                bool isAuthenticated = !string.IsNullOrEmpty(_jsessionId); // Même sans XSRF, on peut être authentifié
                Logger.Log($"Authentification {(isAuthenticated ? "réussie" : "échouée")}");
                if (isAuthenticated)
                {
                    Logger.Log($"JSESSIONID: {_jsessionId}");
                    Logger.Log($"XSRF-TOKEN: {_xsrfToken ?? "Non disponible"}");
                }

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
            string cookieHeader = "";

            if (!string.IsNullOrEmpty(_jsessionId))
            {
                cookieHeader += $"JSESSIONID={_jsessionId}";
            }

            if (!string.IsNullOrEmpty(_xsrfToken))
            {
                if (!string.IsNullOrEmpty(cookieHeader))
                    cookieHeader += "; ";

                cookieHeader += $"XSRF-TOKEN={_xsrfToken}";

                // Ajouter également le header X-XSRF-TOKEN
                request.Headers.Add("X-XSRF-TOKEN", _xsrfToken);
            }

            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
        }

        /// <summary>
        /// Vérifie si les tokens d'authentification sont présents
        /// </summary>
        public bool HasValidTokens()
        {
            return !string.IsNullOrEmpty(_jsessionId); // Le JSESSIONID est l'essentiel
        }
    }
}