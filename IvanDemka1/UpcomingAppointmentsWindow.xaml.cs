using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using IvanDemka1.DataAccess;
using IvanDemka1.Models;

namespace IvanDemka1
{
    public partial class UpcomingAppointmentsWindow : Window
    {
        private AppDbContext _context;
        private DispatcherTimer _timer;

        public UpcomingAppointmentsWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var today = DateTime.Today;
                var afterTomorrow = today.AddDays(2);
                
                var records = _context.ClientServices
                    .Include("Service")
                    .Include("Client")
                    .Where(cs => cs.StartTime >= today && cs.StartTime < afterTomorrow)
                    .OrderBy(cs => cs.StartTime)
                    .ToList()
                    .Select(cs => new AppointmentViewModel(cs))
                    .ToList();

                lvAppointments.ItemsSource = records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }

    public class AppointmentViewModel
    {
        private ClientService _cs;

        public AppointmentViewModel(ClientService cs)
        {
            _cs = cs;
        }

        public string ServiceName => _cs.Service.Title;
        public string ClientName => $"{_cs.Client.LastName} {_cs.Client.FirstName} {_cs.Client.Patronymic}";
        public string ClientContacts => $"Email: {_cs.Client.Email} | Тел: {_cs.Client.Phone}";
        public string StartDateTime => _cs.StartTime.ToString("dd.MM.yyyy HH:mm");

        public string TimeRemaining
        {
            get
            {
                var time = _cs.StartTime - DateTime.Now;
                if (time.TotalSeconds < 0) return "Началось/Прошло";
                return $"{time.Hours} часов {time.Minutes} минут";
            }
        }

        public SolidColorBrush RemainingTimeColor
        {
            get
            {
                var time = _cs.StartTime - DateTime.Now;
                if (time.TotalSeconds > 0 && time.TotalHours < 1)
                    return Brushes.Red;
                return Brushes.Black;
            }
        }
    }
}
