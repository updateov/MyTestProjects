using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Practices.Prism.Commands;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Newtonsoft.Json;
using Microsoft.Practices.Prism.ViewModel;
using System.Globalization;

namespace TestNoda
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new VM();
            DataContext = vm;
        }
    }

    public class VM : NotificationObject
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        public VM()
        {
            Clicked = new DelegateCommand(Button_Clicked);
            JSONText = "{}";
        }

        private void Button_Clicked()
        {
            switch (m_cnt++)
            {
                case 0:
                    TimeToJSON();
                    break;
                case 1:
                    TicksToJSON();
                        break;
                default:
                    TicksToJSON();
                    break;
            }
        }

        private void TicksToJSON()
        {
            Instant now = SystemClock.Instance.Now;
            long ticks = now.Ticks;
            JSONText = JsonConvert.SerializeObject(ticks, Formatting.None, settings);
        }

        private void TimeToJSON()
        {
            Instant now = SystemClock.Instance.Now;
            String timeZone = TimeZoneInfo.Local.DisplayName;
            long ticks = now.Ticks;
            String timeZoneStan = TimeZoneInfo.Local.StandardName;
            String timeZoneId = TimeZoneInfo.Local.Id;
            var nowUtc = now.InUtc();
            DateTimeOffset odt = DateTimeOffset.Now;
            DateTime dt = odt.DateTime;
            DateTimeZoneProviders.Serialization = DateTimeZoneProviders.Bcl;
            //var curZone = DateTimeZoneProviders.Tzdb["Canada/Eastern"];
            var curZone = DateTimeZoneProviders.Bcl[timeZoneId];
            var nowZone = now.InZone(curZone);
            var offset = nowZone.ToOffsetDateTime(); 
            var duration = NodaTime.Duration.FromMinutes(3);
            var local = new LocalDateTime(nowZone.Year, nowZone.Month, nowZone.Day, nowZone.Hour, nowZone.Minute, nowZone.Second, nowZone.Millisecond);
            var localInUtc = local.InUtc();
            var inst = localInUtc.ToInstant();
            var odtToStr = odt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
            var offsetToStr = offset.ToString("yyyy-MM-ddTHH:mm:ss.ffffffo<m>", CultureInfo.InvariantCulture);
            String nowStr = JsonConvert.SerializeObject(now, Formatting.None, settings);
            String offSet = JsonConvert.SerializeObject(offset, Formatting.Indented, settings);
            String utc = JsonConvert.SerializeObject(nowUtc, Formatting.None, settings);
            String dur = JsonConvert.SerializeObject(duration, Formatting.None, settings);
            String loc = JsonConvert.SerializeObject(local, Formatting.None, settings);
            String loc2Str = local.ToString();
            //JSONText = String.Format("Time: {0}\nOffset: {1}\nUtc time: {2}\nZoned time: {3}\nTime zone name: {4}\nTime zone name standard: {5}\nLocal time: {6}\nTostr local: {7}\nTicks: {8}",
            //                            nowStr, offSet, utc, nowZone, timeZone, timeZoneStan, loc, loc2Str, ticks);
            JSONText = String.Format("Time: {0}\nOffset: {1}\nZoned time: {2}\nOffset To String: {3}",
                                        nowStr, offSet, nowZone, offset.ToString());
        }

        private String m_JSONText;
        public String JSONText
        {
            get { return m_JSONText; }
            set
            {
                m_JSONText = value;
                RaisePropertyChanged(() => JSONText);
            }
        }

        private int m_cnt = 0;
        public DelegateCommand Clicked { get; set; }
    }

    public static class Exts
    {
        public static string ToString(this String str, int n)
        {

            return str.ToString();
        }
    }
}
