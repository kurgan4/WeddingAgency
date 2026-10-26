using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class AdminRequestsPage : Page
    {
        public AdminRequestsPage()
        {
            InitializeComponent();
            LoadAllRequests();
        }

        private void LoadAllRequests()
        {
            try
            {
                GridRequests.ItemsSource = Core.Context.Events
                    .Include("EventTypes")
                    .Include("Users")
                    .Include("EventStatuses")
                    .Include("EventItems.CatalogItems")
                    .Include("EventItems.Partners")
                    .Include("EventItems.HolidayProducts")
                    .OrderByDescending(e => e.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось загрузить списки входящих заявок: {ex.Message}");
            }
        }

        private void ComboRequestStatus_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            try { comboBox.ItemsSource = Core.Context.EventStatuses.ToList(); }
            catch { }
        }

        private void ComboRequestStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var currentEvent = comboBox?.DataContext as Events;
            if (currentEvent == null || comboBox.SelectedValue == null) return;

            currentEvent.StatusID = (int)comboBox.SelectedValue;
        }

        private void BtnAcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var currentEvent = button?.DataContext as Events;
            if (currentEvent == null) return;

            try
            {
                var eventInDb = Core.Context.Events.FirstOrDefault(ev => ev.EventID == currentEvent.EventID);
                if (eventInDb != null)
                {
                    // Сохраняем измененный статус в таблицу заказов
                    eventInDb.StatusID = currentEvent.StatusID;

                    // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Синхронизация с кассой (таблицей Transactions)
                    // Предположим, что в вашем справочнике статус "Отменено" имеет ID = 5.
                    // Если вы перевели заказ в статус отмены:
                    if (currentEvent.StatusID == 5)
                    {
                        // Полностью вырезаем все доходы и расходы по этому заказу из кассы
                        var oldTransactions = Core.Context.Transactions.Where(t => t.EventID == currentEvent.EventID).ToList();
                        foreach (var trans in oldTransactions)
                        {
                            Core.Context.Transactions.Remove(trans);
                        }
                    }
                    else
                    {
                        // Если заказ вернули из отмены обратно в работу (Новый, В разработке, Утвержден),
                        // а проводок в кассе почему-то нет — генерируем их заново, чтобы баланс сошелся
                        bool hasMoney = Core.Context.Transactions.Any(t => t.EventID == currentEvent.EventID && t.TransactionTypeID == 1);
                        if (!hasMoney && currentEvent.Budget > 0)
                        {
                            // Восстанавливаем b2c-доход
                            var incomingPayment = new Transactions()
                            {
                                EventID = currentEvent.EventID,
                                TransactionTypeID = 1,
                                Amount = currentEvent.Budget,
                                TransactionDate = DateTime.Now,
                                PaymentMethodID = 1, // Дефолтный метод (например, Карта)
                                Description = $"Восстановление: Поступление средств за возобновленный заказ №{currentEvent.EventID}"
                            };
                            Core.Context.Transactions.Add(incomingPayment);

                            // Восстанавливаем b2b-выплату партнерам (85%)
                            var outgoingPayment = new Transactions()
                            {
                                EventID = currentEvent.EventID,
                                TransactionTypeID = 2,
                                Amount = ROUND_Value(currentEvent.Budget * 0.85m),
                                TransactionDate = DateTime.Now,
                                PaymentMethodID = 1,
                                Description = $"Восстановление: Выплата партнерам за позиции в сметном заказе (-15%)"
                            };
                            Core.Context.Transactions.Add(outgoingPayment);
                        }
                    }

                    Core.Context.SaveChanges();

                    MyMessageBox.Show("Успех", $"Статус сметного заказа №{currentEvent.EventID} успешно изменен. Данные в кассе пересчитаны!");
                    LoadAllRequests();
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка БД", $"Не удалось обновить статус и пересчитать кассу: {ex.Message}");
            }
        }

        private decimal ROUND_Value(decimal val)
        {
            return Math.Round(val, 2);
        }
    }
}
