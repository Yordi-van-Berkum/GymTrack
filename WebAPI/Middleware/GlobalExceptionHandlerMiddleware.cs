using System.Diagnostics;
using System.Net;
using System.Text.Json;
using WebAPI.Exceptions;

namespace WebAPI.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Laat de request doorgaan naar de volgende middleware en uiteindelijk naar de controller.
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // De request is geannuleerd, bijvoorbeeld doordat de gebruiker de pagina verlaat.
                _logger.LogInformation("Request was cancelled by the client. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);
            }
            catch (NotFoundException ex)
            {
                // Geeft een 404 terug wanneer de gevraagde gegevens niet bestaan.
                _logger.LogWarning(ex, "Resource not found. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (ConflictException ex)
            {
                // Geeft een 409 terug wanneer de actie niet uitgevoerd kan worden door een bestaand conflict.
                _logger.LogWarning(ex, "Request conflict. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Geeft een 403 terug wanneer de gebruiker geen toegang heeft tot de gegevens.
                _logger.LogWarning(ex, "Unauthorized access attempt. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context, HttpStatusCode.Unauthorized, "You do not have permission to access this resource.");
            }
            catch (ArgumentException ex)
            {
                // Geeft een 400 terug wanneer de meegestuurde gegevens niet geldig zijn.
                _logger.LogWarning(ex, "Invalid argument. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (InvalidCredentialsException ex)
            {
                // Geeft een 401 terug wanneer het e-mailadres of wachtwoord niet klopt.
                _logger.LogWarning(ex, "Invalid login attempt. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context,HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                // Verwerkt onverwachte fouten zonder interne serverinformatie naar de gebruiker te sturen.
                _logger.LogError(ex, "Unhandled exception occurred. Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);

                await WriteResponseAsync(context, HttpStatusCode.InternalServerError, "Something went wrong.");
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            // Voorkomt dat de response wordt aangepast nadat deze al naar de client is verstuurd.
            if (context.Response.HasStarted)
                return;

            // Zet de HTTP-statuscode van de response, bijvoorbeeld 404, 409 of 500.
            context.Response.StatusCode = (int)statusCode;

            // Geeft aan dat de response JSON bevat.
            context.Response.ContentType = "application/json";

            // Maakt een object met de foutmelding die naar de frontend wordt gestuurd.
            var response = new
            {
                message
            };

            // Stuurt de foutmelding als JSON terug naar de frontend
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}