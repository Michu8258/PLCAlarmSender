using Caliburn.Micro;
using RealmDBHandler.NLogConfig;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NLog;
using RealmDBHandler.CommonClasses;

namespace SMSHandlerUI.ViewModels
{
    class NLogConfigurationViewModel : Screen
    {
        #region Fields and propeties

        //fields
        private NlogConfigModel _selectedConfig;
        private BindableCollection<NlogConfigModel> _configs;
        private List<NLogConfigurationDefinition> _originalConfigList;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;

        //properties
        public NlogConfigModel SelectedConfig { get { return _selectedConfig; } set { _selectedConfig = value; NotifyOfPropertyChange(() => SelectedConfig); } }
        public BindableCollection<NlogConfigModel> Configs { get { return _configs; } set { _configs = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public NLogConfigurationViewModel(IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            ReadSavedConfigs();
            _logger.Info($"NLog configuration window created.");
        }

        #endregion

        #region Reading saved Configs from DB

        //main method for updating data - reading it from DB
        private void ReadSavedConfigs()
        {
            _logger.Info($"Reading saved NLog configurations procedure started.");

            Configs = new BindableCollection<NlogConfigModel>();
            _originalConfigList = ReadAllData();
            
            foreach (var item in _originalConfigList)
            {
                Configs.Add(new NlogConfigModel()
                {
                    Identity = item.Identity,
                    CreatedBy = item.CreatedBy,
                    ModifiedBy = item.ModifiedBy,
                    DeleteOldLogs = item.OldLogDeletion,
                    DeleteoldLogsDays = item.OldLogDeletionDays,
                    NewLogFileHours = item.HoursToCreateNewLogFile,
                    ConfigActivated = item.ConfigActivated,
                    DeleteDays = CreateDeleteOldLogDaysList(),
                    HoursForNewFile = CreateHoursToCreateNewLogFileList()
                });
            }

            AssignSelectedDaysToDelete();
            AssignSelectedHoursToNewLogFile();
        }

        //selecting proper item in checkbox of days to delete old log files
        private void AssignSelectedDaysToDelete()
        {
            _logger.Info($"Assigning amount of days that old log files should be deleted.");

            foreach (var item in Configs)
            {
                item.SelectedDays = item.DeleteDays.Single(x => x.Days == item.DeleteoldLogsDays);
            }
        }

        //creation of combobox list items - days to delete old log files
        private BindableCollection<DeleteOldLogFilesDaysModel> CreateDeleteOldLogDaysList()
        {
            BindableCollection<DeleteOldLogFilesDaysModel> list = new BindableCollection<DeleteOldLogFilesDaysModel>
            {
                CreateOneDeleteOldLogFilesDaysListElement(1),
                CreateOneDeleteOldLogFilesDaysListElement(2),
                CreateOneDeleteOldLogFilesDaysListElement(3),
                CreateOneDeleteOldLogFilesDaysListElement(5),
                CreateOneDeleteOldLogFilesDaysListElement(7),
                CreateOneDeleteOldLogFilesDaysListElement(14),
                CreateOneDeleteOldLogFilesDaysListElement(30)
            };

            return list;
        }

        //creating model of one item of combobox with amount of days to delete old log files
        private DeleteOldLogFilesDaysModel CreateOneDeleteOldLogFilesDaysListElement(int days)
        {
            return new DeleteOldLogFilesDaysModel()
            {
                Days = days,
                DaysString = $"{days} day(s)",
            };
        }

        //selecting proper item in checkbox of amount of hours that need to pass to create new log file
        private void AssignSelectedHoursToNewLogFile()
        {
            _logger.Info($"Assigning amount of hours that need to pass to create new log file.");

            foreach (var item in Configs)
            {
                item.SelectedHours = item.HoursForNewFile.Single(x => x.Hours == item.NewLogFileHours);
            }
        }

        //creation of combobox list items - amount of hours that need to pass to create new log file
        private BindableCollection<HourToCreateNewLogFileModel> CreateHoursToCreateNewLogFileList()
        {
            BindableCollection<HourToCreateNewLogFileModel> list = new BindableCollection<HourToCreateNewLogFileModel>()
            {
                CreateOneHoursToCreateNewLogFileListElement(6),
                CreateOneHoursToCreateNewLogFileListElement(12),
                CreateOneHoursToCreateNewLogFileListElement(24),
                CreateOneHoursToCreateNewLogFileListElement(48),
                CreateOneHoursToCreateNewLogFileListElement(72),
                CreateOneHoursToCreateNewLogFileListElement(120),
                CreateOneHoursToCreateNewLogFileListElement(168),
            };

            return list;
        }

        //creating model of one item of combobox with amount of hours that need to pass to create new log file
        private HourToCreateNewLogFileModel CreateOneHoursToCreateNewLogFileListElement(int hours)
        {
            if (hours < 24) return new HourToCreateNewLogFileModel() { Hours = hours, TimeString = $"{hours} hour(s)" };
            else return new HourToCreateNewLogFileModel() { Hours = hours, TimeString = $"{hours / 24} day(s)" };
        }

        //method that does the job - reads values from DB
        private List<NLogConfigurationDefinition> ReadAllData()
        {
            NlogConfigReader reader = new NlogConfigReader(_realmProvider);
            return reader.GetConfigs();
        }

        #endregion

        #region Activated checkBoxes handling

        public void ActivatedCheckBoxClick(int identity)
        {
            //if clicked checkbox was not selected before
            if (Configs.Single(x => x.Identity == identity).ConfigActivated)
            {
                MakeAllConfigsInactivated();
            }
            Configs.Single(x => x.Identity == identity).ConfigActivated = true;

            _logger.Info($"NLog configuration with ID = {identity} was activated.");
        }

        private void MakeAllConfigsInactivated()
        {
            foreach (var item in Configs)
            {
                item.ConfigActivated = false;
            }
        }

        #endregion

        #region Adding new NLog Configuration

        public void AddNewConfigButton()
        {
            _logger.Info($"Adding new NLog configuration to DB started. Whis is also reaction for button.");

            if (!AddNewConfigToDB())
            {
                _logger.Error($"Saving new NLog configuration to DB went wrong!");
                MessageBox.Show("Creation of new config failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ReadSavedConfigs();
        }

        private bool AddNewConfigToDB()
        {
            NlogConfigCreator creator = new NlogConfigCreator(_realmProvider);
            return creator.AddNewNlogConfigToDB(_runtimeData.DataOfCurrentlyLoggedUser.UserName, false, 30, 168);
        }

        #endregion

        #region Saving data

        //method from button
        public void ApplyChanges()
        {
            _logger.Info($"Button for applying changes and saving those changes to DB, pressed.");

            bool success = SendUpdatedDataToDB();
            if (!success)
            {
                _logger.Error($"Saving modified data of NLog configurationd to DB went wrong!");
                MessageBox.Show("Not all data was successfully saved.", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ReadSavedConfigs();

            //changing NLog config during runtime
            AppConfigHandler.ChangeCurrentNLogConfig();
        }

        //sending data only if necessary
        private bool SendUpdatedDataToDB()
        {
            NlogConfigModifier modifier = new NlogConfigModifier(_realmProvider);
            bool allDataWrittenProperly = true;

            foreach (var item in Configs)
            {
                (bool dataChanged, bool acivenessChanged) = CheckIfConfigChanged(item, _originalConfigList.Single(x => x.Identity == item.Identity));

                //if a config was modified
                if (dataChanged || acivenessChanged)
                {
                    _logger.Info($"Saving modified NLog config to DB. config ID = {item.Identity}, created by: {item.CreatedBy}.");

                    bool success = modifier.ModifyNlogConfigDefinition(item.Identity, _runtimeData.DataOfCurrentlyLoggedUser.UserName,
                        item.DeleteOldLogs, item.SelectedDays.Days, item.SelectedHours.Hours, item.ConfigActivated, !dataChanged && acivenessChanged);
                    if (!success) allDataWrittenProperly = false;
                }
            }

            return allDataWrittenProperly;
        }

        //checking what data changed
        private (bool, bool) CheckIfConfigChanged(NlogConfigModel item, NLogConfigurationDefinition originalItem)
        {
            bool modifiedData = false;
            bool modifiedOnlyActivityOfConfig = false;

            if (item.DeleteOldLogs != originalItem.OldLogDeletion || item.SelectedDays.Days != originalItem.OldLogDeletionDays ||
                item.SelectedHours.Hours != originalItem.HoursToCreateNewLogFile)
            {
                modifiedData = true;
            }
            if (item.ConfigActivated != originalItem.ConfigActivated)
            {
                modifiedOnlyActivityOfConfig = true;
            }

            return (modifiedData, modifiedOnlyActivityOfConfig);
        }

        #endregion

        #region Deleting selected config

        public void DeleteSelectedConfig()
        {
            if (SelectedConfig != null)
            {
                _logger.Info($"Button for deleting selected NLog configuration pressed. Selected config is not equal to null.");

                if (SelectedConfig.ConfigActivated)
                {
                    _logger.Info($"User tried to delete active config, but that cannot be done.");
                    MessageBox.Show("You cannot delete active config", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBoxResult msgRes = MessageBox.Show("Are you sure?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (msgRes == MessageBoxResult.OK)
                    {
                        NlogConfigDeleter deleter = new NlogConfigDeleter(_realmProvider);
                        bool done = deleter.DeleteNLogConfig(SelectedConfig.Identity);

                        if (!done)
                        {
                            _logger.Error($"Deleting of NLog configuration with ID {SelectedConfig.Identity}, went wrong!");
                            MessageBox.Show("Config deletion failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    ReadSavedConfigs();
                }
            }
        }

        #endregion

        #region Closing window by button

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing NLog configuration window pressed");

            TryClose();
        }

        #endregion

        #region User activity

        public void ResetLogoutTimer()
        {
            RuntimeLogoutTimer.UserActivityDetected();
        }

        #endregion
    }
}
