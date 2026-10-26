using System;
using System.Linq;
using System.Windows.Controls;

namespace WeddingAgency.Views.Pages
{
    public partial class EventsPage : Page
    {
        public EventsPage()
        {
            InitializeComponent();
            LoadAllEvents();
        }

        private void LoadAllEvents()
        {
            try
            {
                GridAllEvents.ItemsSource = Core.Context.Events
                    .Include("Users")
                    .Include("EventStatuses")
                    .OrderByDescending(e => e.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки списка заказов: {ex.Message}");
            }
        }
    }
}
