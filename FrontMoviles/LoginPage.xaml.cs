using FrontMoviles.Modelos;
using FrontMoviles.Servicios;
using System.Text.RegularExpressions;

namespace FrontMoviles;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isPasswordVisible = false;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

        // Verificar si ya hay una sesión activa
        VerificarSesionExistente();
    }
    private async void TestLogsClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🔥🔥🔥 TEST LOG VISIBLE - ESTO DEBERÍA APARECER EN SALIDA 🔥🔥🔥");
        System.Console.WriteLine("📱📱📱 CONSOLE LOG TEST 📱📱📱");

        await DisplayAlert("Test", "Revisa la ventana de Salida AHORA", "OK");
    }
    #region Verificación de sesión existente

    private async void VerificarSesionExistente()
    {
        try
        {
            // Limpiar sesiones expiradas automáticamente
            SessionManager.LimpiarSesionExpirada();

            if (SessionManager.EstaLogueado())
            {
                // Si hay una sesión válida, navegar directamente al inicio
                System.Diagnostics.Debug.WriteLine("Sesión activa encontrada, navegando al inicio");
                await NavigateToHome();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error verificando sesión existente: {ex.Message}");
            // En caso de error, continuar con el flujo normal de login
        }
    }

    #endregion

    #region Validaciones

    private bool ValidarFormulario()
    {
        bool esValido = true;
        var errores = new List<string>();

        // Validar email (obligatorio y formato)
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            errores.Add("El correo electrónico es obligatorio");
            esValido = false;
        }
        else if (!ValidarEmail(EmailEntry.Text))
        {
            errores.Add("El formato del correo electrónico no es válido");
            esValido = false;
        }

        // Validar contraseña (obligatorio)
        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            errores.Add("La contraseña es obligatoria");
            esValido = false;
        }

        // Mostrar errores si los hay
        if (!esValido)
        {
            DisplayAlert("Datos incompletos", string.Join("\n", errores), "OK");
        }

        return esValido;
    }

    private bool ValidarEmail(string email)
    {
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    #endregion

    #region Eventos de UI

    private void OnShowPasswordTapped(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;

        if (sender is Label label)
        {
            label.Text = _isPasswordVisible ? "Ocultar" : "Mostrar";
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Recuperar Contraseña", "Funcionalidad de recuperación de contraseña próximamente.", "OK");
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    #endregion

    #region Login de Usuario

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (!ValidarFormulario())
            return;

        try
        {
            // Log del intento de login
            LogLoginAttempt(EmailEntry.Text?.Trim().ToLower());

            // Mostrar indicador de carga
            var button = sender as Button;
            var originalText = button.Text;
            button.Text = "Iniciando sesión...";
            button.IsEnabled = false;

            // Crear el objeto de request
            var request = new ReqLoginUsuario
            {
                Usuario = new UsuarioLogin
                {
                    Correo = EmailEntry.Text?.Trim().ToLower(),
                    Contrasena = PasswordEntry.Text
                }
            };

            // Llamar al API
            var response = await _apiService.LoginUsuarioAsync(request);

            // Procesar respuesta
            if (response.Resultado && response.Sesion != null)
            {
                // Login exitoso - Guardar sesión con información adicional
                await OnLoginExitoso(response.Sesion, EmailEntry.Text?.Trim().ToLower());
            }
            else
            {
                // Login fallido
                var errorMessage = response.Error?.FirstOrDefault()?.Message ?? "Credenciales incorrectas";
                await OnLoginFallido(errorMessage);
            }
        }
        catch (Exception ex)
        {
            await OnLoginFallido($"Error inesperado: {ex.Message}");
        }
        finally
        {
            // Restaurar botón
            if (sender is Button btn)
            {
                btn.Text = "Iniciar sesión";
                btn.IsEnabled = true;
            }
        }
    }

    #endregion

    #region Manejo de respuestas de login

    private async Task OnLoginExitoso(Sesion sesion, string userEmail)
    {
        try
        {
            // Extraer nombre de usuario del email para display
            var userName = userEmail.Split('@')[0];

            // Guardar sesión completa con información adicional
            SessionManager.GuardarSesion(sesion, userEmail, userName);

            // Imprimir información de sesión para debugging
            SessionManager.ImprimirInformacionSesion();

            // Mostrar mensaje de éxito
            await DisplayAlert("¡Bienvenido!", $"Sesión iniciada correctamente", "Continuar");

            // Navegar al inicio
            await NavigateToHome();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en login exitoso: {ex.Message}");
            await DisplayAlert("Error", "Error al procesar la sesión. Intenta nuevamente.", "OK");
        }
    }

    private async Task OnLoginFallido(string mensaje)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Login fallido: {mensaje}");

            // Limpiar campos sensibles
            PasswordEntry.Text = "";

            // Mostrar error específico o genérico
            string errorMessage = mensaje;
            if (mensaje.Contains("credenciales") || mensaje.Contains("usuario") || mensaje.Contains("contraseña"))
            {
                errorMessage = "Credenciales incorrectas. Verifica tu correo y contraseña.";
            }
            else if (mensaje.Contains("verificar") || mensaje.Contains("activar"))
            {
                errorMessage = "Tu cuenta no está verificada. Revisa tu correo electrónico.";
            }
            else if (mensaje.Contains("conexión") || mensaje.Contains("red"))
            {
                errorMessage = "Error de conexión. Verifica tu conexión a internet.";
            }

            await DisplayAlert("Error de inicio de sesión", errorMessage, "OK");

            // Enfocar en el campo de email para facilitar reintento
            EmailEntry.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error manejando login fallido: {ex.Message}");
            await DisplayAlert("Error", "Ocurrió un error inesperado", "OK");
        }
    }

    #endregion

    #region Navegación

    private async Task NavigateToHome()
    {
        try
        {
            // Navegación a la página de inicio
            await Navigation.PushAsync(new InicioPage());

            // Opcional: Remover esta página del stack para evitar volver al login con "Atrás"
            // Navigation.RemovePage(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navegando al inicio: {ex.Message}");
            await DisplayAlert("Error", "Error al navegar al inicio", "OK");
        }
    }

    #endregion

    #region Lifecycle y Cleanup

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Limpiar campos al aparecer la página (excepto si viene de registro)
        if (string.IsNullOrEmpty(EmailEntry.Text))
        {
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
            PasswordEntry.IsPassword = true;
            _isPasswordVisible = false;
        }

        // Enfocar en el campo de email
        EmailEntry.Focus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpiar información sensible
        PasswordEntry.Text = "";

        // Dispose del ApiService
        _apiService?.Dispose();
    }

    #endregion

    #region Debugging y utilidades

    private void LogLoginAttempt(string email)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== INTENTO DE LOGIN ===");
            System.Diagnostics.Debug.WriteLine($"Email: {email}");
            System.Diagnostics.Debug.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"Sesión activa previa: {SessionManager.EstaLogueado()}");
            System.Diagnostics.Debug.WriteLine("=======================");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en log: {ex.Message}");
        }
    }

    #endregion
}