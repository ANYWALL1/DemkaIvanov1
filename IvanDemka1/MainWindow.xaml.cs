using IvanDemka1.DataAccess;
using IvanDemka1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IvanDemka1
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;
        private bool _isAdmin = false;
        private List<ServiceViewModel> _allServices;

        public MainWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Пересоздаем контекст, чтобы очистить кэш EF и получить свежие данные из БД
                _context = new AppDbContext();
                var services = _context.Services.ToList();
                _allServices = services.Select(s => new ServiceViewModel(s, _isAdmin)).ToList();
                FilterAndSortData(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterAndSortData(object sender, SelectionChangedEventArgs e)
        {
            if (_allServices == null) return;

            var filtered = _allServices.AsEnumerable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                var search = tbSearch.Text.ToLower();
                filtered = filtered.Where(s => 
                    s.Title.ToLower().Contains(search) || 
                    (s.Description != null && s.Description.ToLower().Contains(search)));
            }

            // Фильтрация по скидке
            if (cbFilter.SelectedIndex > 0)
            {
                switch (cbFilter.SelectedIndex)
                {
                    case 1: filtered = filtered.Where(s => s.Discount >= 0 && s.Discount < 5); break;
                    case 2: filtered = filtered.Where(s => s.Discount >= 5 && s.Discount < 15); break;
                    case 3: filtered = filtered.Where(s => s.Discount >= 15 && s.Discount < 30); break;
                    case 4: filtered = filtered.Where(s => s.Discount >= 30 && s.Discount < 70); break;
                    case 5: filtered = filtered.Where(s => s.Discount >= 70 && s.Discount <= 100); break;
                }
            }

            // Сортировка
            if (cbSort.SelectedIndex == 1) // Возрастание
            {
                filtered = filtered.OrderBy(s => s.CostWithDiscount);
            }
            else if (cbSort.SelectedIndex == 2) // Убывание
            {
                filtered = filtered.OrderByDescending(s => s.CostWithDiscount);
            }

            var result = filtered.ToList();
            lvServices.ItemsSource = result;
            tbCounter.Text = $"{result.Count} из {_allServices.Count}";
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndSortData(null, null);
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (tbAdminCode.Text == "0000")
            {
                _isAdmin = true;
                pnlAdminControls.Visibility = Visibility.Visible;
                LoadData(); // reload to apply visibility to items
                MessageBox.Show("Режим администратора активирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Неверный код.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ServiceEditWindow.IsOpen)
            {
                ServiceEditWindow.IsOpen = true;
                var win = new ServiceEditWindow();
                win.Show();
            }
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var vm = button.DataContext as ServiceViewModel;
            if (vm != null)
            {
                if (!ServiceEditWindow.IsOpen)
                {
                    ServiceEditWindow.IsOpen = true;
                    var win = new ServiceEditWindow(vm.Service);
                    win.Show();
                }
            }
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var vm = button.DataContext as ServiceViewModel;
            if (vm != null)
            {
                if (MessageBox.Show("Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var hasRecords = _context.ClientServices.Any(cs => cs.ServiceID == vm.Service.ID);
                    if (hasRecords)
                    {
                        MessageBox.Show("Нельзя удалить услугу, по которой есть записи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _context.Services.Remove(vm.Service);
                        _context.SaveChanges();
                        LoadData();
                    }
                }
            }
        }

        private void btnAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (lvServices.SelectedItem is ServiceViewModel selected)
            {
                var win = new AppointmentWindow(selected.Service);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите услугу");
            }
        }

        private void btnUpcoming_Click(object sender, RoutedEventArgs e)
        {
            var win = new UpcomingAppointmentsWindow();
            win.Show();
        }
        
        public void RefreshData()
        {
            LoadData();
        }
    }

    public class ServiceViewModel
    {
        public Service Service { get; set; }
        
        public ServiceViewModel(Service service, bool isAdmin)
        {
            Service = service;
            AdminVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility AdminVisibility { get; }
        public string Title => Service.Title;
        public string Description => Service.Description;
        public double Discount => (Service.Discount ?? 0) * 100; // В БД хранится как 0.05, умножаем на 100
        public decimal Cost => Service.Cost;
        
        public decimal CostWithDiscount => Discount > 0 ? Cost - (Cost * (decimal)Discount / 100m) : Cost;

        public string PriceDurationText 
        {
            get 
            {
                int durationMins = Service.DurationInSeconds / 60;
                if (Discount > 0)
                {
                    return $" {CostWithDiscount:0.##} рублей за {durationMins} минут";
                }
                else
                {
                    return $"{Cost:0.##} рублей за {durationMins} минут";
                }
            }
        }
        
        public string StrikethroughPrice => Discount > 0 ? $"{Cost:0.##}" : "";
        public Visibility StrikethroughVisibility => Discount > 0 ? Visibility.Visible : Visibility.Collapsed;
        
        public string ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(Service.MainImagePath)) return "pack://application:,,,/Resources/beauty_logo.png";
                
                string path = Service.MainImagePath.TrimStart('\\', '/');
                // The images folder might be named exactly as in the DB, e.g. "Услуги салона красоты/img.png"
                // Assuming it's copied to output directory or absolute path
                return System.IO.Path.Combine(Environment.CurrentDirectory, path);
            }
        }

        public SolidColorBrush BackgroundColor => Discount > 0 ? new SolidColorBrush(Color.FromRgb(225, 255, 225)) : Brushes.Transparent;
        
        public string DiscountText => Discount > 0 ? $"* скидка {Discount}%" : "";
    }
}
