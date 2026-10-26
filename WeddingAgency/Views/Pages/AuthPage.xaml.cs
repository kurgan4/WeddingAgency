using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                txtHintPass.Visibility = Visibility.Visible;
            }
            else
            {
                txtHintPass.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MyMessageBox.Show("Предупреждение", "Пожалуйста, заполните все поля!");
                return;
            }

            string hashedPassword = HashService.ComputeSha256Hash(password);

            try
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == hashedPassword);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        MyMessageBox.Show("Ошибка доступа", "Ваш аккаунт временно заблокирован администратором!");
                        return;
                    }

                    Core.CurrentUserId = user.UserID;
                    failedAttempts = 0;

                    try
                    {
                        // Применяем персональную тему оформления пользователя из базы данных
                        string themePath = user.IsDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
                        var uri = new Uri(themePath, UriKind.Relative);
                        ResourceDictionary resourceDict = new ResourceDictionary { Source = uri };

                        Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                        Application.Current.Resources.MergedDictionaries.Insert(0, resourceDict);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка инициализации темы оформления: {ex.Message}");
                    }

                    MyMessageBox.Show("Успешный вход", $"Добро пожаловать, {user.FirstName} {user.LastName}!");

                    NavigationService.Navigate(new MainShellPage());
                }
                else
                {
                    failedAttempts++;

                    if (failedAttempts >= 3)
                    {
                        MyMessageBox.Show("Защита от роботов", "Слишком много неудачных попыток! Вы перенаправлены на ввод капчи.");
                        NavigationService.Navigate(new CaptchaPage(this));
                    }
                    else
                    {
                        MyMessageBox.Show("Ошибка авторизации", $"Неверный логин или пароль! Осталось попыток: {3 - failedAttempts}");
                    }
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Критическая ошибка", $"Ошибка при подключении к базе данных: {ex.Message}");
            }
        }

        private void ButtonGuest_Click(object sender, RoutedEventArgs e)
        {
            Core.CurrentUserId = 0;
            MyMessageBox.Show("Вход выполнен", "Вы вошли как Гость. Вам доступен только просмотр каталога услуг и товаров.");

            NavigationService.Navigate(new MainShellPage());
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

        public void ResetAttempts()
        {
            failedAttempts = 0;
            TextBoxLogin.Clear();
            PasswordBox.Clear();
        }
    }
}
