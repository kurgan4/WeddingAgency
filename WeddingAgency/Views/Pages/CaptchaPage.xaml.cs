using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Navigation;
using WeddingAgency.Services;

namespace WeddingAgency.Views.Pages
{
    public partial class CaptchaPage : Page
    {
        private AuthPage _authPage;
        private string _currentCaptchaText;
        private Random _random = new Random();

        public CaptchaPage(AuthPage authPage)
        {
            InitializeComponent();
            _authPage = authPage;
            UpdateCaptcha();
        }

        private void UpdateCaptcha()
        {
            _currentCaptchaText = CaptchaService.GenerateCaptchaText();
            captchaInput.Clear();
            DrawCaptchaVisual();
        }

        private void DrawCaptchaVisual()
        {
            CaptchaCanvas.Children.Clear();

            double canvasWidth = 280;
            double canvasHeight = 60;

            for (int i = 0; i < 5; i++)
            {
                Line noiseLine = new Line
                {
                    X1 = _random.Next(0, (int)canvasWidth),
                    Y1 = _random.Next(0, (int)canvasHeight),
                    X2 = _random.Next(0, (int)canvasWidth),
                    Y2 = _random.Next(0, (int)canvasHeight),
                    Stroke = new SolidColorBrush(Color.FromRgb((byte)_random.Next(100, 200), (byte)_random.Next(50, 150), (byte)_random.Next(50, 150))),
                    StrokeThickness = _random.Next(1, 3)
                };
                CaptchaCanvas.Children.Add(noiseLine);
            }

            double letterSpacing = canvasWidth / (_currentCaptchaText.Length + 1);

            for (int i = 0; i < _currentCaptchaText.Length; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = _currentCaptchaText[i].ToString(),
                    FontSize = _random.Next(24, 32),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb((byte)_random.Next(150, 255), 0, 0)),
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                double angle = _random.Next(-25, 25);
                textBlock.RenderTransform = new RotateTransform(angle);

                double xPos = (i + 0.5) * letterSpacing + _random.Next(-5, 5);
                double yPos = _random.Next(10, 20);

                Canvas.SetLeft(textBlock, xPos);
                Canvas.SetTop(textBlock, yPos);

                CaptchaCanvas.Children.Add(textBlock);
            }

            for (int i = 0; i < 3; i++)
            {
                Line foregroundLine = new Line
                {
                    X1 = _random.Next(0, (int)canvasWidth),
                    Y1 = _random.Next(0, (int)canvasHeight),
                    X2 = _random.Next(0, (int)canvasWidth),
                    Y2 = _random.Next(0, (int)canvasHeight),
                    Stroke = new SolidColorBrush(Color.FromRgb((byte)_random.Next(200, 255), 0, 0)),
                    StrokeThickness = _random.Next(1, 2)
                };
                CaptchaCanvas.Children.Add(foregroundLine);
            }
        }

        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text.Trim() == _currentCaptchaText)
            {
                MyMessageBox.Show("Успех", "Проверка пройдена успешно. Вы можете попробовать войти снова.");
                _authPage.ResetAttempts();
                NavigationService.Navigate(_authPage);
            }
            else
            {
                MyMessageBox.Show("Ошибка", "Неверно введена капча. Попробуйте еще раз.");
                UpdateCaptcha();
            }
        }

        private void refreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            UpdateCaptcha();
        }
    }
}
