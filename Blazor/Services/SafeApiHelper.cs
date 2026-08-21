using System.Net.Http.Json;
using System.Text.Json;

namespace Blazor.Services
{
    public class SafeApiHelper
    {
        private readonly ILogger<SafeApiHelper> _logger;

        public SafeApiHelper(ILogger<SafeApiHelper> logger)
        {
            _logger = logger;
        }

        // Voert een API-actie uit waarbij een string response wordt verwacht.
        // Wordt gebruikt voor acties zoals create, update en delete.
        public async Task<string> SafeActionApiCallAsync(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                // Voert de meegegeven HTTP-aanroep uit.
                // De response wordt automatisch opgeruimd wanneer deze niet meer nodig is.
                using HttpResponseMessage response = await httpCall();

                // Leest de response van de API als tekst.
                string responseBody = await response.Content.ReadAsStringAsync();

                // Controleert of de API een succesvolle statuscode heeft teruggegeven.
                if (!response.IsSuccessStatusCode)
                {
                    // Logt de fout voor debugging en monitoring.
                    _logger.LogWarning("API call failed. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, responseBody);

                    // Geeft de foutmelding van de API door aan de frontend.
                    throw new InvalidOperationException(GetErrorMessage(responseBody, response));
                }

                // Geeft de succesvolle response terug aan de aanroeper.
                return responseBody;
            }
            catch (HttpRequestException ex)
            {
                // Wordt uitgevoerd wanneer de API niet bereikbaar is.
                _logger.LogError(ex, "Unable to reach the API");

                // Geeft een duidelijke foutmelding terug aan de gebruiker.
                throw new InvalidOperationException("Unable to connect to the server. Please check your internet connection.", ex);
            }
            catch (Exception ex)
            {
                // Vangt onverwachte fouten op en logt deze.
                _logger.LogError(ex, "Unexpected error during API call.");

                // Geeft de oorspronkelijke exception verder door.
                throw;
            }
        }


        // Voert een API-aanroep uit waarbij een JSON-object wordt verwacht.
        // Wordt gebruikt voor het ophalen van data zoals DTO's en lijsten.
        public async Task<TResponse> SafeDataApiCallAsync<TResponse>(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                // Voert de meegegeven HTTP-aanroep uit.
                using HttpResponseMessage response = await httpCall();

                // Controleert of de API een succesvolle statuscode heeft teruggegeven.
                if (!response.IsSuccessStatusCode)
                {
                    // Leest de foutresponse van de API.
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Logt de fout voor debugging en monitoring.
                    _logger.LogWarning("API call failed. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, responseBody);

                    // Geeft de foutmelding van de API door aan de frontend.
                    throw new InvalidOperationException(GetErrorMessage(responseBody, response));
                }

                // Zet de JSON-response om naar het gewenste C#-model.
                return (await response.Content.ReadFromJsonAsync<TResponse>())!;
            }
            catch (HttpRequestException ex)
            {
                // Wordt uitgevoerd wanneer de API niet bereikbaar is.
                _logger.LogError(ex, "Unable to reach the API");

                // Geeft een duidelijke foutmelding terug aan de gebruiker.
                throw new InvalidOperationException("Unable to connect to the server. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                // Wordt uitgevoerd wanneer de JSON-response niet goed naar het gewenste model kan worden omgezet.
                _logger.LogError(ex, "Failed to deserialize the API response.");

                // Geeft een algemene foutmelding terug.
                throw new InvalidOperationException("Unexpected response received from the server.", ex);
            }
            catch (Exception ex)
            {
                // Vangt onverwachte fouten op en logt deze.
                _logger.LogError(ex, "Unexpected error during API call.");

                // Geeft de oorspronkelijke exception verder door.
                throw;
            }
        }


        // Haalt de foutmelding uit de JSON-response van de API.
        // Als de API geen geldige foutmelding terugstuurt, wordt een algemene foutmelding gebruikt.
        private static string GetErrorMessage(string responseBody, HttpResponseMessage response)
        {
            // Controleert of de API een response heeft teruggestuurd.
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    // Probeert de JSON-response om te zetten naar ApiErrorResponse.
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });


                    // Geeft de message uit de API-response terug wanneer deze bestaat.
                    if (!string.IsNullOrWhiteSpace(errorResponse?.Message))
                        return errorResponse.Message;
                }
                catch (JsonException)
                {
                    // Als de response geen geldige JSON is,
                    // wordt hieronder een algemene foutmelding gebruikt.
                }
            }

            // Algemene foutmelding wanneer er geen bruikbare message beschikbaar is.
            return $"Request failed ({(int)response.StatusCode} - {response.ReasonPhrase}).";
        }


        // Model waarmee de message uit een API-foutresponse wordt gelezen.
        private sealed class ApiErrorResponse
        {
            public string? Message { get; set; }
        }
    }
}