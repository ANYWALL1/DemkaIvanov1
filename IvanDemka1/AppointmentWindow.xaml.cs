using IvanDemka1.DataAccess;
using IvanDemka1.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace IvanDemka1
{
    public partial class AppointmentWindow : Window
    {
        private AppDbContext _context;
        private Service _currentService;
        private int _durationMins;

        public AppointmentWindow(Service service)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentService = service;
            _durationMins = service.DurationInSeconds / 60;

            tbServiceName.Text = service.Title;
            tbServiceDuration.Text = _durationMins.ToString();

            // Load clients
            var clients = _context.Clients.ToList()
                .Select(c => new { c.ID, FullName = $"{c.LastName} {c.FirstName} {c.Patronymic}" })
                .ToList();
            cbClients.ItemsSource = clients;
        }

        private void tbStartTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits and colon
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9:]+$");
        }

        private void tbStartTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-insert colon
            if (tbStartTime.Text.Length == 2 && !tbStartTime.Text.Contains(":"))
            {
                tbStartTime.Text += ":";
                tbStartTime.CaretIndex = tbStartTime.Text.Length;
            }

            // Validate format HH:mm
            if (Regex.IsMatch(tbStartTime.Text, @"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$"))
            {
                if (TimeSpan.TryParse(tbStartTime.Text, out TimeSpan startTime))
                {
                    TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(_durationMins));
                    // Formatting to show next day if passes midnight, but for simplicity just HH:mm
                    tbEndTime.Text = $"{(endTime.Days > 0 ? "След. день " : "")}{endTime:hh\\:mm}";
                }
            }
            else
            {
                tbEndTime.Text = "";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbClients.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(tbStartTime.Text, @"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$") || 
                !TimeSpan.TryParse(tbStartTime.Text, out TimeSpan time))
            {
                MessageBox.Show("Введите корректное время в формате ЧЧ:ММ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDateTime = dpDate.SelectedDate.Value.Add(time);

            if (startDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать запись на уже прошедшее время.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var clientService = new ClientService
            {
                ClientID = (int)cbClients.SelectedValue,
                ServiceID = _currentService.ID,
                StartTime = startDateTime
            };

            try
            {
                _context.ClientServices.Add(clientService);
                _context.SaveChanges();
                MessageBox.Show("Клиент успешно записан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
