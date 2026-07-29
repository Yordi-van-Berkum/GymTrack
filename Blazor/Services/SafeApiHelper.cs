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


        // Voert een API actie uit waarbij alleen een string response wordt verwacht.
        // Deze helper wordt gebruikt voor acties zoals create, update en delete.
        public async Task<string> SafeActionApiCallAsync(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                // Voert de meegegeven HTTP-aanroep uit (bijvoorbeeld POST, PUT of DELETE)
                // Using HttpResponseMessage zorgt ervoor dat de response na gebruik netjes wordt opgeruimd.
                using HttpResponseMessage response = await httpCall();

                // Leest de response body als string.
                // Dit bevat een succes of foutmelding van de API
                string message = await response.Content.ReadAsStringAsync();

                // Controleert of de API-aanroep succesvol is uitgevoerd.
                if (!response.IsSuccessStatusCode)
                {
                    // Logt de statuscode en response voor debugging en monitoring zonder dat deze naar de gebruikers gestuurd worden.
                    _logger.LogWarning("API call failed. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, message);

                    // Geeft de foutmelding van de backend terug.
                    // Wanneer de backend geef foutmelding teruggeeft wordt er een algemene foutmelding teruggestuurd.
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? $"Request failed ({(int)response.StatusCode} - {response.ReasonPhrase})." : message);
                }

                // Geeft de succesmelding van de API terug aan de aanroepende service.
                return message;
            }
            catch (HttpRequestException ex)
            {
                // Wordt uitgevoerd wanneer de server niet bereikbaar is, bijvoorbeeld door netwerkproblemen of een offline API.
                _logger.LogError(ex, "Unable to reach the API.");
                throw new InvalidOperationException("Unable to connect to the server. Please check your internet connection.", ex);
            }
            catch (Exception ex)
            {
                // Wordt uitgevoerd wanneer er onverwachtte fouten zijn.
                _logger.LogError(ex, "Unexpected error during API call.");
                throw;
            }
        }

        // Voert een API-aanroep uit waarbij een JSON object wordt verwacht.
        // Deze helper wordt gebruikt voor het ophalen of ontvangen van data, zoals DTO's, lijsten en login responses.
        public async Task<TResponse> SafeDataApiCallAsync<TResponse>(Func<Task<HttpResponseMessage>> httpCall)
        {
            try
            {
                // Voert de meegegeven HTTP-aanroep uit (bijvoorbeeld GET, POST, PUT of DELETE).
                // Using HttpResponseMessage zorgt ervoor dat de response na gebruik netjes wordt opgeruimd.
                using HttpResponseMessage response = await httpCall();

                // Controleert of de API-aanroep succesvol is uitgevoerd.
                if (!response.IsSuccessStatusCode)
                {
                    // Leest de foutmelding die door de backend is teruggestuurd.
                    string message = await response.Content.ReadAsStringAsync();

                    // Logt de statuscode en response voor debugging en monitoring zonder dat deze naar de gebruikers gestuurd worden.
                    _logger.LogWarning("API call failed. StatusCode: {StatusCode}. Response: {Response}", response.StatusCode, message);

                    // Geeft de foutmelding van de backend terug.
                    // Wanneer de backend geef foutmelding teruggeeft wordt er een algemene foutmelding teruggestuurd.
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? $"Request failed ({(int)response.StatusCode} - {response.ReasonPhrase})." : message);
                }

                // Zet de JSON response om naar het gewenste model
                return (await response.Content.ReadFromJsonAsync<TResponse>())!;
            }
            catch (HttpRequestException ex)
            {
                // Wordt uitgevoerd wanneer de server niet bereikbaar is, bijvoorbeeld door netwerkproblemen of een offline API.
                _logger.LogError(ex, "Unable to reach the API.");
                throw new InvalidOperationException("Unable to connect to the server. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                // Wordt uitgevoerd wanneer de ontvangen JSON niet kan worden omgezet naar het opgegeven model.
                _logger.LogError(ex, "Failed to deserialize the API response.");
                throw new InvalidOperationException("Unexpected response received from the server.", ex);
            }
            catch (Exception ex)
            {
                // Wordt uitgevoerd wanneer er een onverwachte fout optreedt.
                _logger.LogError(ex, "Unexpected error during API call.");
                throw;
            }
        }

    }
}