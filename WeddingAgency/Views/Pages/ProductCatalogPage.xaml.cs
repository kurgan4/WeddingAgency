using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class ProductCatalogPage : Page
    {
        private List<HolidayProducts> _allProducts = new List<HolidayProducts>();

        public ProductCatalogPage()
        {
            InitializeComponent();
            LoadProductTypes();
            LoadHolidayProducts();
        }

        private void LoadProductTypes()
        {
            try
            {
                var types = Core.Context.HolidayProductTypes.ToList();

                var allTypesItem = new HolidayProductTypes { ProductTypeID = 0, TypeName = "Все категории" };
                types.Insert(0, allTypesItem);

                ComboProductTypes.ItemsSource = types;
                ComboProductTypes.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить типы товаров: {ex.Message}");
            }
        }

        private void LoadHolidayProducts()
        {
            try
            {
                _allProducts = Core.Context.HolidayProducts.Include("HolidayProductTypes").Where(p => p.IsActive == true).ToList();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить товары: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            if (TxtSearch == null || ComboSort == null || ComboProductTypes == null || ListProducts == null)
                return;

            if (_allProducts == null) return;

            var filteredList = _allProducts.AsQueryable();

            var selectedType = ComboProductTypes.SelectedItem as HolidayProductTypes;
            if (selectedType != null && selectedType.ProductTypeID > 0)
            {
                filteredList = filteredList.Where(p => p.ProductTypeID == selectedType.ProductTypeID);
            }

            string search = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                filteredList = filteredList.Where(p => p.ProductName.ToLower().Contains(search));
            }

            if (ComboSort.SelectedIndex == 1)
            {
                filteredList = filteredList.OrderBy(p => p.Price);
            }
            else if (ComboSort.SelectedIndex == 2)
            {
                filteredList = filteredList.OrderByDescending(p => p.Price);
            }

            ListProducts.ItemsSource = filteredList.ToList();
        }

        private void FilterChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void FilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            ComboSort.SelectedIndex = 0;
            ComboProductTypes.SelectedIndex = 0;
            ApplyFilters();
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUserId == 0)
            {
                MyMessageBox.Show("Авторизация", "Для выбора товаров и оформления заказов необходимо зарегистрироваться или войти в личный кабинет!");
                return;
            }

            var button = sender as Button;
            var product = button?.DataContext as HolidayProducts;
            if (product == null) return;

            var listBoxItem = ListProducts.ItemContainerGenerator.ContainerFromItem(product) as ListBoxItem;
            if (listBoxItem == null) return;

            var txtQty = FindVisualChild<TextBox>(listBoxItem, "TxtQty");
            if (txtQty != null)
            {
                if (int.TryParse(txtQty.Text, out int qty) && qty > 0)
                {
                    CartService.Add(null, qty, null, null, null, product);
                    MyMessageBox.Show("Выбор", $"\"{product.ProductName}\" добавлен в заказ в количестве {qty} шт.");
                }
                else MyMessageBox.Show("Валидация", "Введите корректное количество!");
            }
        }

        private T FindVisualChild<T>(DependencyObject obj, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && (child as FrameworkElement).Name == name) return (T)child;
                T childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
    }
}
