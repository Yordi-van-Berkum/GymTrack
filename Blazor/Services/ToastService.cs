using System;

namespace Blazor.Services
{
    // Enum die bepaalt welk type toast wordt getoond
    // Dit wordt gebruikt voor styling (kleur) en betekenis
    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastService
    {

        // Het event dat wordt afgevuurd naar de razor pagina.
        public event Action<string, ToastLevel>? OnShow;

        // Functie om een toastmelding te tonen. Het Level is standaard info. 
        public void ShowToast(string message, ToastLevel level = ToastLevel.Info)
        {
            // Als de ShowToast wordt aangeroepen voeg message en level eraan toe.
            OnShow?.Invoke(message, level);
        }
    }
}