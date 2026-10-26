using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class AdminActiveEventsPage : Page
    {
        private List<Events> _allActiveEvents = new List<Events>();

        public AdminActiveEventsPage()
        {
            InitializeComponent();
            LoadActiveEventsData();
        }

        private void LoadActiveEventsData()
        {
            try
            {
                // Загружаем мероприятия в производстве (все статусы, кроме Новых заявок [ID=1] и Отмененных [ID=5])
                _allActiveEvents = Core.Context.Events
                    .Include("EventTypes")
                    .Include("Users")
                    .Include("EventStatuses")
                    .Include("EventItems.CatalogItems")
                    .Include("EventItems.Partners")
                    .Include("EventItems.HolidayProducts")
                    .Where(e => e.StatusID != 1 && e.StatusID != 5)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить действующие заказы: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            if (TxtSearch == null || ComboDateSort == null || GridActiveEvents == null)
                return;

            if (_allActiveEvents == null) return;

            var filteredList = _allActiveEvents.AsQueryable();

            string search = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                filteredList = filteredList.Where(e =>
                    e.Title.ToLower().Contains(search) ||
                    e.Location.ToLower().Contains(search));
            }

            if (ComboDateSort.SelectedIndex == 0)
            {
                filteredList = filteredList.OrderBy(e => e.EventDate);
            }
            else if (ComboDateSort.SelectedIndex == 1)
            {
                filteredList = filteredList.OrderByDescending(e => e.EventDate);
            }

            GridActiveEvents.ItemsSource = filteredList.ToList();
        }

        private void FilterChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void FilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            ComboDateSort.SelectedIndex = 0;
            ApplyFilters();
        }
    }
}
