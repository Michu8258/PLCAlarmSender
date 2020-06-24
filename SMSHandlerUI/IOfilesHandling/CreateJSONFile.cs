using Microsoft.Win32;
using System.IO;

namespace SMSHandlerUI.IOfilesHandling
{
    internal class CreateJSONFile
    {
        public void SaveFileDialg(ref string filePath)
        {
            //create instance of save file dialog window
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                CreatePrompt = true,
                OverwritePrompt = true,
                Title = "Specify new file location",
                DefaultExt = "json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
            };

            //set default directory
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
                fileDialog.FileName = Path.GetFileName(null);
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
