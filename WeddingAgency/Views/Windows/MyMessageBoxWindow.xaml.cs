using System.Windows;

namespace WeddingAgency.Views.Windows
{
    public partial class MyMessageBoxWindow : Window
    {
        public bool? Result { get; private set; } = false;

        public MyMessageBoxWindow(string title, string message, bool isConfirmation)
        {
            InitializeComponent();

            TxtTitle.Text = title;
            TxtMessage.Text = message;

            if (isConfirmation)
            {
                BtnOk.Visibility = Visibility.Collapsed;
                BtnYes.Visibility = Visibility.Visible;
                BtnNo.Visibility = Visibility.Visible;
            }
            else
            {
                BtnOk.Visibility = Visibility.Visible;
                BtnYes.Visibility = Visibility.Collapsed;
                BtnNo.Visibility = Visibility.Collapsed;
                Result = true;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
