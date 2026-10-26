using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class RegisterPage : Page
    {
        private const string PhoneMask = "+7 (___) ___-__-__";

        public RegisterPage()
        {
            InitializeComponent();
        }

        private void TxtPhone_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtPhone.Text == PhoneMask)
            {
                TxtPhone.CaretIndex = 4;
            }
        }

        private void TxtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
            if (e.Handled) return;

            string currentText = TxtPhone.Text;
            int indexToReplace = currentText.IndexOf('_');

            if (indexToReplace != -1)
            {
                char[] textChars = currentText.ToCharArray();
                textChars[indexToReplace] = e.Text[0];
                TxtPhone.Text = new string(textChars);
                TxtPhone.CaretIndex = indexToReplace + 1;

                while (TxtPhone.CaretIndex < TxtPhone.Text.Length && TxtPhone.Text[TxtPhone.CaretIndex] != '_')
                {
                    if (!TxtPhone.Text.Substring(TxtPhone.CaretIndex).Contains('_'))
                        break;
                    TxtPhone.CaretIndex++;
                }
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void TxtPhone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                int caretIndex = TxtPhone.CaretIndex;
                string currentText = TxtPhone.Text;
                int indexToRemove = (e.Key == Key.Back) ? caretIndex - 1 : caretIndex;

                if (indexToRemove < 4 || indexToRemove >= currentText.Length)
                {
                    e.Handled = true;
                    return;
                }

                if (char.IsDigit(currentText[indexToRemove]))
                {
                    char[] textChars = currentText.ToCharArray();
                    textChars[indexToRemove] = '_';
                    TxtPhone.Text = new string(textChars);
                    TxtPhone.CaretIndex = (e.Key == Key.Back) ? indexToRemove : indexToRemove + 1;
                }
                else
                {
                    if (e.Key == Key.Back)
                    {
                        TxtPhone.CaretIndex = indexToRemove;
                    }
                    else
                    {
                        TxtPhone.CaretIndex = indexToRemove + 1;
                    }
                }
                e.Handled = true;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string lastName = TxtLastName.Text.Trim();
            string firstName = TxtFirstName.Text.Trim();
            string login = TxtLogin.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string phone = TxtPhone.Text;
            string password = PassBox.Password;
            string passwordConfirm = PassBoxConfirm.Password;

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email))
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, заполните все текстовые поля личной информации и данных входа!");
                return;
            }

            if (phone == PhoneMask || phone.Contains('_'))
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, введите номер телефона полностью!");
                return;
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, введите пароль и подтвердите его!");
                return;
            }

            if (password != passwordConfirm)
            {
                MyMessageBox.Show("Ошибка валидации", "Введенные пароли не совпадают!");
                return;
            }

            try
            {
                bool isLoginExists = Core.Context.Users.Any(u => u.Login == login);
                if (isLoginExists)
                {
                    MyMessageBox.Show("Дубликат данных", "Пользователь с таким логином уже зарегистрирован в системе! Придумайте другой.");
                    return;
                }

                bool isEmailExists = Core.Context.Users.Any(u => u.Email == email);
                if (isEmailExists)
                {
                    MyMessageBox.Show("Дубликат данных", "Пользователь с таким Email уже существует!");
                    return;
                }

                var newUser = new Users()
                {
                    RoleID = 2,
                    Login = login,
                    LastName = lastName,
                    FirstName = firstName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = HashService.ComputeSha256Hash(password),
                    IsActive = true,
                    IsDarkMode = false,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();

                MyMessageBox.Show("Успех", "Регистрация успешно завершена! Теперь вы можете войти.");
                NavigationService.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Произошла критическая ошибка при записи в базу данных: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }
    }
}
