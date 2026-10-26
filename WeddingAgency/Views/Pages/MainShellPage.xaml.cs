using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class MainShellPage : Page
    {
        private bool _isCurrentThemeDark = false;

        public MainShellPage()
        {
            InitializeComponent();
            ConfigureMenuByRole();
        }

        private void ConfigureMenuByRole()
        {
            if (Core.CurrentUserId == 0)
            {
                TxtRoleTitle.Text = "Режим: Гость";
                TxtUserFullName.Text = "Ограниченный доступ";

                BtnStats.Visibility = Visibility.Collapsed;
                BtnManageCatalog.Visibility = Visibility.Collapsed;
                BtnActiveEvents.Visibility = Visibility.Collapsed;
                BtnNewRequests.Visibility = Visibility.Collapsed;
                BtnToggleTheme.Visibility = Visibility.Collapsed;

                BtnWedding.Visibility = Visibility.Visible;
                BtnCatalog.Visibility = Visibility.Visible;
                BtnOtherEvents.Visibility = Visibility.Collapsed;
                BtnProfile.Visibility = Visibility.Collapsed;
                BtnCart.Visibility = Visibility.Collapsed;

                BtnExit.Content = "Авторизоваться";
                ContentFrame.Navigate(new CatalogPage());
                return;
            }

            try
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserID == Core.CurrentUserId);
                if (user != null)
                {
                    TxtUserFullName.Text = $"{user.FirstName} {user.LastName}";
                    _isCurrentThemeDark = user.IsDarkMode;
                    BtnToggleTheme.Visibility = Visibility.Visible;

                    if (user.RoleID == 1)
                    {
                        TxtRoleTitle.Text = "Администратор";

                        BtnWedding.Visibility = Visibility.Visible;
                        BtnCatalog.Visibility = Visibility.Visible;
                        BtnOtherEvents.Visibility = Visibility.Visible;
                        BtnProfile.Visibility = Visibility.Visible;
                        BtnCart.Visibility = Visibility.Visible;

                        AdminSeparator.Visibility = Visibility.Visible;
                        BtnStats.Visibility = Visibility.Visible;
                        BtnManageCatalog.Visibility = Visibility.Visible;
                        BtnActiveEvents.Visibility = Visibility.Visible;
                        BtnNewRequests.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TxtRoleTitle.Text = "Клиент";

                        AdminSeparator.Visibility = Visibility.Collapsed;
                        BtnStats.Visibility = Visibility.Collapsed;
                        BtnManageCatalog.Visibility = Visibility.Collapsed;
                        BtnActiveEvents.Visibility = Visibility.Collapsed;
                        BtnNewRequests.Visibility = Visibility.Collapsed;

                        BtnWedding.Visibility = Visibility.Visible;
                        BtnCatalog.Visibility = Visibility.Visible;
                        BtnOtherEvents.Visibility = Visibility.Visible;
                        BtnProfile.Visibility = Visibility.Visible;
                        BtnCart.Visibility = Visibility.Visible;
                    }

                    ContentFrame.Navigate(new CatalogPage());
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка", $"Ошибка загрузки профиля: {ex.Message}");
            }
        }


        private void BtnEvents_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new EventsPage());
        }

        private void BtnWedding_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CatalogPage());
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProductCatalogPage());
        }

        private void BtnOtherEvents_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new OtherEventsPage());
        }

        private void BtnTransactions_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new TransactionsPage());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new UsersPage());
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProfilePage());
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CheckoutPage());
        }

        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminStatsPage());
        }

        private void BtnManageCatalog_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminCatalogPage());
        }

        private void BtnActiveEvents_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminActiveEventsPage());
        }

        private void BtnNewRequests_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminRequestsPage());
        }

        private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0) return;

            try
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserID == Core.CurrentUserId);
                if (user != null)
                {
                    _isCurrentThemeDark = !_isCurrentThemeDark;
                    user.IsDarkMode = _isCurrentThemeDark;
                    Core.Context.SaveChanges();

                    string themePath = _isCurrentThemeDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
                    var uri = new Uri(themePath, UriKind.Relative);
                    ResourceDictionary resourceDict = new ResourceDictionary { Source = uri };
                    Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                    Application.Current.Resources.MergedDictionaries.Insert(0, resourceDict);
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка", $"Не удалось сохранить настройки темы: {ex.Message}");
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                if (MyMessageBox.Show("Авторизация", "Перейти на страницу авторизации?", true) == true)
                {
                    NavigationService.Navigate(new AuthPage());
                }
                return;
            }

            if (MyMessageBox.Show("Выход", "Вы действительно хотите выйти из учетной записи?", true) == true)
            {
                Core.CurrentUserId = 0;
                NavigationService.Navigate(new AuthPage());
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            while (ContentFrame.CanGoBack)
            {
                ContentFrame.RemoveBackEntry();
            }
        }
    }
}
