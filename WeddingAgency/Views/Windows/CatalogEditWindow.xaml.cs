using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WeddingAgency.Views.Windows
{
    public partial class CatalogEditWindow : Window
    {
        private int _mode; // 0 - Услуга, 1 - Товар, 2 - Партнер
        private object _currentItem;
        private string _newAbsoluteFilePath = null;

        public CatalogEditWindow(int mode, object item = null)
        {
            InitializeComponent();
            _mode = mode;
            _currentItem = item;
            LoadComboBoxes();
            InitializeForm();
        }

        private void LoadComboBoxes()
        {
            try
            {
                ComboPartners.ItemsSource = Core.Context.Partners.Where(p => p.IsActive).ToList();
                ComboProductTypes.ItemsSource = Core.Context.HolidayProductTypes.ToList();
                ComboCategories.ItemsSource = Core.Context.ItemCategories.ToList();
            }
            catch { }
        }

        private void InitializeForm()
        {
            if (_currentItem == null) TxtHeader.Text = "Добавление позиции";
            else TxtHeader.Text = "Редактирование параметров";

            if (_mode == 0)
            {
                PanelService.Visibility = Visibility.Visible;
                if (_currentItem is CatalogItems service)
                {
                    ComboPartners.SelectedValue = service.PartnerID;
                    TxtServiceName.Text = service.ItemName;
                    TxtServiceDesc.Text = service.Description;
                    TxtServicePrice.Text = service.BasePrice.ToString();
                    TxtServiceImgPath.Text = service.ImagePath;
                    SetPreviewImage(service.ImagePath, ImgPreviewService);
                }
            }
            else if (_mode == 1)
            {
                PanelProduct.Visibility = Visibility.Visible;
                if (_currentItem is HolidayProducts product)
                {
                    ComboProductTypes.SelectedValue = product.ProductTypeID;
                    TxtProductName.Text = product.ProductName;
                    TxtProductDesc.Text = product.Description;
                    TxtProductPrice.Text = product.Price.ToString();
                    TxtProductImgPath.Text = product.ImagePath;
                    SetPreviewImage(product.ImagePath, ImgPreviewProduct);
                }
            }
            else if (_mode == 2)
            {
                PanelPartner.Visibility = Visibility.Visible;
                if (_currentItem is Partners partner)
                {
                    ComboCategories.SelectedValue = partner.CategoryID;
                    TxtPartnerName.Text = partner.PartnerName;
                    TxtPartnerNotes.Text = partner.Notes;
                    CheckIsCompany.IsChecked = partner.IsCompany;
                    TxtPartnerPrice.Text = partner.PersonalPrice.ToString();
                    TxtPartnerImgPath.Text = partner.ImagePath;
                    SetPreviewImage(partner.ImagePath, ImgPreviewPartner);
                }
            }
        }

        private void SetPreviewImage(string fullPath, Image imgControl)
        {
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath)) { imgControl.Source = null; return; }
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgControl.Source = bitmap;
            }
            catch { imgControl.Source = null; }
        }

        private void BtnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Графические файлы (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                // Запоминаем ПОЛНЫЙ абсолютный путь к файлу на компьютере
                _newAbsoluteFilePath = openFileDialog.FileName;

                if (_mode == 0) { TxtServiceImgPath.Text = _newAbsoluteFilePath; SetPreviewImage(_newAbsoluteFilePath, ImgPreviewService); }
                else if (_mode == 1) { TxtProductImgPath.Text = _newAbsoluteFilePath; SetPreviewImage(_newAbsoluteFilePath, ImgPreviewProduct); }
                else if (_mode == 2) { TxtPartnerImgPath.Text = _newAbsoluteFilePath; SetPreviewImage(_newAbsoluteFilePath, ImgPreviewPartner); }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_mode == 0)
                {
                    var partner = ComboPartners.SelectedItem as Partners;
                    if (partner == null || string.IsNullOrEmpty(TxtServiceName.Text) || !decimal.TryParse(TxtServicePrice.Text, out decimal price)) return;

                    var service = (_currentItem as CatalogItems) ?? new CatalogItems { IsActive = true };
                    service.PartnerID = partner.PartnerID;
                    service.ItemName = TxtServiceName.Text.Trim();
                    service.Description = TxtServiceDesc.Text.Trim();
                    service.BasePrice = price;

                    // Сохраняем полный путь
                    if (_newAbsoluteFilePath != null) service.ImagePath = _newAbsoluteFilePath;

                    if (_currentItem == null) Core.Context.CatalogItems.Add(service);
                }
                else if (_mode == 1)
                {
                    var type = ComboProductTypes.SelectedItem as HolidayProductTypes;
                    if (type == null || string.IsNullOrEmpty(TxtProductName.Text) || !decimal.TryParse(TxtProductPrice.Text, out decimal price)) return;

                    var product = (_currentItem as HolidayProducts) ?? new HolidayProducts { IsActive = true };
                    product.ProductTypeID = type.ProductTypeID;
                    product.ProductName = TxtProductName.Text.Trim();
                    product.Description = TxtProductDesc.Text.Trim();
                    product.Price = price;

                    if (_newAbsoluteFilePath != null) product.ImagePath = _newAbsoluteFilePath;

                    if (_currentItem == null) Core.Context.HolidayProducts.Add(product);
                }
                else if (_mode == 2)
                {
                    var cat = ComboCategories.SelectedItem as ItemCategories;
                    if (cat == null || string.IsNullOrEmpty(TxtPartnerName.Text) || !decimal.TryParse(TxtPartnerPrice.Text, out decimal price)) return;

                    var partner = (_currentItem as Partners) ?? new Partners { IsActive = true };
                    partner.CategoryID = cat.CategoryID;
                    partner.PartnerName = TxtPartnerName.Text.Trim();
                    partner.Notes = TxtPartnerNotes.Text.Trim();
                    partner.IsCompany = CheckIsCompany.IsChecked ?? false;
                    partner.PersonalPrice = price;

                    if (_newAbsoluteFilePath != null) partner.ImagePath = _newAbsoluteFilePath;

                    if (_currentItem == null) Core.Context.Partners.Add(partner);
                }

                Core.Context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
