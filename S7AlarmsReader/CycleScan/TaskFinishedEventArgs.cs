using S7Connections.TagsReading;
using System;
using System.Collections.Generic;

namespace S7AlarmsReader.CycleScan
{
    internal class TaskFinishedEventArgs : EventArgs
    {
        public long ScanTimeMiliseconds { get; set; }
        public Dictionary<int, List<AlarmDataModel>> AlarmsMemory { get; set; }
    }
}
