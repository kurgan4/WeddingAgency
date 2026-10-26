using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WeddingAgency.Services;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace WeddingAgency.Views.Pages
{
    public partial class AdminStatsPage : Page
    {
        public AdminStatsPage()
        {
            InitializeComponent();
            LoadFinancialAnalytics();
        }

        private void LoadFinancialAnalytics()
        {
            try
            {
                var transactions = Core.Context.Transactions.ToList();
                int currentYear = DateTime.Today.Year;

                // Фильтруем b2c-доходы (тип 1) за текущий год
                var incomeTransactions = transactions
                    .Where(t => t.TransactionTypeID == 1 && t.TransactionDate.Year == currentYear)
                    .ToList();

                // Считаем валовый доход за год
                decimal totalIncome = incomeTransactions.Sum(t => t.Amount);

                // Выплаты подрядчикам — это строго 85% от валового дохода
                decimal totalExpense = totalIncome * 0.85m;

                // Чистая прибыль агентства — это строго 15% от валового дохода
                decimal netProfit = totalIncome * 0.15m;

                // Выводим данные в верхние карточки (теперь они железно бьются с графиком)
                TxtTotalIncome.Text = $"{totalIncome:N2} руб.";
                TxtTotalExpense.Text = $"{totalExpense:N2} руб.";
                TxtNetProfit.Text = $"{netProfit:N2} руб.";

                UpdateAnalyticsChart();
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка", $"Не удалось загрузить финансовую аналитику: {ex.Message}");
            }
        }

        private void UpdateAnalyticsChart()
        {
            if (ChartCanvas == null) return;
            ChartCanvas.Children.Clear();

            double width = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            try
            {
                int currentYear = DateTime.Today.Year;
                var incomeTransactions = Core.Context.Transactions
                    .Where(t => t.TransactionTypeID == 1 && t.TransactionDate.Year == currentYear)
                    .ToList();

                decimal[] monthlyProfits = new decimal[12];
                for (int m = 1; m <= 12; m++)
                {
                    decimal grossMonthIncome = incomeTransactions
                        .Where(t => t.TransactionDate.Month == m)
                        .Sum(t => t.Amount);

                    // Чистая прибыль месяца — строго 15% маржи
                    monthlyProfits[m - 1] = grossMonthIncome * 0.15m;
                }

                decimal maxProfit = monthlyProfits.Max();
                if (maxProfit == 0) maxProfit = 10000;

                double colWidth = width / 12;
                double barWidth = colWidth - 15;

                Brush barBrush = Application.Current.Resources["AccentMainBrush"] as Brush ?? Brushes.DeepPink;
                Brush badgeBgBrush = Application.Current.Resources["AccentSecondaryBrush"] as Brush ?? Brushes.Purple;
                Brush mutedBrush = Application.Current.Resources["TextSecondaryBrush"] as Brush ?? Brushes.Gray;

                for (int i = 0; i < 12; i++)
                {
                    double barHeight = ((double)monthlyProfits[i] / (double)maxProfit) * (height - 50);
                    double leftPosition = (i * colWidth) + 7.5;

                    Rectangle rect = new Rectangle
                    {
                        Width = Math.Max(barWidth, 2),
                        Height = Math.Max(barHeight, 4),
                        Fill = barBrush,
                        RadiusX = 4,
                        RadiusY = 4
                    };

                    Canvas.SetLeft(rect, leftPosition);
                    Canvas.SetTop(rect, height - barHeight);
                    ChartCanvas.Children.Add(rect);

                    if (monthlyProfits[i] > 0)
                    {
                        Border badgeBorder = new Border
                        {
                            Background = badgeBgBrush,
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(5, 2, 5, 2)
                        };

                        TextBlock txtSum = new TextBlock
                        {
                            Text = monthlyProfits[i].ToString("N0"),
                            FontSize = 9,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White
                        };
                        badgeBorder.Child = txtSum;

                        Canvas.SetLeft(badgeBorder, leftPosition - 2);
                        Canvas.SetTop(badgeBorder, height - barHeight - 22);
                        ChartCanvas.Children.Add(badgeBorder);
                    }

                    TextBlock txtMonth = new TextBlock
                    {
                        Text = new DateTime(currentYear, i + 1, 1).ToString("MMM"),
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = mutedBrush
                    };
                    Canvas.SetLeft(txtMonth, leftPosition + (barWidth / 4));
                    Canvas.SetTop(txtMonth, height + 6);
                    ChartCanvas.Children.Add(txtMonth);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка генерации графика: {ex.Message}");
            }
        }



        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAnalyticsChart();
        }

        // ============================================================================
        // ОБРАБОТЧИКИ ЭКСПОРТА ОТЧЕТОВ WORD / PDF / EXCEL ОСТАЮТСЯ БЕЗ ИЗМЕНЕНИЙ
        // ============================================================================
        // ============================================================================
        // ВЫГРУЗКА СВОДНОГО ОТЧЕТА ПО МЕСЯЦАМ В WORD И PDF
        // ============================================================================
        private void BtnExportWord_Click(object sender, RoutedEventArgs e) => ExportWordOrPdf(false);
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e) => ExportWordOrPdf(true);

        private void ExportWordOrPdf(bool saveAsPdf)
        {
            try
            {
                // Извлекаем b2c-доходы (тип 1)
                var incomeTransactions = Core.Context.Transactions
                    .Where(t => t.TransactionTypeID == 1)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                if (incomeTransactions.Count == 0)
                {
                    MyMessageBox.Show("Экспорт", "Нет финансовых данных для формирования отчета!");
                    return;
                }

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                // Заголовок документа
                Word.Paragraph titleParagraph = document.Paragraphs.Add();
                Word.Range titleRange = titleParagraph.Range;
                titleRange.Text = "Сводный отчет о чистой прибыли агентства по месяцам";
                titleParagraph.set_Style("Заголовок 1");
                titleRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titleRange.InsertParagraphAfter();
                document.Paragraphs.Add();

                // Группируем доходы по Году и Месяцу
                var monthlyGroups = incomeTransactions
                    .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                    .OrderByDescending(g => g.Key.Year)
                    .ThenByDescending(g => g.Key.Month)
                    .ToList();

                // Создаем таблицу
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table statsTable = document.Tables.Add(tableRange, monthlyGroups.Count + 1, 3);

                statsTable.Borders.InsideLineStyle = statsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                statsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                // Стилизация всей таблицы
                statsTable.Range.Font.Name = "Times New Roman";
                statsTable.Range.Font.Size = 12;

                // Настройка шапки таблицы
                Word.Range cellRange;
                cellRange = statsTable.Cell(1, 1).Range; cellRange.Text = "Период (Год / Месяц)";
                cellRange = statsTable.Cell(1, 2).Range; cellRange.Text = "Валовый приход от клиентов";
                cellRange = statsTable.Cell(1, 3).Range; cellRange.Text = "Чистая прибыль (Маржа 15%)";

                // ИСПРАВЛЕНО: Явно берем Range ПЕРВОЙ строки таблицы (шапки) для жирного шрифта и центрирования
                Word.Range headerRowRange = statsTable.Rows[1].Range;
                headerRowRange.Font.Size = 13;
                headerRowRange.Font.Bold = 1;
                headerRowRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;


                decimal totalGrandProfit = 0;

                // Заполнение строк данными
                for (int i = 0; i < monthlyGroups.Count; i++)
                {
                    var group = monthlyGroups[i];
                    string periodText = $"{group.Key.Year} — {new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM")}";

                    decimal grossIncome = group.Sum(t => t.Amount);
                    decimal netMonthProfit = grossIncome * 0.15m;
                    totalGrandProfit += netMonthProfit;

                    // ИСПРАВЛЕНО: Убрана лишняя цепочка .Cell.Range, свойства применяются напрямую к cellRange
                    cellRange = statsTable.Cell(i + 2, 1).Range;
                    cellRange.Text = periodText;
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

                    cellRange = statsTable.Cell(i + 2, 2).Range;
                    cellRange.Text = grossIncome.ToString("N2") + " руб.";
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                    cellRange = statsTable.Cell(i + 2, 3).Range;
                    cellRange.Text = netMonthProfit.ToString("N2") + " руб.";
                    cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                }
                document.Paragraphs.Add();

                // Расчет максимального прибыльного месяца (Красный цвет)
                var maxMonth = monthlyGroups.OrderByDescending(g => g.Sum(t => t.Amount)).FirstOrDefault();
                if (maxMonth != null)
                {
                    Word.Paragraph maxParagraph = document.Paragraphs.Add();
                    Word.Range maxRange = maxParagraph.Range;
                    string maxMonthName = new DateTime(maxMonth.Key.Year, maxMonth.Key.Month, 1).ToString("MMMM");
                    maxRange.Text = $"Самый прибыльный период: {maxMonthName} {maxMonth.Key.Year} года (Маржа: {(maxMonth.Sum(t => t.Amount) * 0.15m):N2} руб.)";
                    maxParagraph.set_Style("Подзаголовок");
                    maxRange.Font.Color = Word.WdColor.wdColorDarkRed;
                    maxRange.InsertParagraphAfter();
                }
                document.Paragraphs.Add();

                // Расчет минимального прибыльного месяца (Зеленый цвет)
                var minMonth = monthlyGroups.OrderBy(g => g.Sum(t => t.Amount)).FirstOrDefault();
                if (minMonth != null)
                {
                    Word.Paragraph minParagraph = document.Paragraphs.Add();
                    Word.Range minRange = minParagraph.Range;
                    string minMonthName = new DateTime(minMonth.Key.Year, minMonth.Key.Month, 1).ToString("MMMM");
                    minRange.Text = $"Минимальный по прибыли период: {minMonthName} {minMonth.Key.Year} года (Маржа: {(minMonth.Sum(t => t.Amount) * 0.15m):N2} руб.)";
                    minParagraph.set_Style("Подзаголовок");
                    minRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                    minRange.InsertParagraphAfter();
                }

                // Автоматическая генерация колонтитулов
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.Size = 10;
                    headerRange.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }

                string tempFolder = System.IO.Path.GetTempPath();
                string docxPath = System.IO.Path.Combine(tempFolder, "FinancialMonthlyReport.docx");
                string pdfPath = System.IO.Path.Combine(tempFolder, "FinancialMonthlyReport.pdf");

                if (saveAsPdf)
                {
                    // Экспортируем в PDF
                    document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
                    document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
                    application.Quit();

                    // ИСПРАВЛЕНО: Принудительно открываем сгенерированный PDF-файл в браузере по умолчанию
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
                }
                else
                {
                    // Сохраняем в DOCX и открываем на экране приложения Word
                    document.SaveAs2(docxPath);
                    application.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка выгрузки", ex.Message);
            }
        }


        // ============================================================================
        // ВЫГРУЗКА СВОДНОГО ОТЧЕТА ПО МЕСЯЦАМ В EXCEL
        // ============================================================================
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var incomeTransactions = Core.Context.Transactions
                    .Where(t => t.TransactionTypeID == 1)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                if (incomeTransactions.Count == 0)
                {
                    MyMessageBox.Show("Экспорт", "Нет финансовых данных для формирования отчета!");
                    return;
                }

                var application = new Excel.Application();
                application.SheetsInNewWorkbook = 1; // Нужен всего 1 рабочий лист для сводной таблицы
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = application.Worksheets.Item[1];
                worksheet.Name = "Аналитика по месяцам";

                int startRowIndex = 1;

                // Создание заголовков столбцов
                worksheet.Cells[1][startRowIndex] = "Год";
                worksheet.Cells[2][startRowIndex] = "Месяц праздника";
                worksheet.Cells[3][startRowIndex] = "Валовый приход (руб)";
                worksheet.Cells[4][startRowIndex] = "Выплаты партнерам (85%)";
                worksheet.Cells[5][startRowIndex] = "Чистая прибыль (15%)";

                Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][1]];
                columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                columnHeaderRange.Font.Bold = true;
                startRowIndex++;

                // Группируем по годам и месяцам
                var monthlyGroups = incomeTransactions
                    .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                    .OrderByDescending(g => g.Key.Year)
                    .ThenByDescending(g => g.Key.Month)
                    .ToList();

                decimal grandTotalProfit = 0;
                int itemsCount = 0;

                foreach (var group in monthlyGroups)
                {
                    decimal grossIncome = group.Sum(t => t.Amount);
                    string monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM");

                    worksheet.Cells[1][startRowIndex] = group.Key.Year;
                    worksheet.Cells[2][startRowIndex] = monthName;

                    // Валовый приход
                    worksheet.Cells[3][startRowIndex] = grossIncome;
                    (worksheet.Cells[3][startRowIndex] as Excel.Range).NumberFormat = "#,##0.00";

                    // Формула выплат партнерам (85% от ячейки C)
                    worksheet.Cells[4][startRowIndex].Formula = $"=C{startRowIndex}*0.85";
                    (worksheet.Cells[4][startRowIndex] as Excel.Range).NumberFormat = "#,##0.00";
                    // Формула чистой прибыли (15% от ячейки C)
                    worksheet.Cells[5][startRowIndex].Formula = $"=C{startRowIndex}*0.15";
                    (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "#,##0.00";
                    grandTotalProfit += grossIncome * 0.15m;
                    startRowIndex++;
                    itemsCount++;
                }
                // Добавление строки ИТОГО (Шаг 14 методического файла)
                Excel.Range sumLabelRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[4][startRowIndex]];
                sumLabelRange.Merge();
                sumLabelRange.Value = "ОБЩИЙ ИТОГ ЧИСТОЙ ПРИБЫЛИ:";
                sumLabelRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                sumLabelRange.Font.Bold = true;
                // Рассчитываем сумму прибыли по формуле Excel SUM из 5 столбца (E)
                worksheet.Cells[5][startRowIndex].Formula = $"=SUM(E{startRowIndex - itemsCount}:E{startRowIndex - 1})";
                (worksheet.Cells[5][startRowIndex] as Excel.Range).Font.Bold = true;
                (worksheet.Cells[5][startRowIndex] as Excel.Range).Font.Color = Excel.XlRgbColor.rgbRed; // Красный цвет по ТЗ
                (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "#,##0.00";
                // Рисуем рамки и границы таблицы (Шаг 14 методического файла)
                Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][startRowIndex]];
                rangeBorders.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Columns.AutoFit();
                application.Visible = true;
                MyMessageBox.Show("Успех", "Сводная таблица финансовой аналитики по годам и месяцам успешно выгружена в Microsoft Excel.");
            }
            catch (Exception ex)
            {
                MyMessageBox.Show("Ошибка Excel", $"Не удалось сформировать книгу: {ex.Message}");
            }
        }
    }
}