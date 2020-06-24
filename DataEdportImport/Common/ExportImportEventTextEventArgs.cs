using System;

namespace DataEdportImport.Common
{
    public class ExportImportEventTextEventArgs : EventArgs
    {
        public string MessageText { get; set; }
        public string ObjectName { get; set; }
        public bool Success { get; set; }
        public bool Done { get; set; }
    }
}
