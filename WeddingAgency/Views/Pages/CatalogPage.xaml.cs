using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
            LoadCategoriesData();
        }

        private void LoadCategoriesData()
        {
            try
            {
                // Загружаем категории из базы данных для плиточного меню
                ListCategories.ItemsSource = Core.Context.ItemCategories.ToList();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить каталог категорий: {ex.Message}");
            }
        }

        private void ListCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCategory = ListCategories.SelectedItem as ItemCategories;
            if (selectedCategory != null)
            {
                // Сбрасываем выделение строки для предотвращения застревания триггеров
                ListCategories.SelectedItem = null;

                // Перенаправляем пользователя на страницу исполнителей выбранного направления
                NavigationService.Navigate(new SubCatalogPage(selectedCategory));
            }
        }
    }
}
