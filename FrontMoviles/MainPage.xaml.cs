namespace FrontMoviles
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void btnIrLogin_Clicked_1(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LoginPage());
        }

        private void btnIrRegistrar_Clicked_1(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RegisterPage());
        }

        private async void btnIrVerificar_Clicked(object sender, EventArgs e)
        {
            // Solicitar email para verificación
            string email = await DisplayPromptAsync(
                "Verificar Cuenta",
                "Ingresa tu email:",
                placeholder: "correo@ejemplo.com",
                keyboard: Keyboard.Email);

            if (!string.IsNullOrWhiteSpace(email))
            {
                await Navigation.PushAsync(new VerificationPage(email.Trim().ToLower()));
            }
        }
    }
}