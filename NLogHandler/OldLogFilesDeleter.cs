using NLog;
using System;
using System.IO;

namespace NLogHandler
{
    internal class OldLogFilesDeleter
    {
        #region Private fields

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public methods

        public void DeleteOldLogFiles(int days)
        {
            _logger.Info($"Method for deleting old log files fired. Amount of days: {days}.");

            if (CheckDaysAmount(days))
            {
                DirectoryInfo info = new DirectoryInfo(NLogConfigurator.LogFolder);
                FileInfo[] files = info.GetFiles("*.txt");

                //deleting files from folder with log txt files
                DateTime deletionDate = DateTime.Today.AddDays(days * -1);
                for (int i = 0; i < files.Length; i++)
                {
                    if (DateTime.Compare(files[i].CreationTime, deletionDate) < 0)
                    {
                        _logger.Info($"Log file: {files[i].Name} is beeing deleted.");
                        File.Delete(files[i].FullName);
                    }
                }
            }
        }

        #endregion

        #region Internal methods

        private bool CheckDaysAmount(int daysAmount)
        {
            return daysAmount >= 0;
        }

        #endregion
    }
}
