namespace AlarmsClasses.CommonUsageClasses
{
    internal static class AlarmTypeNameObtainer
    {
        public static string GetAlarmName(AlarmEnum type)
        {
            switch (type)
            {
                case AlarmEnum.Bin: return "Binary alarm";
                case AlarmEnum.ElecProt: return "Electrical protection activation";
                case AlarmEnum.Contactor: return "Contactor failure";
                case AlarmEnum.Overload: return "Overload protection";
                case AlarmEnum.Isolator: return "Isolator activation";
                case AlarmEnum.Inverter: return "Inverter failure";
                case AlarmEnum.Run: return "Inverter not running";
                case AlarmEnum.Position: return "Wrong position";
                case AlarmEnum.Maintenance: return "Maintenance";
                case AlarmEnum.Break: return "Sensor break";
                case AlarmEnum.HHDeviation: return "High deviation";
                case AlarmEnum.LLDeviation: return "Low d eviation";
                case AlarmEnum.HighHigh: return "High high";
                case AlarmEnum.High: return "High";
                case AlarmEnum.LowLow: return "Low low";
                case AlarmEnum.Low: return "Low";
                default: return "Default alarm";
            }
        }
    }
}
