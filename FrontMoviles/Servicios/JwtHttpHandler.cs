using System.Net.Http.Headers;
using FrontMoviles.Servicios;

namespace FrontMoviles.Servicios
{
    /// <summary>
    /// Handler que intercepta todas las peticiones HTTP para agregar automáticamente
    /// los headers de autenticación JWT y manejar respuestas de error de autenticación
    /// </summary>
    public class JwtHttpHandler : DelegatingHandler
    {
        public JwtHttpHandler() : base(new HttpClientHandler())
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Agregar headers de autenticación si existe sesión activa
            AgregarHeadersAutenticacion(request);

            // Enviar la petición
            var response = await base.SendAsync(request, cancellationToken);

            // Manejar respuestas de error de autenticación
            await ManejarRespuestaAutenticacion(response);

            return response;
        }

        private void AgregarHeadersAutenticacion(HttpRequestMessage request)
        {
            try
            {
                // Verificar si hay sesión activa y no está expirada
                if (!SessionManager.EstaLogueado())
                    return;

                var token = SessionManager.ObtenerToken();
                var sessionId = SessionManager.ObtenerSessionId();

                // Agregar Authorization header con Bearer token
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Agregar SessionId header personalizado
                if (!string.IsNullOrEmpty(sessionId))
                {
                    request.Headers.Add("SessionId", sessionId);
                }

                // Agregar otros headers comunes
                request.Headers.Add("Accept", "application/json");

                // Header personalizado para identificar la aplicación
                request.Headers.Add("X-App-Version", "1.0");
                request.Headers.Add("X-Platform", "MAUI");

                System.Diagnostics.Debug.WriteLine($"Headers agregados a petición: {request.RequestUri}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando headers de autenticación: {ex.Message}");
            }
        }

        private async Task ManejarRespuestaAutenticacion(HttpResponseMessage response)
        {
            try
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Unauthorized: // 401
                        await ManejarTokenExpirado();
                        break;

                    case System.Net.HttpStatusCode.Forbidden: // 403
                        await ManejarAccesoProhibido();
                        break;

                    case System.Net.HttpStatusCode.BadRequest: // 400
                        // Verificar si es error de token inválido
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("token") || content.Contains("session"))
                        {
                            await ManejarTokenInvalido();
                        }
                        break;

                    default:
                        // Para otros códigos, extraer y actualizar token si viene en headers
                        ActualizarTokenDesdHeaders(response);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error manejando respuesta de autenticación: {ex.Message}");
            }
        }

        private async Task ManejarTokenExpirado()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Token expirado detectado - cerrando sesión");

                // Cerrar sesión local
                SessionManager.CerrarSesion();

                // Notificar al usuario (esto se ejecuta en background thread, así que usar Dispatcher)
                await NotificarTokenExpirado();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error manejando token expirado: {ex.Message}");
            }
        }

        private async Task ManejarAccesoProhibido()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Acceso prohibido detectado");

                // El token es válido pero no tiene permisos
                await NotificarAccesoProhibido();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error manejando acceso prohibido: {ex.Message}");
            }
        }

        private async Task ManejarTokenInvalido()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Token inválido detectado - cerrando sesión");

                // Cerrar sesión local
                SessionManager.CerrarSesion();

                await NotificarTokenInvalido();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error manejando token inválido: {ex.Message}");
            }
        }

        private void ActualizarTokenDesdHeaders(HttpResponseMessage response)
        {
            try
            {
                // Buscar nuevo token en headers de respuesta
                if (response.Headers.TryGetValues("New-Token", out var newTokenValues))
                {
                    var newToken = newTokenValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        SessionManager.ActualizarToken(newToken);
                        System.Diagnostics.Debug.WriteLine("Token actualizado desde headers de respuesta");
                    }
                }

                // Buscar refresh token
                if (response.Headers.TryGetValues("Refresh-Token", out var refreshTokenValues))
                {
                    var refreshToken = refreshTokenValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        // Aquí podrías implementar lógica de refresh token
                        System.Diagnostics.Debug.WriteLine("Refresh token recibido");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando token desde headers: {ex.Message}");
            }
        }

        #region Métodos de notificación

        private async Task NotificarTokenExpirado()
        {
            try
            {
                // Ejecutar en UI thread
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Navegar a login
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Sesión Expirada",
                                "Tu sesión ha expirado. Por favor, inicia sesión nuevamente.",
                                "OK");

                            // Navegar a login
                            Application.Current.MainPage = new AppShell();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en notificación UI: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notificando token expirado: {ex.Message}");
            }
        }

        private async Task NotificarAccesoProhibido()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Acceso Denegado",
                                "No tienes permisos para realizar esta acción.",
                                "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en notificación UI: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notificando acceso prohibido: {ex.Message}");
            }
        }

        private async Task NotificarTokenInvalido()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Sesión Inválida",
                                "Tu sesión no es válida. Por favor, inicia sesión nuevamente.",
                                "OK");

                            // Navegar a login
                            Application.Current.MainPage = new AppShell();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en notificación UI: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notificando token inválido: {ex.Message}");
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cleanup si es necesario
            }
            base.Dispose(disposing);
        }
    }
}