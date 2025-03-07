using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ParallelAsyncExample
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _client = new HttpClient { MaxResponseContentBufferSize = 1_000_000 };

        private readonly IEnumerable<string> _urlList = new string[]
        {
            "https://docs.microsoft.com",
            "https://docs.microsoft.com/azure",
            "https://docs.microsoft.com/powershell",
            "https://docs.microsoft.com/dotnet",
            "https://docs.microsoft.com/aspnet/core",
            "https://docs.microsoft.com/windows",
            "https://docs.microsoft.com/office",
            "https://docs.microsoft.com/enterprise-mobility-security",
            "https://docs.microsoft.com/visualstudio",
            "https://docs.microsoft.com/microsoft-365",
            "https://docs.microsoft.com/sql",
            "https://docs.microsoft.com/dynamics365",
            "https://docs.microsoft.com/surface",
            "https://docs.microsoft.com/xamarin",
            "https://docs.microsoft.com/azure/devops",
            "https://docs.microsoft.com/system-center",
            "https://docs.microsoft.com/graph",
            "https://docs.microsoft.com/education",
            "https://docs.microsoft.com/gaming"
        };

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            _startButton.IsEnabled = false;
            _resultsTextBox.Clear();

            Task.Run(() => StartSumPageSizesAsync());
        }

        // 開始計算所有頁面的大小
        private async Task StartSumPageSizesAsync()
        {
            // 計算頁面大小
            await SumPageSizesAsync();
            // 更新 UI 控制項
            await Dispatcher.BeginInvoke(() =>
            {
                _resultsTextBox.Text += $"\nControl returned to {nameof(OnStartButtonClick)}.";
                _startButton.IsEnabled = true;
            });
        }

        /// <summary>
        /// Asynchronously sums the sizes of pages from a list of URLs.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SumPageSizesAsync()
        {
            // 開始計時以測量經過的時間
            var stopwatch = Stopwatch.StartNew();

            // 建立一個查詢，當執行時，返回一個任務集合
            IEnumerable<Task<int>> downloadTasksQuery =
            from url in _urlList
            select ProcessUrlAsync(url, _client);

            // 執行查詢並啟動任務
            Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

            // 等待所有任務完成並獲取它們的結果
            int[] lengths = await Task.WhenAll(downloadTasks);
            // 將所有下載內容的長度相加
            int total = lengths.Sum();

            // 更新 UI，顯示總字節數和經過的時間
            await Dispatcher.BeginInvoke(() =>
            {
            stopwatch.Stop();

            _resultsTextBox.Text += $"\n總返回字節數:  {total:#,#}";
            _resultsTextBox.Text += $"\n經過的時間:    {stopwatch.Elapsed}\n";
            });
        }

        private async Task<int> ProcessUrlAsync(string url, HttpClient client)
        {
            byte[] byteArray = await client.GetByteArrayAsync(url);
            await DisplayResultsAsync(url, byteArray);

            return byteArray.Length;
        }

        private Task DisplayResultsAsync(string url, byte[] content) =>
            Dispatcher.BeginInvoke(() =>
                _resultsTextBox.Text += $"{url,-60} {content.Length,10:#,#}\n")
                      .Task;

        protected override void OnClosed(EventArgs e) => _client.Dispose();
    }
}
