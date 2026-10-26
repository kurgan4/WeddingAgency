using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class ProfilePage : Page
    {
        private Users _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadUserOrdersHistory();
        }

        private void LoadUserData()
        {
            if (Core.CurrentUserId == 0) return;

            try
            {
                _currentUser = Core.Context.Users.FirstOrDefault(u => u.UserID == Core.CurrentUserId);
                if (_currentUser != null)
                {
                    TxtLogin.Text = _currentUser.Login;
                    TxtLastName.Text = _currentUser.LastName;
                    TxtFirstName.Text = _currentUser.FirstName;
                    TxtEmail.Text = _currentUser.Email;
                    TxtPhone.Text = _currentUser.Phone;
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить данные аккаунта: {ex.Message}");
            }
        }

        private void LoadUserOrdersHistory()
        {
            if (Core.CurrentUserId == 0) return;

            try
            {
                var userEvents = Core.Context.Events
                    .Include("EventStatuses")
                    .Include("EventTypes")
                    .Where(e => e.ClientID == Core.CurrentUserId)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToList();

                ListUserOrders.ItemsSource = userEvents;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить список актуальных заказов: {ex.Message}");
            }
        }

        private void OrderRowPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as StackPanel;
            var currentEvent = panel?.DataContext as Events;
            if (currentEvent == null) return;

            var txtOrderDetails = panel.FindName("TxtOrderDetails") as TextBlock;
            var borderWeddingNotice = panel.FindName("BorderWeddingNotice") as Border;

            if (borderWeddingNotice != null)
            {
                if (currentEvent.EventTypeID == 1) borderWeddingNotice.Visibility = Visibility.Visible;
                else borderWeddingNotice.Visibility = Visibility.Collapsed;
            }

            if (txtOrderDetails != null)
            {
                try
                {
                    var items = Core.Context.EventItems
                        .Include("CatalogItems")
                        .Include("Partners")
                        .Include("HolidayProducts")
                        .Where(ei => ei.EventID == currentEvent.EventID)
                        .ToList();

                    if (items.Count > 0)
                    {
                        List<string> namesList = new List<string>();
                        foreach (var row in items)
                        {
                            string positionName = "";
                            if (row.HolidayProducts != null) positionName = row.HolidayProducts.ProductName;
                            else if (row.PartnerID.HasValue) positionName = row.Partners?.PartnerName;
                            else if (row.CatalogItems != null) positionName = row.CatalogItems.ItemName;

                            if (!string.IsNullOrEmpty(positionName))
                            {
                                namesList.Add($"{positionName} ({row.Quantity} шт.)");
                            }
                        }

                        txtOrderDetails.Text = "Состав: " + string.Join(", ", namesList);
                    }
                    else
                    {
                        txtOrderDetails.Text = "Состав: Позиции еще не добавлены менеджером";
                    }
                }
                catch
                {
                    txtOrderDetails.Text = "Состав: Не удалось загрузить список позиций";
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            string lastName = TxtLastName.Text.Trim();
            string firstName = TxtFirstName.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string phone = TxtPhone.Text.Trim();

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, заполните все текстовые поля личной информации!");
                return;
            }

            try
            {
                if (Core.Context.Users.Any(u => u.Email == email && u.UserID != _currentUser.UserID))
                {
                    MyMessageBox.Show("Дубликат данных", "Этот Email уже занят другим пользователем!");
                    return;
                }

                if (Core.Context.Users.Any(u => u.Phone == phone && u.UserID != _currentUser.UserID))
                {
                    MyMessageBox.Show("Дубликат данных", "Этот номер телефона уже зарегистрирован!");
                    return;
                }

                _currentUser.LastName = lastName;
                _currentUser.FirstName = firstName;
                _currentUser.Email = email;
                _currentUser.Phone = phone;

                Core.Context.SaveChanges();
                MyMessageBox.Show("Успех", "Личные данные профиля успешно обновлены!");
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось сохранить изменения профиля: {ex.Message}");
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            string oldPassword = PassOld.Password;
            string newPassword = PassNew.Password;
            string confirmPassword = PassConfirm.Password;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MyMessageBox.Show("Валидация", "Для смены пароля заполните все три соответствующих поля!");
                return;
            }

            string oldHashed = HashService.ComputeSha256Hash(oldPassword);
            if (_currentUser.PasswordHash != oldHashed)
            {
                MyMessageBox.Show("Ошибка", "Старый пароль введен неверно!");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MyMessageBox.Show("Ошибка", "Новые пароли не совпадают!");
                return;
            }

            try
            {
                _currentUser.PasswordHash = HashService.ComputeSha256Hash(newPassword);
                Core.Context.SaveChanges();

                MyMessageBox.Show("Успех", "Пароль учетной записи успешно изменен!");
                PassOld.Clear();
                PassNew.Clear();
                PassConfirm.Clear();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось обновить пароль: {ex.Message}");
            }
        }
        private void BtnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MyMessageBox.Show("Авторизация", "Гости не могут оставлять отзывы!");
                return;
            }

            string reviewText = TxtReviewText.Text.Trim();
            int rating = 5 - ComboRating.SelectedIndex; 

            if (string.IsNullOrEmpty(reviewText))
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, напишите текст отзыва!");
                return;
            }

            try
            {
                var newReview = new Reviews()
                {
                    ClientID = Core.CurrentUserId,
                    Rating = rating,
                    ReviewText = reviewText,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Reviews.Add(newReview);
                Core.Context.SaveChanges();

                MyMessageBox.Show("Успех", "Спасибо! Ваш отзыв успешно отправлен и опубликован на сайте.");
                TxtReviewText.Clear();
                ComboRating.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось сохранить отзыв: {ex.Message}");
            }
        }

    }
}
