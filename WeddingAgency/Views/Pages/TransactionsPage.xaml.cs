using System;
using System.Linq;
using System.Windows.Controls;

namespace WeddingAgency.Views.Pages
{
    public partial class TransactionsPage : Page
    {
        public TransactionsPage()
        {
            InitializeComponent();
            LoadFinancialReportData();
        }

        private void LoadFinancialReportData()
        {
            try
            {
                var transactions = Core.Context.Transactions
                    .Include("Events")
                    .Include("TransactionTypes")
                    .Include("PaymentMethods")
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                GridTransactions.ItemsSource = transactions;

                decimal totalIncome = transactions
                    .Where(t => t.TransactionTypeID == 1)
                    .Sum(t => t.Amount);

                decimal totalExpense = transactions
                    .Where(t => t.TransactionTypeID == 2)
                    .Sum(t => t.Amount);

                decimal netProfit = totalIncome - totalExpense;

                TxtTotalIncome.Text = $"{totalIncome:N2} руб.";
                TxtTotalExpense.Text = $"{totalExpense:N2} руб.";
                TxtNetProfit.Text = $"{netProfit:N2} руб.";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Не удалось сформировать финансовый отчет: {ex.Message}", "Ошибка БД");
            }
        }
    }
}
