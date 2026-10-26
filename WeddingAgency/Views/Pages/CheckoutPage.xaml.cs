using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class CheckoutPage : Page
    {
        private bool _isOnlyProducts = false;
        private bool _isVenueSelectedInCart = false;
        private string _cartVenueName = "";

        public CheckoutPage()
        {
            InitializeComponent();
            LoadCartData();
            DateEvent.DisplayDateStart = DateTime.Today;
            EvaluateCartType();
        }

        private void EvaluateCartType()
        {
            // 1. Проверяем, оформляется ли только покупка розничных товаров (шарики/свечи)
            _isOnlyProducts = CartService.Items.All(i => i.HolidayProduct != null);

            // 2. Сканируем корзину на наличие выбранной площадки (CategoryID == 1)
            var venueItem = CartService.Items.FirstOrDefault(i => i.ProductOrService != null &&
                Core.Context.CatalogItems.Any(ci => ci.ItemID == i.ProductOrService.ItemID && ci.Partners.CategoryID == 1));

            if (venueItem != null)
            {
                _isVenueSelectedInCart = true;
                _cartVenueName = venueItem.ProductOrService.ItemName;
            }
            else
            {
                _isVenueSelectedInCart = false;
                _cartVenueName = "";
            }

            // 3. Настраиваем видимость элементов на основе анализа состава корзины
            if (_isOnlyProducts)
            {
                TxtEventTitle.Text = "Покупка праздничных товаров";
                PanelWeddingDetails.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtEventTitle.Text = "Наша идеальная свадьба";
                PanelWeddingDetails.Visibility = Visibility.Visible;

                // ИСПРАВЛЕНО: Если площадка УЖЕ выбрана в каталоге, скрываем текстовое поле ввода
                if (_isVenueSelectedInCart)
                {
                    PanelCustomLocation.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PanelCustomLocation.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadCartData()
        {
            ListCartItems.ItemsSource = null;
            ListCartItems.ItemsSource = CartService.Items;
            TxtTotalSum.Text = $"{CartService.GetTotalSum():N2} руб.";

            try
            {
                ComboPaymentMethods.ItemsSource = Core.Context.PaymentMethods.ToList();
                if (ComboPaymentMethods.Items.Count > 0) ComboPaymentMethods.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить способы оплаты: {ex.Message}");
            }
        }

        private void CartItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            var item = border?.DataContext as CartItem;
            if (item == null) return;

            var imgItem = border.FindName("ImgItem") as Image;
            if (imgItem == null) return;

            string path = null;
            if (item.HolidayProduct != null) path = item.HolidayProduct.ImagePath;
            else if (item.ProductOrService != null) path = item.ProductOrService.ImagePath;
            else if (item.Partner != null) path = item.Partner.ImagePath;

            if (!string.IsNullOrEmpty(path))
            {
                try { imgItem.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute)); }
                catch { imgItem.Source = null; }
            }
            else imgItem.Source = null;
        }

        private void CartItemRow_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as StackPanel;
            var item = panel?.DataContext as CartItem;
            if (item == null) return;

            var txtItemName = panel.FindName("TxtItemName") as TextBlock;
            var txtPartnerName = panel.FindName("TxtPartnerName") as TextBlock;
            var panelTimeInfo = panel.FindName("PanelTimeInfo") as StackPanel;

            if (item.HolidayProduct != null)
            {
                if (txtItemName != null) { txtItemName.Text = item.HolidayProduct.ProductName; txtItemName.Visibility = Visibility.Visible; }
                if (txtPartnerName != null) txtPartnerName.Visibility = Visibility.Collapsed;
                if (panelTimeInfo != null) panelTimeInfo.Visibility = Visibility.Collapsed;
            }
            else if (item.Partner != null)
            {
                if (txtItemName != null) txtItemName.Visibility = Visibility.Collapsed;
                if (txtPartnerName != null) txtPartnerName.Visibility = Visibility.Visible;
                if (panelTimeInfo != null) panelTimeInfo.Visibility = Visibility.Visible;
            }
            else
            {
                if (txtItemName != null) { txtItemName.Text = item.ProductOrService?.ItemName; txtItemName.Visibility = Visibility.Visible; }
                if (txtPartnerName != null) txtPartnerName.Visibility = Visibility.Collapsed;
                if (panelTimeInfo != null) panelTimeInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void CartItemQty_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = sender as StackPanel;
            var item = panel?.DataContext as CartItem;
            if (item == null) return;

            if (item.Partner != null) panel.Visibility = Visibility.Collapsed;
            else panel.Visibility = Visibility.Visible;
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.DataContext as CartItem;
            if (item != null)
            {
                CartService.Remove(item.ProductOrService?.ItemID, item.Partner?.PartnerID, item.HolidayProduct?.ProductID);
                LoadCartData();
                EvaluateCartType();
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (CartService.Items.Count == 0)
            {
                MyMessageBox.Show("Валидация", "Ваша корзина пуста!");
                return;
            }

            string title = TxtEventTitle.Text.Trim();
            var selectedPayment = ComboPaymentMethods.SelectedItem as PaymentMethods;

            DateTime eventDate = DateTime.Today;
            string location = "Продажа праздничной атрибутики (Доставка/Самовывоз)";

            if (!_isOnlyProducts)
            {
                if (DateEvent.SelectedDate == null)
                {
                    MyMessageBox.Show("Валидация", "Пожалуйста, укажите дату проведения праздника!");
                    return;
                }
                eventDate = DateEvent.SelectedDate.Value;

                if (_isVenueSelectedInCart)
                {
                    location = _cartVenueName;
                }
                else
                {
                    location = TxtCustomLocation.Text.Trim();
                    if (string.IsNullOrEmpty(location) || location == "Ресторан / Банкетный зал")
                    {
                        MyMessageBox.Show("Валидация", "Пожалуйста, введите точный адрес места проведения мероприятия!");
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(title) || selectedPayment == null)
            {
                MyMessageBox.Show("Валидация", "Пожалуйста, заполните параметры заказа!");
                return;
            }

            if (!_isOnlyProducts)
            {
                foreach (var item in CartService.Items)
                {
                    if (item.Partner != null && item.StartTime.HasValue && item.EndTime.HasValue)
                    {
                        bool available = CartService.IsServiceAvailable(item.Partner.PartnerID, eventDate, item.StartTime.Value, item.EndTime.Value);
                        if (!available)
                        {
                            MyMessageBox.Show("Занято", $"Специалист \"{item.Partner.PartnerName}\" уже занят в эти часы!");
                            return;
                        }
                    }
                }
            }

            try
            {
                decimal totalCost = CartService.GetTotalSum();

                int weddingTypeId = Core.Context.EventTypes.FirstOrDefault(t => t.TypeName == "Свадьба")?.EventTypeID ?? 1;
                int productSaleTypeId = Core.Context.EventTypes.FirstOrDefault(t => t.TypeName == "Покупка товаров")?.EventTypeID ?? weddingTypeId;

                var newEvent = new Events()
                {
                    ClientID = Core.CurrentUserId == 0 ? 1 : Core.CurrentUserId,
                    EventTypeID = _isOnlyProducts ? productSaleTypeId : weddingTypeId,
                    StatusID = 1,
                    Title = title,
                    EventDate = eventDate,
                    Location = location,
                    Budget = totalCost,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Events.Add(newEvent);
                Core.Context.SaveChanges();

                foreach (var item in CartService.Items)
                {
                    var orderItem = new EventItems()
                    {
                        EventID = newEvent.EventID,
                        ItemID = item.ProductOrService?.ItemID,
                        PartnerID = item.Partner?.PartnerID,
                        HolidayProductID = item.HolidayProduct?.ProductID,
                        Quantity = item.Quantity,
                        PricePerUnit = item.Partner != null ? item.Partner.PersonalPrice : (item.HolidayProduct != null ? item.HolidayProduct.Price : item.ProductOrService.BasePrice),
                        StartTime = item.StartTime,
                        EndTime = item.EndTime
                    };
                    Core.Context.EventItems.Add(orderItem);
                }
                var incomingPayment = new Transactions()
                {
                    EventID = newEvent.EventID,
                    TransactionTypeID = 1,
                    Amount = totalCost,
                    TransactionDate = DateTime.Now,
                    PaymentMethodID = selectedPayment.PaymentMethodID,
                    Description = $"Поступление средств от клиента за заказ { title }"
                };
                Core.Context.Transactions.Add(incomingPayment);
                foreach (var item in CartService.Items)
                {
                    decimal itemCost = 0;
                    string name = "";
                    if (item.HolidayProduct != null) continue;
                    if (item.Partner != null)
                    {
                        name = item.Partner.PartnerName;
                        if (item.StartTime.HasValue && item.EndTime.HasValue)
                        {
                            int hours = (int)Math.Ceiling((item.EndTime.Value - item.StartTime.Value).TotalHours);
                            if (hours <= 0) hours = 1;
                            itemCost = item.Partner.PersonalPrice * hours;
                        }
                        else itemCost = item.Partner.PersonalPrice;
                    }
                    else if (item.ProductOrService != null)
                    {
                        int pId = item.ProductOrService.PartnerID;
                        name = Core.Context.Partners.FirstOrDefault(p => p.PartnerID == pId)?.PartnerName ?? "Партнер";
                        itemCost = item.ProductOrService.BasePrice * item.Quantity;
                    }
                    var outgoingPayment = new Transactions()
                    {
                        EventID = newEvent.EventID,
                        TransactionTypeID = 2,
                        Amount = itemCost * 0.85m,
                        TransactionDate = DateTime.Now,
                        PaymentMethodID = selectedPayment.PaymentMethodID,
                        Description = $"Выплата партнеру { name } за позицию в заказе (удержана комиссия агентства 15%)"
                    };
                    Core.Context.Transactions.Add(outgoingPayment);
                }
                Core.Context.SaveChanges();
                MyMessageBox.Show("Успех", "Ваш заказ успешно оформлен! Данные локации и финансовые проводки сохранены в СУБД.");
                CartService.Clear();
                NavigationService.Navigate(new CatalogPage());
            }
            catch (Exception ex)
            {
                string innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MyMessageBox.Show("Ошибка БД", $"Критическая ошибка сохранения: {innerMessage}");
            }
        }
    }
}