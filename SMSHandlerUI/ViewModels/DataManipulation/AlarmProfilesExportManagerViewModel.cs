using Caliburn.Micro;
using NLog;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.IOfilesHandling;
using SMSHandlerUI.Models;
using SMSHandlerUI.ProgressWindowEnum;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SMSHandlerUI.ViewModels.DataManipulation
{
    class AlarmProfilesExportManagerViewModel : Screen
    {
        #region Fields and properties

        private readonly IWindowManager _manager;
        private readonly Logger _logger;
        private readonly IRealmProvider _realmProvider;

        //filepat to the export file be saved
        private string _filePath;

        //list of profiles
        private List<AlarmProfileDefinition> _originalProfilesList;

        //bindable list
        private BindableCollection<AlarmProfileExportModel> _availableProfiles;
        private AlarmProfileExportModel _selectedProfile;
        private readonly List<int> _profilesToExportIDlist;

        public BindableCollection<AlarmProfileExportModel> AvailableProfiles { get { return _availableProfiles; } set { _availableProfiles = value; NotifyOfPropertyChange(); } }
        public AlarmProfileExportModel SelectedProfile { get { return _selectedProfile; } set { _selectedProfile = value; NotifyOfPropertyChange(() => SelectedProfile); } }

        //export selected button enabling
        private bool _enableExportButton;
        public bool EnableExportButton { get { return _enableExportButton; } set { _enableExportButton = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Constructor

        public AlarmProfilesExportManagerViewModel(IWindowManager manager, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;

            _originalProfilesList = new List<AlarmProfileDefinition>();
            AvailableProfiles = new BindableCollection<AlarmProfileExportModel>();
            _profilesToExportIDlist = new List<int>();
            EnableExportButton = false;

            //default filepath
            _filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Alarms profiles export";

            ReadAllProfiles();

            _logger.Info($"Alarm urgency profiles export manager window created.");
        }

        #endregion

        #region Reading all profiles from Database

        private void ReadAllProfiles()
        {
            _logger.Info($"Reading list of all alarm profiles from DB.");

            ReadRawProfiles();
            if (_originalProfilesList.Count > 0)
            {
                PopulateProfilesModelCollection();
                if (AvailableProfiles.Count > 0)
                {
                    EnableExportButton = true;
                }
            }
        }

        private void ReadRawProfiles()
        {
            //create reader instance (reader of alarm profiles)
            AlarmProfileReader reader = new AlarmProfileReader(_realmProvider);

            //get full list of currently defined alarm urgency profiles
            _originalProfilesList = reader.GetListOfAllProfiles();
        }

        private void PopulateProfilesModelCollection()
        {
            //clear list (bindable collection)
            AvailableProfiles.Clear();

            //for every read raw profile definition
            foreach (var item in _originalProfilesList)
            {
                AlarmProfileExportModel model = new AlarmProfileExportModel()
                {
                    ToExport = false,
                    Identity = item.Identity,
                    CreatedBy = item.CreatedBy,
                    ModifiedBy = item.ModifiedBy,
                    ProfileName = item.ProfileName,
                    ProfileComment = item.ProfileComment,
                };

                AvailableProfiles.Add(model);
            }
        }

        #endregion

        #region Exporting profiles

        public void ExportSelectedProfiles()
        {
            _logger.Info($"Button for exporting all selected profiles pressed.");

            //checking if at leas one of all profiles is marked as profile to export
            if (CheckIfAtLeastOneProfileIsSelected())
            {
                //open dialog file - location of file to save
                ShowExportFileLocationDialog();
                if (_filePath != null && _filePath.Substring(_filePath.Count() - 5, 5) == ".json")
                {
                    //create list of profiles ID to export
                    CreateListOfProfilesIDtoExport();

                    //exporting
                    Export();
                }
            }
            else
            {
                MessageBox.Show($"You need to choose at least one alarm profile before export.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CheckIfAtLeastOneProfileIsSelected()
        {
            return AvailableProfiles.Where(x => x.ToExport == true).Count() > 0;
        }

        private void ShowExportFileLocationDialog()
        {
            CreateJSONFile fileCreator = new CreateJSONFile();
            fileCreator.SaveFileDialg(ref _filePath);
        }

        private void CreateListOfProfilesIDtoExport()
        {
            _profilesToExportIDlist.Clear();

            foreach (var item in AvailableProfiles)
            {
                if (item.ToExport)
                {
                    _profilesToExportIDlist.Add(item.Identity);
                }
            }
        }

        private void Export()
        {
            _logger.Info($"Export of profiles starting.");

            ProgressBarViewModel pbvm = new ProgressBarViewModel(ProgressWindowTypeEnum.ProfilesExport,
                        _filePath, _profilesToExportIDlist, 0, _realmProvider);
            _manager.ShowDialog(pbvm);
        }

        #endregion

        #region Closing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing the window pressed. Closing the window.");

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
