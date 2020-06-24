using System;

namespace DataEdportImport.Common
{
    public class ImportElementsCountEventArgs : EventArgs
    {
        public int MaxValueOfProgressBar { get; set; }
    }
}
