using System;
using System.Linq;
using System.Windows.Controls;

namespace WeddingAgency.Views.Pages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            try
            {
                GridUsers.ItemsSource = Core.Context.Users
                    .Include("Roles")
                    .OrderBy(u => u.RoleID)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки списка пользователей: {ex.Message}");
            }
        }
    }
}
