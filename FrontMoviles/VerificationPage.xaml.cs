using FrontMoviles.Modelos;
using FrontMoviles.Servicios;
using System.Timers;

namespace FrontMoviles;

public partial class VerificationPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly string _userEmail;
    private System.Timers.Timer _resendTimer;
    private int _secondsRemaining = 60;

    public VerificationPage(string userEmail)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _userEmail = userEmail;

        SetupUI();
        StartResendTimer();
    }

    #region Configuración Inicial

    private void SetupUI()
    {
        UserEmailLabel.Text = _userEmail;
        VerificationCodeEntry.Focus();
    }

    private void StartResendTimer()
    {
        ResendLabel.IsVisible = false;
        TimerLabel.IsVisible = true;

        _resendTimer = new System.Timers.Timer(1000); // 1 segundo
        _resendTimer.Elapsed += OnTimerElapsed;
        _resendTimer.Start();
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _secondsRemaining--;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_secondsRemaining <= 0)
            {
                _resendTimer?.Stop();
                TimerLabel.IsVisible = false;
                ResendLabel.IsVisible = true;
                _secondsRemaining = 60;
            }
            else
            {
                TimerLabel.Text = $"Puedes reenviar en {_secondsRemaining} segundos";
            }
        });
    }

    #endregion

    #region Manejo de Código

    private void OnCodeTextChanged(object sender, TextChangedEventArgs e)
    {
        // Solo permitir números
        if (!string.IsNullOrEmpty(e.NewTextValue))
        {
            var validCode = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
            if (validCode != e.NewTextValue)
            {
                VerificationCodeEntry.Text = validCode;
                return;
            }
        }

        // Verificar si se completó el código (6 dígitos)
        CheckCodeCompletion();

        // Ocultar mensaje de error si está visible
        ErrorLabel.IsVisible = false;
    }

    private void CheckCodeCompletion()
    {
        bool isComplete = !string.IsNullOrEmpty(VerificationCodeEntry.Text) && VerificationCodeEntry.Text.Length == 6;
        VerifyButton.IsEnabled = isComplete;
    }

    private void ClearCode()
    {
        VerificationCodeEntry.Text = "";
        VerificationCodeEntry.Focus();
        VerifyButton.IsEnabled = false;
    }

    #endregion

    #region Eventos de UI

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        if (!VerifyButton.IsEnabled)
            return;

        try
        {
            // Mostrar indicador de carga
            VerifyButton.Text = "Verificando...";
            VerifyButton.IsEnabled = false;

            // Obtener el código
            var codeString = VerificationCodeEntry.Text;
            if (!int.TryParse(codeString, out int verificationCode))
            {
                ShowError("Código inválido");
                return;
            }

            // Crear el request
            var request = new ReqVerificacion
            {
                Correo = _userEmail,
                Verificacion = verificationCode
            };

            // Llamar al API
            var response = await _apiService.VerificarUsuarioAsync(request);

            // Procesar respuesta
            if (response.Resultado)
            {
                await DisplayAlert("Éxito", "Cuenta verificada exitosamente", "OK");

                // Navegar al login
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                // Mostrar errores del servidor
                var mensajesError = response.Error?.Select(e => e.Message) ?? new[] { "Código de verificación incorrecto" };
                var mensaje = string.Join("\n", mensajesError);
                ShowError(mensaje);

                // Limpiar el código para que el usuario vuelva a intentar
                ClearCode();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error inesperado: {ex.Message}");
            ClearCode();
        }
        finally
        {
            // Restaurar botón
            VerifyButton.Text = "Verificar";
            CheckCodeCompletion();
        }
    }

    private async void OnResendCodeTapped(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Código reenviado", $"Se ha enviado un nuevo código de verificación a {_userEmail}", "OK");

            // Reiniciar el timer
            StartResendTimer();

            // Limpiar el código actual
            ClearCode();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al reenviar código: {ex.Message}", "OK");
        }
    }

    private async void OnChangeEmailTapped(object sender, EventArgs e)
    {
        // Regresar al registro para cambiar el email
        await Navigation.PopAsync();
    }

    #endregion

    #region Helpers

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    #endregion

    #region Cleanup

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _resendTimer?.Stop();
        _resendTimer?.Dispose();
        _apiService?.Dispose();
    }

    #endregion
}