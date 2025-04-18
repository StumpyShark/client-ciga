using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace AAAAAAAAAAAAAAAAAA
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient = new HttpClient();
        private List<Game> _games = new List<Game>();
        public MainWindow()
        {
            InitializeComponent();
            LoadGames();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        private async void LoadGames()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://server/api/games");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _games = JsonConvert.DeserializeObject<List<Game>>(content);
                    GamesListView.ItemsSource = _games;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка игр: {ex.Message}");
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            var filteredGames = _games.FindAll(g =>
                g.Title.ToLower().Contains(searchText) ||
                g.Genre.ToLower().Contains(searchText));

            GamesListView.ItemsSource = filteredGames;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (GamesListView.SelectedItem is Game selectedGame)
            {
                try
                {
                    var downloadUrl = $"http://server/api/games/download/{selectedGame.Id}";
                    var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        var totalBytes = response.Content.Headers.ContentLength ?? 0;
                        var stream = await response.Content.ReadAsStreamAsync();

                        using (var fileStream = System.IO.File.Create($"{selectedGame.Title}.exe"))
                        {
                            var buffer = new byte[8192];
                            int bytesRead;
                            long totalRead = 0;

                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;
                                DownloadProgress.Value = (double)totalRead / totalBytes * 100;
                            }
                        }

                        MessageBox.Show("Игра успешно скачана!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка скачивания: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите игру для скачивания");
            }
        }
    }

    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public double Rating { get; set; }
        public string Size { get; set; }
        public string FilePath { get; set; }
    }
}

