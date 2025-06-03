using FrontMoviles.Servicios;
using FrontMoviles.Modelos;
using System.Globalization;

namespace FrontMoviles;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService;
    private Usuario _usuarioActual;

    public PerfilPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        CargarPerfilUsuario();
    }

    #region Carga de datos

    private async void CargarPerfilUsuario()
    {
        try
        {
            // Mostrar indicador de carga
            MostrarEstado("loading");

            // Obtener email del usuario logueado
            var userEmail = SessionManager.ObtenerEmailUsuario();

            if (string.IsNullOrEmpty(userEmail))
            {
                MostrarError("No se encontró información de sesión");
                return;
            }

            // Llamar a la API para obtener información del usuario
            var response = await _apiService.ObtenerUsuarioAsync(userEmail);

            if (response.Resultado && response.Usuario != null)
            {
                _usuarioActual = response.Usuario;
                CargarDatosEnUI(_usuarioActual);
                MostrarEstado("content");
            }
            else
            {
                var errorMessage = response.Error?.FirstOrDefault()?.Message ?? "Error desconocido";
                MostrarError($"Error al cargar perfil: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            MostrarError($"Error inesperado: {ex.Message}");
        }
    }

    private void CargarDatosEnUI(Usuario usuario)
    {
        try
        {
            // Nombre completo
            var nombreCompleto = $"{usuario.Nombre} {usuario.Apellido1}";
            if (!string.IsNullOrEmpty(usuario.Apellido2))
            {
                nombreCompleto += $" {usuario.Apellido2}";
            }
            NombreCompletoLabel.Text = nombreCompleto;

            // Email
            EmailLabel.Text = usuario.Correo;

            // Teléfono
            TelefonoLabel.Text = FormatearTelefono(usuario.Telefono);

            // Fecha de nacimiento
            if (DateTime.TryParse(usuario.FechaNacimiento.ToString(), out DateTime fechaNacimiento))
            {
                FechaNacimientoLabel.Text = fechaNacimiento.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
            }
            else
            {
                FechaNacimientoLabel.Text = "No especificado";
            }

            // Ubicación
            var ubicacion = "";
            if (usuario.Canton != null && usuario.Canton.Provincia != null)
            {
                ubicacion = $"{usuario.Canton.Nombre}, {usuario.Canton.Provincia.Nombre}";
            }
            else if (usuario.Provincia != null)
            {
                ubicacion = usuario.Provincia.Nombre;
            }
            else
            {
                ubicacion = "No especificado";
            }
            UbicacionLabel.Text = ubicacion;

            // Dirección exacta
            DireccionLabel.Text = string.IsNullOrEmpty(usuario.Direccion) ? "No especificado" : usuario.Direccion;
            DireccionLabel.IsVisible = !string.IsNullOrEmpty(usuario.Direccion);

            // Estado de verificación
            ActualizarEstadoVerificacion(usuario.Verificacion > 0);

            // Foto de perfil
            if (!string.IsNullOrEmpty(usuario.FotoPerfil))
            {
                // Aquí podrías cargar la imagen real si tienes URLs de imágenes
                FotoPerfilLabel.Text = "📷";
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al mostrar datos: {ex.Message}", "OK");
        }
    }

    private void ActualizarEstadoVerificacion(bool verificado)
    {
        if (verificado)
        {
            VerificacionFrame.BackgroundColor = Color.FromArgb("#28a745"); // Verde
            VerificacionLabel.Text = "✓ Cuenta verificada";
        }
        else
        {
            VerificacionFrame.BackgroundColor = Color.FromArgb("#ffc107"); // Amarillo
            VerificacionLabel.Text = "⚠ Cuenta no verificada";
        }
    }

    private string FormatearTelefono(string telefono)
    {
        if (string.IsNullOrEmpty(telefono))
            return "No especificado";

        // Remover espacios y caracteres especiales
        var numeroLimpio = telefono.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Si es un número de Costa Rica (8 dígitos que empiezan con 6, 7 u 8)
        if (numeroLimpio.Length == 8 && (numeroLimpio.StartsWith("6") || numeroLimpio.StartsWith("7") || numeroLimpio.StartsWith("8")))
        {
            return $"+506 {numeroLimpio.Substring(0, 4)}-{numeroLimpio.Substring(4)}";
        }

        // Si ya tiene código de país
        if (numeroLimpio.StartsWith("506") && numeroLimpio.Length == 11)
        {
            return $"+506 {numeroLimpio.Substring(3, 4)}-{numeroLimpio.Substring(7)}";
        }

        return telefono; // Devolver como está si no se puede formatear
    }

    private void MostrarEstado(string estado)
    {
        LoadingGrid.IsVisible = estado == "loading";
        ContentScrollView.IsVisible = estado == "content";
        ErrorGrid.IsVisible = estado == "error";
    }

    private void MostrarError(string mensaje)
    {
        ErrorMessageLabel.Text = mensaje;
        MostrarEstado("error");
    }

    #endregion

    #region Eventos de navegación

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnConfigClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Configuración", "Función en desarrollo", "OK");
    }

    private async void OnReintentarClicked(object sender, EventArgs e)
    {
        CargarPerfilUsuario();
    }

    #endregion

    #region Eventos de opciones

    private async void OnEditarPerfilClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Editar Perfil", "Función en desarrollo", "OK");
        // Aquí navegarías a una página de edición de perfil
        // await Navigation.PushAsync(new EditarPerfilPage(_usuarioActual));
    }

    private async void OnCambiarContrasenaClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Cambiar Contraseña", "Función en desarrollo", "OK");
        // Aquí navegarías a una página para cambiar contraseña
        // await Navigation.PushAsync(new CambiarContrasenaPage());
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmar = await DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "Cancelar");

            if (confirmar)
            {
                // Limpiar la sesión
                SessionManager.CerrarSesion();

                // Navegar a la página principal
                Application.Current.MainPage = new AppShell();

                // Mostrar mensaje de confirmación
                await DisplayAlert("Sesión Cerrada", "Has cerrado sesión exitosamente", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
        }
    }

    #endregion

    #region Cleanup

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _apiService?.Dispose();
    }

    #endregion
}