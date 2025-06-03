using System;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using FrontMoviles.Modelos;

namespace FrontMoviles.Servicios
{
    public static class SessionManager
    {
        private const string SESSION_ID_KEY = "SessionId";
        private const string TOKEN_KEY = "Token";
        private const string IS_LOGGED_IN_KEY = "IsLoggedIn";
        private const string USER_EMAIL_KEY = "UserEmail";
        private const string USER_ID_KEY = "UserId";
        private const string USER_NAME_KEY = "UserName";
        private const string SESSION_DATA_KEY = "SessionData";

        #region Métodos principales de sesión

        public static void GuardarSesion(Sesion sesion, string userEmail, string userName = null, int userId = 0)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("💾 === GUARDANDO SESIÓN ===");

                // Guardar datos básicos de sesión
                Preferences.Set(SESSION_ID_KEY, sesion.SesionId ?? string.Empty);
                Preferences.Set(TOKEN_KEY, sesion.Token ?? string.Empty);
                Preferences.Set(IS_LOGGED_IN_KEY, true);
                Preferences.Set(USER_EMAIL_KEY, userEmail ?? string.Empty);
                Preferences.Set(USER_ID_KEY, userId);
                Preferences.Set(USER_NAME_KEY, userName ?? string.Empty);

                System.Diagnostics.Debug.WriteLine($"📱 SessionId guardado: {sesion.SesionId}");
                System.Diagnostics.Debug.WriteLine($"🔑 Token guardado: {(!string.IsNullOrEmpty(sesion.Token) ? "SÍ" : "NO")}");
                System.Diagnostics.Debug.WriteLine($"📧 Email guardado: {userEmail}");

                // Guardar fechas de sesión
                if (!string.IsNullOrEmpty(sesion.FechaCreacion))
                {
                    if (DateTime.TryParse(sesion.FechaCreacion, out DateTime fechaCreacion))
                    {
                        Preferences.Set("SessionCreatedAt", fechaCreacion.ToBinary());
                        System.Diagnostics.Debug.WriteLine($"📅 Fecha creación guardada: {fechaCreacion:yyyy-MM-dd HH:mm:ss}");
                    }
                }

                // Serializar y guardar toda la sesión para respaldo
                var sessionJson = JsonSerializer.Serialize(sesion);
                Preferences.Set(SESSION_DATA_KEY, sessionJson);

                System.Diagnostics.Debug.WriteLine($"✅ Sesión guardada para: {userEmail}");
                System.Diagnostics.Debug.WriteLine("==========================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar sesión: {ex.Message}");
            }
        }

        public static void CerrarSesion()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚪 === CERRANDO SESIÓN ===");

                // Limpiar todas las preferencias de sesión
                Preferences.Remove(SESSION_ID_KEY);
                Preferences.Remove(TOKEN_KEY);
                Preferences.Remove(IS_LOGGED_IN_KEY);
                Preferences.Remove(USER_EMAIL_KEY);
                Preferences.Remove(USER_ID_KEY);
                Preferences.Remove(USER_NAME_KEY);
                Preferences.Remove(SESSION_DATA_KEY);
                Preferences.Remove("SessionCreatedAt");

                System.Diagnostics.Debug.WriteLine("✅ Sesión cerrada correctamente");
                System.Diagnostics.Debug.WriteLine("==========================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cerrar sesión: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de verificación de sesión

        public static bool EstaLogueado()
        {
            try
            {
                var isLoggedIn = Preferences.Get(IS_LOGGED_IN_KEY, false);
                var hasToken = !string.IsNullOrEmpty(ObtenerToken());
                var hasSessionId = !string.IsNullOrEmpty(ObtenerSessionId());

                System.Diagnostics.Debug.WriteLine($"🔍 Verificando si está logueado:");
                System.Diagnostics.Debug.WriteLine($"  - IsLoggedIn flag: {isLoggedIn}");
                System.Diagnostics.Debug.WriteLine($"  - Tiene token: {hasToken}");
                System.Diagnostics.Debug.WriteLine($"  - Tiene SessionId: {hasSessionId}");

                // ===== CAMBIO PRINCIPAL: SIN VERIFICACIÓN DE EXPIRACIÓN =====
                var resultado = isLoggedIn && hasToken && hasSessionId;
                System.Diagnostics.Debug.WriteLine($"  - RESULTADO: {resultado}");

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando sesión: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTODO SIMPLIFICADO: TokenExpirado() siempre retorna FALSE =====
        public static bool TokenExpirado()
        {
            // Si el backend nunca expira tokens, este método siempre retorna false
            System.Diagnostics.Debug.WriteLine("🔍 TokenExpirado() - Backend nunca expira tokens, retornando FALSE");
            return false;
        }

        public static bool SesionExpirada()
        {
            var logueado = EstaLogueado();
            System.Diagnostics.Debug.WriteLine($"🔍 SesionExpirada - Logueado: {logueado}");
            return !logueado;
        }

        #endregion

        #region Métodos de obtención de datos

        public static string ObtenerSessionId()
        {
            return Preferences.Get(SESSION_ID_KEY, string.Empty);
        }

        public static string ObtenerToken()
        {
            return Preferences.Get(TOKEN_KEY, string.Empty);
        }

        public static string ObtenerEmailUsuario()
        {
            return Preferences.Get(USER_EMAIL_KEY, string.Empty);
        }

        public static string ObtenerNombreUsuario()
        {
            return Preferences.Get(USER_NAME_KEY, string.Empty);
        }

        public static int ObtenerIdUsuario()
        {
            return Preferences.Get(USER_ID_KEY, 0);
        }

        public static Sesion ObtenerSesionCompleta()
        {
            try
            {
                var sessionJson = Preferences.Get(SESSION_DATA_KEY, string.Empty);
                if (!string.IsNullOrEmpty(sessionJson))
                {
                    return JsonSerializer.Deserialize<Sesion>(sessionJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sesión completa: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Métodos de actualización

        public static void ActualizarToken(string nuevoToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Actualizando token...");
                Preferences.Set(TOKEN_KEY, nuevoToken);
                System.Diagnostics.Debug.WriteLine("✅ Token actualizado correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error actualizando token: {ex.Message}");
            }
        }

        public static void ActualizarDatosUsuario(string nombre, int userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(nombre))
                {
                    Preferences.Set(USER_NAME_KEY, nombre);
                }

                if (userId > 0)
                {
                    Preferences.Set(USER_ID_KEY, userId);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Datos de usuario actualizados: {nombre}, ID: {userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error actualizando datos de usuario: {ex.Message}");
            }
        }

        #endregion

        #region Métodos auxiliares para JWT (para info únicamente)

        public static string ObtenerClaimDelToken(string claimType)
        {
            try
            {
                var token = ObtenerToken();
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                return jsonToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo claim {claimType}: {ex.Message}");
                return string.Empty;
            }
        }

        // Método para obtener información del token solo para debugging
        public static void ImprimirInformacionToken()
        {
            try
            {
                var token = ObtenerToken();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("❌ No hay token");
                    return;
                }

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                System.Diagnostics.Debug.WriteLine("=== INFORMACIÓN DEL TOKEN ===");
                System.Diagnostics.Debug.WriteLine($"📅 Emitido: {jsonToken.IssuedAt:yyyy-MM-dd HH:mm:ss} UTC");
                System.Diagnostics.Debug.WriteLine($"⏰ Fecha exp en JWT: {jsonToken.ValidTo:yyyy-MM-dd HH:mm:ss} UTC (IGNORADA)");
                System.Diagnostics.Debug.WriteLine($"🕐 Ahora: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                System.Diagnostics.Debug.WriteLine($"🔍 Token válido: SÍ (backend nunca expira)");
                System.Diagnostics.Debug.WriteLine("===============================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error imprimiendo info del token: {ex.Message}");
            }
        }

        // Método alias para compatibilidad
        public static void ImprimirInformacionTokenDetallada()
        {
            ImprimirInformacionToken();
        }

        #endregion

        #region Métodos de depuración y utilidad

        public static void ImprimirInformacionSesion()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INFORMACIÓN DE SESIÓN ===");
                System.Diagnostics.Debug.WriteLine($"Logueado: {EstaLogueado()}");
                System.Diagnostics.Debug.WriteLine($"Token expirado: {TokenExpirado()} (siempre FALSE)");
                System.Diagnostics.Debug.WriteLine($"Email: {ObtenerEmailUsuario()}");
                System.Diagnostics.Debug.WriteLine($"Nombre: {ObtenerNombreUsuario()}");
                System.Diagnostics.Debug.WriteLine($"Session ID: {ObtenerSessionId()}");
                System.Diagnostics.Debug.WriteLine($"User ID: {ObtenerIdUsuario()}");
                System.Diagnostics.Debug.WriteLine("===========================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error imprimiendo información de sesión: {ex.Message}");
            }
        }

        // Método simplificado ya que no hay expiración
        public static void LimpiarSesionExpirada()
        {
            System.Diagnostics.Debug.WriteLine("🔍 LimpiarSesionExpirada() - No aplica, tokens nunca expiran");
            // No hacer nada ya que los tokens nunca expiran
        }

        #endregion
    }
}