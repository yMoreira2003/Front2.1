using FrontMoviles.Servicios;
using FrontMoviles.Modelos;
using System.Text.RegularExpressions;

namespace FrontMoviles;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<Provincia> _provincias = new List<Provincia>();
    private List<Canton> _cantones = new List<Canton>();
    private List<Canton> _cantonesFiltrados = new List<Canton>();

    public RegisterPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        ConfigurarFechas();
        CargarUbicaciones();
    }

    #region Configuración Inicial

    private void ConfigurarFechas()
    {
        // Configurar fechas para el DatePicker
        var hoy = DateTime.Now;
        FechaNacimientoPicker.MaximumDate = hoy.AddYears(-13); // Mínimo 13 años
        FechaNacimientoPicker.MinimumDate = hoy.AddYears(-120); // Máximo 120 años
        FechaNacimientoPicker.Date = hoy.AddYears(-25); // Valor por defecto: 25 años
    }

    private async void CargarUbicaciones()
    {
        try
        {
            // Mostrar indicador de carga (opcional)
            ProvinciaPicker.Title = "Cargando provincias...";
            ProvinciaPicker.IsEnabled = false;

            // Cargar provincias desde la API
            var responseProvincias = await _apiService.ObtenerProvinciasAsync();

            if (responseProvincias.Resultado && responseProvincias.Provincias?.Any() == true)
            {
                _provincias = responseProvincias.Provincias;
                CargarProvinciasPicker();
            }
            else
            {
                var errorMsg = responseProvincias.Error?.FirstOrDefault()?.Message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudieron cargar las provincias: {errorMsg}", "OK");
                // Cargar datos de respaldo
                CargarProvinciasRespaldo();
            }

            // Cargar cantones desde la API
            var responseCantones = await _apiService.ObtenerCantonesAsync();

            if (responseCantones.Resultado && responseCantones.Cantones?.Any() == true)
            {
                _cantones = responseCantones.Cantones;
            }
            else
            {
                var errorMsg = responseCantones.Error?.FirstOrDefault()?.Message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudieron cargar los cantones: {errorMsg}", "OK");
                // Cargar datos de respaldo
                CargarCantonesRespaldo();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar ubicaciones: {ex.Message}", "OK");
            // Cargar datos de respaldo en caso de error
            CargarProvinciasRespaldo();
            CargarCantonesRespaldo();
        }
    }

    private void CargarProvinciasPicker()
    {
        try
        {
            ProvinciaPicker.ItemsSource = null; // Limpiar primero
            ProvinciaPicker.ItemsSource = _provincias.Select(p => p.Nombre).ToList();
            ProvinciaPicker.Title = "Seleccionar provincia";
            ProvinciaPicker.IsEnabled = true;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al cargar provincias en picker: {ex.Message}", "OK");
        }
    }

    private void CargarProvinciasRespaldo()
    {
        // Datos de respaldo en caso de error con la API
        _provincias = new List<Provincia>
        {
            new Provincia { ProvinciaId = 1, Nombre = "San José" },
            new Provincia { ProvinciaId = 2, Nombre = "Alajuela" },
            new Provincia { ProvinciaId = 3, Nombre = "Cartago" },
            new Provincia { ProvinciaId = 4, Nombre = "Heredia" },
            new Provincia { ProvinciaId = 5, Nombre = "Guanacaste" },
            new Provincia { ProvinciaId = 6, Nombre = "Puntarenas" },
            new Provincia { ProvinciaId = 7, Nombre = "Limón" }
        };

        CargarProvinciasPicker();
    }

    private void CargarCantonesRespaldo()
    {
        // Datos de respaldo básicos - en una implementación real, tendrías más cantones
        _cantones = new List<Canton>
        {
            // San José
            new Canton { CantonId = 1, Nombre = "San José", Provincia = new Provincia { ProvinciaId = 1, Nombre = "San José" } },
            new Canton { CantonId = 2, Nombre = "Escazú", Provincia = new Provincia { ProvinciaId = 1, Nombre = "San José" } },
            new Canton { CantonId = 3, Nombre = "Desamparados", Provincia = new Provincia { ProvinciaId = 1, Nombre = "San José" } },
            
            // Alajuela
            new Canton { CantonId = 4, Nombre = "Alajuela", Provincia = new Provincia { ProvinciaId = 2, Nombre = "Alajuela" } },
            new Canton { CantonId = 5, Nombre = "San Ramón", Provincia = new Provincia { ProvinciaId = 2, Nombre = "Alajuela" } },
            
            // Agregar más cantones según necesidad...
        };
    }

    #endregion

    #region Eventos de Ubicación

    private void OnProvinciaSelectionChanged(object sender, EventArgs e)
    {
        try
        {
            if (ProvinciaPicker.SelectedIndex >= 0 && ProvinciaPicker.SelectedIndex < _provincias.Count)
            {
                var provinciaSeleccionada = _provincias[ProvinciaPicker.SelectedIndex];

                // Filtrar cantones por provincia seleccionada
                _cantonesFiltrados = _apiService.FiltrarCantonesPorProvincia(_cantones, provinciaSeleccionada.ProvinciaId);

                // Cargar cantones en el picker
                CantonPicker.ItemsSource = null;
                CantonPicker.ItemsSource = _cantonesFiltrados.Select(c => c.Nombre).ToList();
                CantonPicker.SelectedIndex = -1; // Resetear selección
                CantonPicker.IsEnabled = _cantonesFiltrados.Any();
                CantonPicker.Title = _cantonesFiltrados.Any() ? "Seleccionar cantón" : "No hay cantones disponibles";
            }
            else
            {
                // Resetear cantones si no hay provincia seleccionada
                CantonPicker.ItemsSource = null;
                CantonPicker.IsEnabled = false;
                CantonPicker.Title = "Seleccionar cantón";
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al cargar cantones: {ex.Message}", "OK");
        }
    }

    #endregion

    #region Eventos de Validación

    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidarFortalezaContrasena(e.NewTextValue);
    }

    private void ValidarFortalezaContrasena(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            ResetearBarrasFortaleza();
            PasswordStrengthLabel.Text = "La contraseña debe tener al menos 8 caracteres";
            PasswordStrengthLabel.TextColor = Colors.Gray;
            return;
        }

        int puntuacion = 0;
        var criterios = new List<string>();

        // Longitud mínima
        if (password.Length >= 8)
        {
            puntuacion++;
        }
        else
        {
            criterios.Add("al menos 8 caracteres");
        }

        // Contiene mayúsculas
        if (password.Any(char.IsUpper))
        {
            puntuacion++;
        }
        else
        {
            criterios.Add("una mayúscula");
        }

        // Contiene minúsculas
        if (password.Any(char.IsLower))
        {
            puntuacion++;
        }
        else
        {
            criterios.Add("una minúscula");
        }

        // Contiene números
        if (password.Any(char.IsDigit))
        {
            puntuacion++;
        }
        else
        {
            criterios.Add("un número");
        }

        ActualizarBarrasFortaleza(puntuacion);
        ActualizarMensajeFortaleza(puntuacion, criterios);
    }

    private void ResetearBarrasFortaleza()
    {
        StrengthBar1.Color = Color.FromArgb("#E0E0E0");
        StrengthBar2.Color = Color.FromArgb("#E0E0E0");
        StrengthBar3.Color = Color.FromArgb("#E0E0E0");
        StrengthBar4.Color = Color.FromArgb("#E0E0E0");
    }

    private void ActualizarBarrasFortaleza(int puntuacion)
    {
        ResetearBarrasFortaleza();

        var colores = new[]
        {
            "#FF4444", // Rojo - Muy débil
            "#FF8800", // Naranja - Débil
            "#FFBB00", // Amarillo - Moderada
            "#00AA00"  // Verde - Fuerte
        };

        for (int i = 0; i < puntuacion && i < 4; i++)
        {
            var color = Color.FromArgb(colores[Math.Min(puntuacion - 1, 3)]);

            switch (i)
            {
                case 0: StrengthBar1.Color = color; break;
                case 1: StrengthBar2.Color = color; break;
                case 2: StrengthBar3.Color = color; break;
                case 3: StrengthBar4.Color = color; break;
            }
        }
    }

    private void ActualizarMensajeFortaleza(int puntuacion, List<string> criterios)
    {
        switch (puntuacion)
        {
            case 0:
            case 1:
                PasswordStrengthLabel.Text = "Contraseña muy débil";
                PasswordStrengthLabel.TextColor = Color.FromArgb("#FF4444");
                break;
            case 2:
                PasswordStrengthLabel.Text = "Contraseña débil";
                PasswordStrengthLabel.TextColor = Color.FromArgb("#FF8800");
                break;
            case 3:
                PasswordStrengthLabel.Text = "Contraseña moderada";
                PasswordStrengthLabel.TextColor = Color.FromArgb("#FFBB00");
                break;
            case 4:
                PasswordStrengthLabel.Text = "Contraseña fuerte";
                PasswordStrengthLabel.TextColor = Color.FromArgb("#00AA00");
                break;
        }

        if (criterios.Any())
        {
            PasswordStrengthLabel.Text += $" - Falta: {string.Join(", ", criterios)}";
        }
    }

    #endregion

    #region Eventos de UI

    private void OnShowPasswordTapped(object sender, EventArgs e)
    {
        ContrasenaEntry.IsPassword = !ContrasenaEntry.IsPassword;
        var label = sender as Label;
        label.Text = ContrasenaEntry.IsPassword ? "Mostrar" : "Ocultar";
    }

    private async void OnTermsClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Términos y Condiciones", "Aquí irían los términos y condiciones completos.", "OK");
    }

    private async void OnPrivacyClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Política de Privacidad", "Aquí iría la política de privacidad completa.", "OK");
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    #endregion

    #region Validaciones

    private bool ValidarFormulario()
    {
        var errores = new List<string>();

        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
            errores.Add("El nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(Apellido1Entry.Text))
            errores.Add("El primer apellido es obligatorio");

        if (string.IsNullOrWhiteSpace(TelefonoEntry.Text))
            errores.Add("El teléfono es obligatorio");

        if (ProvinciaPicker.SelectedIndex < 0)
            errores.Add("Debe seleccionar una provincia");

        if (CantonPicker.SelectedIndex < 0)
            errores.Add("Debe seleccionar un cantón");

        // Validar email
        if (!ValidarEmail(CorreoEntry.Text))
        {
            errores.Add("El correo electrónico no es válido");
            CorreoErrorLabel.IsVisible = true;
        }
        else
        {
            CorreoErrorLabel.IsVisible = false;
        }

        // Validar contraseña
        if (string.IsNullOrWhiteSpace(ContrasenaEntry.Text) || ContrasenaEntry.Text.Length < 8)
            errores.Add("La contraseña debe tener al menos 8 caracteres");

        // Validar confirmación de contraseña
        if (ContrasenaEntry.Text != ConfirmarContrasenaEntry.Text)
        {
            errores.Add("Las contraseñas no coinciden");
            ConfirmarErrorLabel.IsVisible = true;
        }
        else
        {
            ConfirmarErrorLabel.IsVisible = false;
        }

        // Validar términos y condiciones
        if (!TerminosCheck.IsChecked)
            errores.Add("Debe aceptar los términos y condiciones");

        if (errores.Any())
        {
            DisplayAlert("Errores de validación", string.Join("\n", errores), "OK");
            return false;
        }

        return true;
    }

    private bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        catch
        {
            return false;
        }
    }

    private bool ValidarTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return false;

        // Remover espacios y caracteres especiales
        var numeroLimpio = telefono.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

        // Validar que tenga 8 dígitos (Costa Rica) o 11 con código de país
        return numeroLimpio.Length == 8 || (numeroLimpio.StartsWith("506") && numeroLimpio.Length == 11);
    }

    #endregion

    #region Registro

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidarFormulario())
            return;

        try
        {
            // Mostrar indicador de carga
            ((Button)sender).Text = "Registrando...";
            ((Button)sender).IsEnabled = false;

            // Obtener objetos seleccionados
            var provinciaSeleccionada = _provincias[ProvinciaPicker.SelectedIndex];
            var cantonSeleccionado = _cantonesFiltrados[CantonPicker.SelectedIndex];

            // Crear request
            var request = new ReqInsertarUsuario
            {
                Usuario = new Usuario
                {
                    Nombre = NombreEntry.Text.Trim(),
                    Apellido1 = Apellido1Entry.Text.Trim(),
                    Apellido2 = string.IsNullOrWhiteSpace(Apellido2Entry.Text) ? "" : Apellido2Entry.Text.Trim(),
                    FechaNacimiento = FechaNacimientoPicker.Date,
                    Telefono = TelefonoEntry.Text.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(DireccionEntry.Text) ? "" : DireccionEntry.Text.Trim(),
                    Correo = CorreoEntry.Text.Trim().ToLower(),
                    Contrasena = ContrasenaEntry.Text,
                    Provincia = provinciaSeleccionada,
                    Canton = cantonSeleccionado,
                    Activo = true,
                    PerfilCompleto = false,
                    Verificacion = 0
                }
            };

            // Llamar al API
            var response = await _apiService.RegistrarUsuarioAsync(request);

            if (response.Resultado)
            {
                await DisplayAlert("Éxito", "¡Cuenta creada exitosamente! Se ha enviado un código de verificación a tu correo.", "OK");

                // Navegar a la página de verificación
                await Navigation.PushAsync(new VerificationPage(CorreoEntry.Text.Trim().ToLower()));
            }
            else
            {
                var errorMessage = response.Error?.FirstOrDefault()?.Message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudo crear la cuenta: {errorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
        finally
        {
            // Restaurar botón
            ((Button)sender).Text = "Crear cuenta";
            ((Button)sender).IsEnabled = true;
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