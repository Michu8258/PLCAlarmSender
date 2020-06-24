using System.Collections.Generic;

namespace DataEdportImport.AlarmProfilesHandling
{
    internal class FullAlarmProfileDefinition
    {
        public AlarmProfileDefinitionExportModel AlarmProfileDefinition { get; set; }
        public List<AlarmProfileSingleDayExportModel> DaysList { get; set; }
        public bool NoErrors { get; set; }
    }
}
