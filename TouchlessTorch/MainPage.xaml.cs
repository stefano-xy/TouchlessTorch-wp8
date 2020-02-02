using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace TouchlessTorch
{
    public partial class MainPage : PhoneApplicationPage
    {
        readonly Controller _controller = new Controller();
        readonly DispatcherTimer _timer = new DispatcherTimer();
        readonly Brush _onBrush = (Brush)App.Current.Resources["PhoneForegroundBrush"];
        readonly Brush _offBrush = (Brush)App.Current.Resources["PhoneInactiveBrush"];
        readonly Brush _emergencyBrush = new SolidColorBrush(Colors.Red);
        
        public MainPage()
        {
            InitializeComponent();           

            _timer.Interval = new TimeSpan(0, 0, 2);
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_controller.Ready)
                _controller.Enabled = true;

            UpdateClock();
            UpdateBattery();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine(this.GetHashCode() + ", OnNavigatedTo " + e.NavigationMode.ToString());

            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Reset)
            {
                UpdateClock();
                UpdateBattery();

                if (! _controller.Ready)
                    await _controller.InitAsync();

                if (_controller.Ready)
                    _controller.Enabled = true;

                _timer.Start();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Debug.WriteLine(this.GetHashCode() + ", OnNavigatedFrom " + e.NavigationMode.ToString());

            _timer.Stop();

            if (_controller.Ready)
                _controller.Enabled = false;

            _controller.Term();

            base.OnNavigatingFrom(e);
        }

        private void UpdateClock()
        {
            TimeDisplay.Text = DateTime.Now.ToShortTimeString();
        }

        private void UpdateBattery()
        {
            int charge = Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent;
            bool charging = Microsoft.Phone.Info.DeviceStatus.PowerSource == Microsoft.Phone.Info.PowerSource.External;

            BatteryDisplay.Text = charge.ToString();

            p0.Fill = (!charging && charge < 20) ? _emergencyBrush : _offBrush;
            p1.Fill = (!charging && charge >= 20 && charge < 40) ? _onBrush : _offBrush;
            p2.Fill = (!charging && charge >= 40 && charge < 80) ? _onBrush : _offBrush;
            p3.Fill = (!charging && charge >= 80) ? _onBrush : _offBrush;
            pc.Fill = charging ? _onBrush : _offBrush;
        }
    }
}