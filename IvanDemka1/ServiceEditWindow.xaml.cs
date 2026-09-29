using IvanDemka1.DataAccess;
using IvanDemka1.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace IvanDemka1
{
    public partial class ServiceEditWindow : Window
    {
        public static bool IsOpen = false;
        private AppDbContext _context;
        private Service _currentService;
        private string _newImagePath = null;

        public ServiceEditWindow(Service service = null)
        {
            InitializeComponent();
            _context = new AppDbContext();
            
            if (service != null)
            {
                // Редактирование
                _currentService = _context.Services.Find(service.ID);
                tbId.Text = _currentService.ID.ToString();
                tbTitle.Text = _currentService.Title;
                tbCost.Text = _currentService.Cost.ToString("G29");
                tbDuration.Text = (_currentService.DurationInSeconds / 60).ToString();
                tbDiscount.Text = _currentService.Discount?.ToString();
                tbDescription.Text = _currentService.Description;
                
                if (!string.IsNullOrEmpty(_currentService.MainImagePath))
                {
                    tbImagePath.Text = _currentService.MainImagePath;
                    try
                    {
                        string path = Path.Combine(Environment.CurrentDirectory, _currentService.MainImagePath.TrimStart('\\', '/'));
                        if (File.Exists(path))
                            imgMain.Source = new BitmapImage(new Uri(path));
                    }
                    catch { }
                }
            }
            else
            {
                // Добавление
                _currentService = new Service();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsOpen = false;
            // Refresh main window list
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Dispatcher.Invoke(() => 
                {
                    mainWindow.RefreshData();
                });
            }
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files|*.jpg;*.jpeg;*.png";
            if (ofd.ShowDialog() == true)
            {
                _newImagePath = ofd.FileName;
                imgMain.Source = new BitmapImage(new Uri(_newImagePath));
                tbImagePath.Text = Path.GetFileName(_newImagePath);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                MessageBox.Show("Название не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(tbCost.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Некорректная стоимость", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbDuration.Text, out int durationMins) || durationMins < 0 || durationMins > 240)
            {
                MessageBox.Show("Длительность должна быть от 0 до 240 минут (4 часа)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double? discount = null;
            if (!string.IsNullOrWhiteSpace(tbDiscount.Text))
            {
                if (double.TryParse(tbDiscount.Text, out double d) && d >= 0 && d <= 100)
                    discount = d;
                else
                {
                    MessageBox.Show("Скидка должна быть числом от 0 до 100", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Уникальность названия
            var existingService = _context.Services.FirstOrDefault(s => s.Title == tbTitle.Text && s.ID != _currentService.ID);
            if (existingService != null)
            {
                MessageBox.Show("Услуга с таким названием уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохранение картинки
            if (_newImagePath != null)
            {
                string targetFolder = Path.Combine(Environment.CurrentDirectory, "Услуги салона красоты");
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);
                
                string ext = Path.GetExtension(_newImagePath);
                string newFileName = Guid.NewGuid().ToString() + ext;
                string targetPath = Path.Combine(targetFolder, newFileName);
                
                File.Copy(_newImagePath, targetPath, true);
                _currentService.MainImagePath = $"Услуги салона красоты\\{newFileName}";
            }

            _currentService.Title = tbTitle.Text;
            _currentService.Cost = cost;
            _currentService.DurationInSeconds = durationMins * 60;
            _currentService.Discount = discount;
            _currentService.Description = tbDescription.Text;

            if (_currentService.ID == 0)
            {
                _context.Services.Add(_currentService);
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Услуга успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
