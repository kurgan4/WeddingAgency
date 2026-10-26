using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class OtherEventsPage : Page
    {
        public OtherEventsPage()
        {
            InitializeComponent();
            LoadEventTypes();
            LoadReviews();
            DateEvent.DisplayDateStart = DateTime.Today;
        }

        private void LoadEventTypes()
        {
            try
            {
                ComboEventTypes.ItemsSource = Core.Context.EventTypes.Where(t => t.EventTypeID != 1).ToList();
                if (ComboEventTypes.Items.Count > 0) ComboEventTypes.SelectedIndex = 0;
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка", ex.Message); }
        }

        private void LoadReviews()
        {
            try
            {
                // Подгружаем отзывы вместе с именами авторов (Users)
                ListReviews.ItemsSource = Core.Context.Reviews.Include("Users").OrderByDescending(r => r.CreatedAt).ToList();
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить отзывы: {ex.Message}"); }
        }

        private void RatingStars_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as StackPanel;
            var review = panel?.DataContext as Reviews;
            if (review == null) return;

            var txtStars = panel.FindName("TxtStars") as TextBlock;
            if (txtStars != null)
            {
                // Формируем количество звезд по оценке из базы
                txtStars.Text = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var selectedType = ComboEventTypes.SelectedItem as EventTypes;
            string title = TxtEventTitle.Text.Trim();
            DateTime? eventDate = DateEvent.SelectedDate;
            string startStr = TxtStart.Text.Trim();
            string endStr = TxtEnd.Text.Trim();
            string location = TxtLocation.Text.Trim();

            if (selectedType == null || string.IsNullOrEmpty(title) || eventDate == null || string.IsNullOrEmpty(location))
            {
                MyMessageBox.Show("Валидация", "Заполните все основные поля заявки!");
                return;
            }

            if (!TimeSpan.TryParse(startStr, out TimeSpan start) || !TimeSpan.TryParse(endStr, out TimeSpan end) || start >= end)
            {
                MyMessageBox.Show("Валидация", "Введите корректное время праздника!");
                return;
            }

            try
            {
                var newEvent = new Events()
                {
                    ClientID = Core.CurrentUserId == 0 ? 1 : Core.CurrentUserId,
                    EventTypeID = selectedType.EventTypeID,
                    StatusID = 1,
                    Title = title,
                    EventDate = eventDate.Value,
                    Location = location,
                    Budget = 0,
                    CreatedAt = DateTime.Now
                };
                Core.Context.Events.Add(newEvent);
                Core.Context.SaveChanges();

                MyMessageBox.Show("Успех", "Ваша заявка отправлена!");
                NavigationService.Navigate(new CatalogPage());
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка", ex.Message); }
        }
    }
}
