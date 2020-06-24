using Microsoft.Win32;
using System.IO;

namespace SMSHandlerUI.IOfilesHandling
{
    internal class OpenFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"> pass here the default file path</param>
        /// <param name="jsonOrCsv">0 - json, 1 - txt/csv</param>
        public void OpenFileDialog(ref string filePath, bool jsonOrCsv)
        {
            string filter;
            string defaultExtension;
            if (!jsonOrCsv)
            {
                filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                defaultExtension = "json";
            }
            else
            {
                filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv|All files (*.*)|*.*";
                defaultExtension = "txt";
            }

            //create instance of open dialog window
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Specify location of file to import",
                Multiselect = false,
                DefaultExt = defaultExtension,
                Filter = filter,
                FilterIndex = 1,
            };

            //set default directory
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
                fileDialog.FileName = Path.GetFileName(filePath);
            }

            //check choosen file path
            bool? result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                filePath = fileDialog.FileName;
            }
        }
    }
}
