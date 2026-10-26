using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class PartnerOffersPage : Page
    {
        private Partners _currentPartner;
        private ItemCategories _parentCategory;

        public PartnerOffersPage(Partners partner, ItemCategories parentCategory)
        {
            InitializeComponent();
            _currentPartner = partner;
            _parentCategory = parentCategory;
            TxtPartnerHeader.Text = _currentPartner.PartnerName;
            LoadOffersData();
        }

        private void LoadOffersData()
        {
            try
            {
                var offers = Core.Context.CatalogItems
                    .Where(i => i.PartnerID == _currentPartner.PartnerID && i.IsActive == true)
                    .ToList();
                ListOffers.ItemsSource = offers;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить прайс-лист: {ex.Message}");
            }
        }

        // ИСПРАВЛЕНО: Название метода теперь строго соответствует XAML-разметке Click="BtnAddToCart_Click"
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MyMessageBox.Show("Авторизация", "Для выбора позиций прайс-листа необходимо зарегистрироваться или войти в личный кабинет!");
                return;
            }

            var button = sender as Button;
            var item = button?.DataContext as CatalogItems;
            if (item == null) return;

            // Находим контейнер строки списка для извлечения количества из TextBox
            var listBoxItem = ListOffers.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
            if (listBoxItem == null) return;

            var txtQty = FindVisualChild<TextBox>(listBoxItem, "TxtQty");
            if (txtQty != null)
            {
                if (int.TryParse(txtQty.Text, out int qty) && qty > 0)
                {
                    // ИСПРАВЛЕНО: Добавляем позицию в корзину с полной b2b-валидацией сохранения структуры данных
                    CartService.Add(item, qty, null, null, null);
                    MyMessageBox.Show("Выбор", $"Позиция \"{item.ItemName}\" выбрана в количестве {qty} шт.");
                }
                else
                {
                    MyMessageBox.Show("Валидация", "Введите корректное количество!");
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SubCatalogPage(_parentCategory));
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
