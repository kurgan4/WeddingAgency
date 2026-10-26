using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeddingAgency.Services;
using WeddingAgency.Views.Windows;

namespace WeddingAgency.Views.Pages
{
    public partial class AdminCatalogPage : Page
    {
        public AdminCatalogPage()
        {
            InitializeComponent();
            RefreshDataGrids();
        }

        private void RefreshDataGrids()
        {
            try
            {
                // 1. Загружаем предложения прайс-листов
                GridServices.ItemsSource = Core.Context.CatalogItems
                    .Include("Partners")
                    .OrderBy(c => c.Partners.PartnerName)
                    .ToList();

                // 2. ИСПРАВЛЕНО: Ошибка pi устранена, сортировка идет по hp.ProductName
                GridProducts.ItemsSource = Core.Context.HolidayProducts
                    .Include("HolidayProductTypes")
                    .OrderBy(hp => hp.ProductName)
                    .ToList();

                // 3. ДОБАВЛЕНО: Загружаем базу партнеров и подрядчиков агентства
                GridPartners.ItemsSource = Core.Context.Partners
                    .Include("ItemCategories")
                    .OrderBy(p => p.ItemCategories.CategoryName)
                    .ThenBy(p => p.PartnerName)
                    .ToList();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось обновить списки: {ex.Message}");
            }
        }

        private void BtnSaveService_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CatalogItems;
            if (item == null) return;
            try
            {
                Core.Context.SaveChanges();
                MyMessageBox.Show("Каталог", $"Изменения для услуги \"{item.ItemName}\" сохранены.");
                RefreshDataGrids();
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка", ex.Message); }
        }

        private void BtnSaveProduct_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as HolidayProducts;
            if (item == null) return;
            try
            {
                Core.Context.SaveChanges();
                MyMessageBox.Show("Каталог", $"Изменения для товара \"{item.ProductName}\" сохранены.");
                RefreshDataGrids();
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка", ex.Message); }
        }

        private void BtnSavePartner_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as Partners;
            if (item == null) return;
            try
            {
                Core.Context.SaveChanges();
                MyMessageBox.Show("Каталог", $"Данные партнера \"{item.PartnerName}\" успешно обновлены.");
                RefreshDataGrids();
            }
            catch (Exception ex) { MyMessageBox.Show("Ошибка", ex.Message); }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно в режиме соответствующей выбранной вкладки TabControl
            int mode = TabCatalogContainer.SelectedIndex;
            CatalogEditWindow editWindow = new CatalogEditWindow(mode, null) { Owner = Window.GetWindow(this) };

            if (editWindow.ShowDialog() == true)
            {
                RefreshDataGrids(); // Мгновенно обновляем таблицы при успешном добавлении
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            int mode = TabCatalogContainer.SelectedIndex;
            object selectedItem = null;

            if (mode == 0) selectedItem = GridServices.SelectedItem;
            else if (mode == 1) selectedItem = GridProducts.SelectedItem;
            else if (mode == 2) selectedItem = GridPartners.SelectedItem;

            if (selectedItem == null)
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, выберите строку в таблице для редактирования!");
                return;
            }

            CatalogEditWindow editWindow = new CatalogEditWindow(mode, selectedItem) { Owner = Window.GetWindow(this) };
            if (editWindow.ShowDialog() == true)
            {
                RefreshDataGrids(); // Мгновенно обновляем таблицы при изменении данных
            }
        }

    }
}
