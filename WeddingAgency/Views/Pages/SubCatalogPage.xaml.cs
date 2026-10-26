using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class SubCatalogPage : Page
    {
        private ItemCategories _currentCategory;

        public SubCatalogPage(ItemCategories category)
        {
            InitializeComponent();
            _currentCategory = category;
            TxtCategoryHeader.Text = _currentCategory.CategoryName;
            LoadPartnersData();
        }

        private void LoadPartnersData()
        {
            try
            {
                var partners = Core.Context.Partners
                    .Where(p => p.IsActive == true && p.CategoryID == _currentCategory.CategoryID)
                    .ToList();
                ListPartners.ItemsSource = partners;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить исполнителей: {ex.Message}");
            }
        }

        private void BtnViewOffers_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var partner = button?.DataContext as Partners;
            if (partner != null)
            {
                NavigationService.Navigate(new PartnerOffersPage(partner, _currentCategory));
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MyMessageBox.Show("Авторизация", "Для бронирования специалистов и услуг необходимо зарегистрироваться или войти в личный кабинет!");
                return;
            }

            var button = sender as Button;
            var partner = button?.DataContext as Partners;
            if (partner == null) return;

            var listBoxItem = ListPartners.ItemContainerGenerator.ContainerFromItem(partner) as ListBoxItem;
            if (listBoxItem == null) return;

            var txtStart = FindVisualChild<TextBox>(listBoxItem, "TxtStart");
            var txtEnd = FindVisualChild<TextBox>(listBoxItem, "TxtEnd");

            if (txtStart != null && txtEnd != null)
            {
                if (TimeSpan.TryParse(txtStart.Text, out TimeSpan start) && TimeSpan.TryParse(txtEnd.Text, out TimeSpan end))
                {
                    if (start >= end)
                    {
                        MyMessageBox.Show("Валидация", "Время начала не может быть больше или равно времени окончания!");
                        return;
                    }

                    CartService.Add(null, 1, start, end, partner);
                    MyMessageBox.Show("Корзина", $"Вы успешно выбрали специалиста: {partner.PartnerName}");
                }
                else
                {
                    MyMessageBox.Show("Валидация", "Введите время в формате ЧЧ:ММ!");
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage());
        }

        private T FindVisualChild<T>(DependencyObject obj, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && (child as FrameworkElement).Name == name)
                {
                    return (T)child;
                }
                T childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
    }
}
