using System.Windows;

namespace AlarmsClasses.CommonUsageClasses
{
    public class AlarmOccuredEventArgs : RoutedEventArgs
    {
        public string Message { get; set; }
        public int AlarmID { get; set; }
        public AlarmEnum AlarmType { get; set; }
        public LanguageEnum Language { get; set; }
    }
}
