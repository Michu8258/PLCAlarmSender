namespace AlarmsClasses.CommonUsageClasses
{
    public class AlarmTypeModel
    {
        public AlarmEnum AlarmType { get; set; }
        public string AlarmTypeName { get; set; }
        public bool AddThisAlarm { get; set; }
        public bool ActivateAlarm { get; set; }
    }
}
