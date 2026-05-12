using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TTManager.Communication.REST
{
    public enum RestApiClientState
    {
        Connected,
        Disconnected,
        Sent,
        Received,
        Error
    }

    public sealed class RestApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new();
    }

    public sealed class RestApiClientHelper : IDisposable
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private bool _disposed;
        #endregion

        #region Properties
        public string BaseUrl { get; set; } = string.Empty;
        public string? BearerToken { get; set; }
        public string? BasicUsername { get; set; }
        public string? BasicPassword { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public Dictionary<string, string> DefaultHeaders { get; } = new();
        #endregion

        #region Events
        public delegate void RestApiClientEventHandler(RestApiClientState state, string data);
        public event RestApiClientEventHandler? RestApiClientCallback;
        #endregion

        #region Constructors
        public RestApiClientHelper(string baseUrl = "")
        {
            BaseUrl = baseUrl;
            _httpClient = new HttpClient();
            OnRestApiClientCallback(RestApiClientState.Connected, "REST client ready");
        }
        #endregion

        #region Private Methods
        private void OnRestApiClientCallback(RestApiClientState state, string data)
        {
            RestApiClientCallback?.Invoke(state, data);
        }

        private Uri BuildUri(string endpoint)
        {
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? absoluteUri))
            {
                return absoluteUri;
            }

            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new InvalidOperationException("BaseUrl is empty and endpoint is not absolute.");
            }

            string baseUrl = BaseUrl.EndsWith("/") ? BaseUrl : BaseUrl + "/";
            string relativePath = endpoint.TrimStart('/');
            return new Uri(new Uri(baseUrl), relativePath);
        }

        private void ApplyHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
            }
            else if (!string.IsNullOrWhiteSpace(BasicUsername))
            {
                string rawAuth = $"{BasicUsername}:{BasicPassword ?? string.Empty}";
                string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawAuth));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
            }

            foreach (KeyValuePair<string, string> header in DefaultHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (headers == null) return;

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Remove(header.Key);
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private static HttpContent? CreateJsonContent(object? body)
        {
            if (body == null) return null;

            string json = body is string text ? text : JsonConvert.SerializeObject(body);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<RestApiResponse> SendAsync(HttpMethod method, string endpoint, object? body, Dictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            try
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(TimeoutSeconds));
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
                using HttpRequestMessage request = new(method, BuildUri(endpoint));

                ApplyHeaders(request, headers);
                request.Content = CreateJsonContent(body);

                OnRestApiClientCallback(RestApiClientState.Sent, $"{method} {request.RequestUri}");

                using HttpResponseMessage response = await _httpClient.SendAsync(request, linkedCts.Token);
                string content = await response.Content.ReadAsStringAsync(linkedCts.Token);

                RestApiResponse apiResponse = new()
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase ?? string.Empty,
                    Content = content
                };

                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                {
                    apiResponse.Headers[header.Key] = header.Value;
                }

                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
                {
                    apiResponse.Headers[header.Key] = header.Value;
                }

                OnRestApiClientCallback(
                    response.IsSuccessStatusCode ? RestApiClientState.Received : RestApiClientState.Error,
                    $"{apiResponse.StatusCode} {apiResponse.ReasonPhrase}: {content}");

                return apiResponse;
            }
            catch (TaskCanceledException ex)
            {
                string message = ex.CancellationToken.IsCancellationRequested
                    ? "Request canceled"
                    : $"Request timeout after {TimeoutSeconds} seconds";
                OnRestApiClientCallback(RestApiClientState.Error, message);
                return new RestApiResponse { Success = false, StatusCode = 0, ReasonPhrase = message, Content = ex.Message };
            }
            catch (Exception ex)
            {
                OnRestApiClientCallback(RestApiClientState.Error, ex.Message);
                return new RestApiResponse { Success = false, StatusCode = 0, ReasonPhrase = ex.GetType().Name, Content = ex.Message };
            }
        }
        #endregion

        #region Public Methods
        public Task<RestApiResponse> GetAsync(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Get, endpoint, null, headers, cancellationToken);
        }

        public Task<RestApiResponse> PostAsync(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Post, endpoint, body, headers, cancellationToken);
        }

        public Task<RestApiResponse> PutAsync(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Put, endpoint, body, headers, cancellationToken);
        }

        public Task<RestApiResponse> PatchAsync(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Patch, endpoint, body, headers, cancellationToken);
        }

        public Task<RestApiResponse> DeleteAsync(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return SendAsync(HttpMethod.Delete, endpoint, body, headers, cancellationToken);
        }

        public async Task<T?> GetJsonAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            RestApiResponse response = await GetAsync(endpoint, headers, cancellationToken);
            return response.Success && !string.IsNullOrWhiteSpace(response.Content)
                ? JsonConvert.DeserializeObject<T>(response.Content)
                : default;
        }

        public async Task<T?> PostJsonAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            RestApiResponse response = await PostAsync(endpoint, body, headers, cancellationToken);
            return response.Success && !string.IsNullOrWhiteSpace(response.Content)
                ? JsonConvert.DeserializeObject<T>(response.Content)
                : default;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _httpClient.Dispose();
            _disposed = true;
            OnRestApiClientCallback(RestApiClientState.Disconnected, "REST client disposed");
        }
        #endregion
    }
}
