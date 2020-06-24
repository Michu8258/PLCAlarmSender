using Caliburn.Micro;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.RealmObjects;
using SMSHandlerUI.RuntimeData;
using System.Collections.Generic;
using System.Windows;
using NLog;
using RealmDBHandler.CommonClasses;

namespace SMSHandlerUI.ViewModels
{
    class AlarmUrgencyProfilerViewModel : Screen
    {
        #region Monday fields and properties

        private AlarmProfilerDayDefinition _monday;

        public int MondayDayNumber { get { return _monday.DayNumber; } set { _monday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool MondayAlwaysSend { get { return _monday.AlwaysSend; } set { _monday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool MondayNeverSend { get { return _monday.NeverSend; } set { _monday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool MondaySendBetween { get { return _monday.SendBetween; } set { _monday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int MondayLowerHour { get { return _monday.LowerHour; } set { _monday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int MondayUpperHour { get { return _monday.UpperHour; } set { _monday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Tuesday fields and properties

        private AlarmProfilerDayDefinition _tuesday;

        public int TuesdayDayNumber { get { return _tuesday.DayNumber; } set { _tuesday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool TuesdayAlwaysSend { get { return _tuesday.AlwaysSend; } set { _tuesday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool TuesdayNeverSend { get { return _tuesday.NeverSend; } set { _tuesday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool TuesdaySendBetween { get { return _tuesday.SendBetween; } set { _tuesday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int TuesdayLowerHour { get { return _tuesday.LowerHour; } set { _tuesday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int TuesdayUpperHour { get { return _tuesday.UpperHour; } set { _tuesday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Wednesday fields and properties

        private AlarmProfilerDayDefinition _wednesday;

        public int WednesdayDayNumber { get { return _wednesday.DayNumber; } set { _wednesday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool WednesdayAlwaysSend { get { return _wednesday.AlwaysSend; } set { _wednesday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool WednesdayNeverSend { get { return _wednesday.NeverSend; } set { _wednesday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool WednesdaySendBetween { get { return _wednesday.SendBetween; } set { _wednesday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int WednesdayLowerHour { get { return _wednesday.LowerHour; } set { _wednesday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int WednesdayUpperHour { get { return _wednesday.UpperHour; } set { _wednesday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Thursday fields and properties

        private AlarmProfilerDayDefinition _thursday;

        public int ThursdayDayNumber { get { return _thursday.DayNumber; } set { _thursday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool ThursdayAlwaysSend { get { return _thursday.AlwaysSend; } set { _thursday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool ThursdayNeverSend { get { return _thursday.NeverSend; } set { _thursday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool ThursdaySendBetween { get { return _thursday.SendBetween; } set { _thursday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int ThursdayLowerHour { get { return _thursday.LowerHour; } set { _thursday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int ThursdayUpperHour { get { return _thursday.UpperHour; } set { _thursday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Friday fields and properties

        private AlarmProfilerDayDefinition _friday;

        public int FridayDayNumber { get { return _friday.DayNumber; } set { _friday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool FridayAlwaysSend { get { return _friday.AlwaysSend; } set { _friday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool FridayNeverSend { get { return _friday.NeverSend; } set { _friday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool FridaySendBetween { get { return _friday.SendBetween; } set { _friday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int FridayLowerHour { get { return _friday.LowerHour; } set { _friday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int FridayUpperHour { get { return _friday.UpperHour; } set { _friday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Saturday fields and properties

        private AlarmProfilerDayDefinition _saturday;

        public int SaturdayDayNumber { get { return _saturday.DayNumber; } set { _saturday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool SaturdayAlwaysSend { get { return _saturday.AlwaysSend; } set { _saturday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool SaturdayNeverSend { get { return _saturday.NeverSend; } set { _saturday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool SaturdaySendBetween { get { return _saturday.SendBetween; } set { _saturday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int SaturdayLowerHour { get { return _saturday.LowerHour; } set { _saturday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int SaturdayUpperHour { get { return _saturday.UpperHour; } set { _saturday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region Sunday fields and properties

        private AlarmProfilerDayDefinition _sunday;

        public int SundayDayNumber { get { return _sunday.DayNumber; } set { _sunday.DayNumber = value; NotifyOfPropertyChange(); } }
        public bool SundayAlwaysSend { get { return _sunday.AlwaysSend; } set { _sunday.AlwaysSend = value; NotifyOfPropertyChange(); } }
        public bool SundayNeverSend { get { return _sunday.NeverSend; } set { _sunday.NeverSend = value; NotifyOfPropertyChange(); } }
        public bool SundaySendBetween { get { return _sunday.SendBetween; } set { _sunday.SendBetween = value; NotifyOfPropertyChange(); } }
        public int SundayLowerHour { get { return _sunday.LowerHour; } set { _sunday.LowerHour = value; NotifyOfPropertyChange(); } }
        public int SundayUpperHour { get { return _sunday.UpperHour; } set { _sunday.UpperHour = value; NotifyOfPropertyChange(); } }

        #endregion

        #region TextBox properties

        private string _profileName;
        private string _comment;
        private bool _profileNameEnabled;
        private readonly Logger _logger;

        public string ProfileName { get { return _profileName; } set { _profileName = value; NotifyOfPropertyChange(); } }
        public string Comment { get { return _comment; } set { _comment = value; NotifyOfPropertyChange(); } }
        public bool ProfileNameEnabled { get { return _profileNameEnabled; } set { _profileNameEnabled = value; NotifyOfPropertyChange(); } }


        #endregion

        #region Internal data

        private readonly bool _windowMode;
        private readonly AlarmProfileDefinition _profile;
        private readonly IRealmProvider _realmProvider;
        private IRuntimeData _runtimeData;

        #endregion

        #region Constructor

        /// <summary>
        /// This Profiler has 2 modes - creating new or modifying existing one
        /// </summary>
        /// <param name="mode">FALSE = new profile / TRUE = mdify  existing profile</param>
        /// <param name="days">Pass data of existing profile here / null when creating new</param>
        /// <param name="profileName">Pass here name of existing profile, if new, pass null/param>
        /// <param name="profileComment">Pass here comment of existing profile, if new, pass null</param>
        /// <param name="profile">Pass here unmodified profile definition - only if modifying</param>
        public AlarmUrgencyProfilerViewModel(bool mode, string profileName, string profileComment, List<AlarmProfilerDayDefinition> days,
            AlarmProfileDefinition profile, IRealmProvider realmProvider, IRuntimeData runtimeData)
        {
            _realmProvider = realmProvider;
            _runtimeData = runtimeData;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _windowMode = mode;
            AssignInstancesToDays();

            if (!mode || days.Count != 7) // new profile
            {
                InitializeControlsNewProfile();
            }
            else //modification of existing profile
            {
                _profile = profile;
                InitializeControlsModifyProfile(profileName, profileComment, days);
            }

            _logger.Info($"Alarm urgency profiler window created. Window mode: {mode}.");
        }

        //assigning new instances to days
        private void AssignInstancesToDays()
        {
            _monday = new AlarmProfilerDayDefinition();
            _tuesday = new AlarmProfilerDayDefinition();
            _wednesday = new AlarmProfilerDayDefinition();
            _thursday = new AlarmProfilerDayDefinition();
            _friday = new AlarmProfilerDayDefinition();
            _saturday = new AlarmProfilerDayDefinition();
            _sunday = new AlarmProfilerDayDefinition();
        }

        //initialisation of properties of days while creating new profile
        private void InitializeControlsNewProfile()
        {
            ProfileName = "";
            Comment = "";
            ProfileNameEnabled = true;

            for (int i = 0; i < 7; i++)
            {
                AssignDayStartValues(null, i);
            }

            _logger.Info($"Initialized window data in creating new profile mode.");
        }

        //initialization of properties of days while modifying existing profile
        private void InitializeControlsModifyProfile(string profileName, string comment, List<AlarmProfilerDayDefinition> days)
        {
            ProfileName = profileName;
            Comment = comment;
            ProfileNameEnabled = false;

            for (int i = 0; i < 7; i++)
            {
                AssignDayStartValues(days[i], days[i].DayNumber);
            }

            _logger.Info($"Initialized window data in modification of existing profile mode. Profile name: {profileName}, comment: { comment}.");
        }

        //assigning values to days properties
        private void AssignDayStartValues(AlarmProfilerDayDefinition day, int dayNumber)
        {
            switch (dayNumber)
            {
                case 0:
                    if (day != null) { SundayDayNumber = 0; SundayAlwaysSend = day.AlwaysSend; SundayNeverSend = day.NeverSend; SundaySendBetween = day.SendBetween; SundayLowerHour = day.LowerHour; SundayUpperHour = day.UpperHour; break; }
                    else { SundayDayNumber = 0; SundayAlwaysSend = true; SundayNeverSend = false; SundaySendBetween = false; SundayLowerHour = 0; SundayUpperHour = 24; break; }

                case 1:
                    if (day != null) { MondayDayNumber = 1; MondayAlwaysSend = day.AlwaysSend; MondayNeverSend = day.NeverSend; MondaySendBetween = day.SendBetween; MondayLowerHour = day.LowerHour; MondayUpperHour = day.UpperHour; break; }
                    else { MondayDayNumber = 1; MondayAlwaysSend = true; MondayNeverSend = false; MondaySendBetween = false; MondayLowerHour = 0; MondayUpperHour = 24; break; }

                case 2:
                    if (day != null) { TuesdayDayNumber = 2; TuesdayAlwaysSend = day.AlwaysSend; TuesdayNeverSend = day.NeverSend; TuesdaySendBetween = day.SendBetween; TuesdayLowerHour = day.LowerHour; TuesdayUpperHour = day.UpperHour; break; }
                    else { TuesdayDayNumber = 2; TuesdayAlwaysSend = true; TuesdayNeverSend = false; TuesdaySendBetween = false; TuesdayLowerHour = 0; TuesdayUpperHour = 24; break; }

                case 3:
                    if (day != null) { WednesdayDayNumber = 3; WednesdayAlwaysSend = day.AlwaysSend; WednesdayNeverSend = day.NeverSend; WednesdaySendBetween = day.SendBetween; WednesdayLowerHour = day.LowerHour; WednesdayUpperHour = day.UpperHour; break; }
                    else { WednesdayDayNumber = 3; WednesdayAlwaysSend = true; WednesdayNeverSend = false; WednesdaySendBetween = false; WednesdayLowerHour = 0; WednesdayUpperHour = 24; break; }

                case 4:
                    if (day != null) { ThursdayDayNumber = 4; ThursdayAlwaysSend = day.AlwaysSend; ThursdayNeverSend = day.NeverSend; ThursdaySendBetween = day.SendBetween; ThursdayLowerHour = day.LowerHour; ThursdayUpperHour = day.UpperHour; break; }
                    else { ThursdayDayNumber = 4; ThursdayAlwaysSend = true; ThursdayNeverSend = false; ThursdaySendBetween = false; ThursdayLowerHour = 0; ThursdayUpperHour = 24; break; }

                case 5:
                    if (day != null) { FridayDayNumber = 5; FridayAlwaysSend = day.AlwaysSend; FridayNeverSend = day.NeverSend; FridaySendBetween = day.SendBetween; FridayLowerHour = day.LowerHour; FridayUpperHour = day.UpperHour; break; }
                    else { FridayDayNumber = 5; FridayAlwaysSend = true; FridayNeverSend = false; FridaySendBetween = false; FridayLowerHour = 0; FridayUpperHour = 24; break; }

                case 6:
                    if (day != null) { SaturdayDayNumber = 6; SaturdayAlwaysSend = day.AlwaysSend; SaturdayNeverSend = day.NeverSend; SaturdaySendBetween = day.SendBetween; SaturdayLowerHour = day.LowerHour; SaturdayUpperHour = day.UpperHour; break; }
                    else { SaturdayDayNumber = 6; SaturdayAlwaysSend = true; SaturdayNeverSend = false; SaturdaySendBetween = false; SaturdayLowerHour = 0; SaturdayUpperHour = 24; break; }
            }
        }

        #endregion

        #region Closing the window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing alarm urgency profiler window pressed.");

            TryClose();
        }

        #endregion

        #region Handling the valuse of sliders

        public void LowerValueChanged(int tag)
        {
            switch (tag)
            {
                case 0: if (SundayLowerHour >= SundayUpperHour - 1) SundayLowerHour = SundayUpperHour - 1; break;
                case 1: if (MondayLowerHour >= MondayUpperHour - 1) MondayLowerHour = MondayUpperHour - 1; break;
                case 2: if (TuesdayLowerHour >= TuesdayUpperHour - 1) TuesdayLowerHour = TuesdayUpperHour - 1; break;
                case 3: if (WednesdayLowerHour >= WednesdayUpperHour - 1) WednesdayLowerHour = WednesdayUpperHour - 1; break;
                case 4: if (ThursdayLowerHour >= ThursdayUpperHour - 1) ThursdayLowerHour = ThursdayUpperHour - 1; break;
                case 5: if (FridayLowerHour >= FridayUpperHour - 1) FridayLowerHour = FridayUpperHour - 1; break;
                case 6: if (SaturdayLowerHour >= SaturdayUpperHour - 1) SaturdayLowerHour = SaturdayUpperHour - 1; break;
                default: break;
            }
        }

        public void UpperValueChanged(int tag)
        {
            switch (tag)
            {
                case 0: if (SundayUpperHour <= SundayLowerHour + 1) SundayUpperHour = SundayLowerHour + 1; break;
                case 1: if (MondayUpperHour <= MondayLowerHour + 1) MondayUpperHour = MondayLowerHour + 1; break;
                case 2: if (TuesdayUpperHour <= TuesdayLowerHour + 1) TuesdayUpperHour = TuesdayLowerHour + 1; break;
                case 3: if (WednesdayUpperHour <= WednesdayLowerHour + 1) WednesdayUpperHour = WednesdayLowerHour + 1; break;
                case 4: if (ThursdayUpperHour <= ThursdayLowerHour + 1) ThursdayUpperHour = ThursdayLowerHour + 1; break;
                case 5: if (FridayUpperHour <= FridayLowerHour + 1) FridayUpperHour = FridayLowerHour + 1; break;
                case 6: if (SaturdayUpperHour <= SaturdayLowerHour + 1) SaturdayUpperHour = SaturdayLowerHour + 1; break;
                default: break;
            }
        }

        #endregion

        #region Saving data - button pressing

        //clicking the save button
        public void SaveButton()
        {
            _logger.Info($"Button for sawing profile pressed.");

            if (!_windowMode) //new profile
            {
                SavingNewProfileAlgorithm();
            }
            else //profile modification
            {
                ModifyProfileAlgorithm();
            }
        }

        //saving new profile procedure
        private void SavingNewProfileAlgorithm()
        {
            _logger.Info($"Saving new profile procedure started.");

            bool success = CheckIfProfileNameIsOK();
            if (success)
            {
                bool saved = SaveNewProfile();
                if (!saved)
                {
                    _logger.Error($"Saving new alarm profile went wrong!");
                    MessageBox.Show("Profile not saved properly!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    TryClose();
                }
            }
        }

        //modifying existing profile procedure
        private void ModifyProfileAlgorithm()
        {
            _logger.Info($"Saving modified alarm profile procedure started.");

            bool modified = ModifyExistingProfile();
            if (!modified)
            {
                _logger.Error($"Modifying alarm profile went wrong!");
                MessageBox.Show("Profile not updated properly!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TryClose();
            }
        }

        //method for checking if inputed profile name is ok (valid when creating new profile)
        private bool CheckIfProfileNameIsOK()
        {
            bool output = true;

            if (ProfileName.Length <= 4)
            {
                _logger.Info($"Checking name for new alarm profile failed. New name to short.");
                MessageBox.Show("The profile name is to short. It must have at least 5 characters", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ProfileName = "";
                output = false;
            }
            if (output)
            {
                AlarmProfileNameUniquenessChecker checker = new AlarmProfileNameUniquenessChecker(_realmProvider);
                bool nameOK = checker.CheckAlarmProfileName(ProfileName);
                if (!nameOK)
                {
                    _logger.Info($"Checking name for new alarm profile failed. Povided name of new alarm profile already exists in DB.");
                    MessageBox.Show("The profile name is not unique. Profile with this name already exists.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProfileName = "";
                    output = false;
                }
            }
            return output;
        }

        //gather data specified for each week day
        private List<AlarmProfilerDayDefinition> GatherAlarmProfileDaysData()
        {
            List<AlarmProfilerDayDefinition> outputList = new List<AlarmProfilerDayDefinition>
            {
                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 1,
                    AlwaysSend = MondayAlwaysSend,
                    NeverSend = MondayNeverSend,
                    SendBetween = MondaySendBetween,
                    LowerHour = MondayLowerHour,
                    UpperHour = MondayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 2,
                    AlwaysSend = TuesdayAlwaysSend,
                    NeverSend = TuesdayNeverSend,
                    SendBetween = TuesdaySendBetween,
                    LowerHour = TuesdayLowerHour,
                    UpperHour = TuesdayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 3,
                    AlwaysSend = WednesdayAlwaysSend,
                    NeverSend = WednesdayNeverSend,
                    SendBetween = WednesdaySendBetween,
                    LowerHour = WednesdayLowerHour,
                    UpperHour = WednesdayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 4,
                    AlwaysSend = ThursdayAlwaysSend,
                    NeverSend = ThursdayNeverSend,
                    SendBetween = ThursdaySendBetween,
                    LowerHour = ThursdayLowerHour,
                    UpperHour = ThursdayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 5,
                    AlwaysSend = FridayAlwaysSend,
                    NeverSend = FridayNeverSend,
                    SendBetween = FridaySendBetween,
                    LowerHour = FridayLowerHour,
                    UpperHour = FridayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 6,
                    AlwaysSend = SaturdayAlwaysSend,
                    NeverSend = SaturdayNeverSend,
                    SendBetween = SaturdaySendBetween,
                    LowerHour = SaturdayLowerHour,
                    UpperHour = SaturdayUpperHour
                },

                new AlarmProfilerDayDefinition()
                {
                    DayNumber = 0,
                    AlwaysSend = SundayAlwaysSend,
                    NeverSend = SundayNeverSend,
                    SendBetween = SundaySendBetween,
                    LowerHour = SundayLowerHour,
                    UpperHour = SundayUpperHour
                }
            };

            return outputList;
        }

        //saving new profile to DB
        private bool SaveNewProfile()
        {
            _logger.Info($"Button for saving new alarm profile pressed.");

            AlarmProfileCreator creator = new AlarmProfileCreator(_realmProvider);
            return creator.SaveNewProfile(_runtimeData.DataOfCurrentlyLoggedUser.UserName, ProfileName,
                Comment, GatherAlarmProfileDaysData());
        }

        //modifying existing profile in DB
        private bool ModifyExistingProfile()
        {
            _logger.Info($"Button for saving modified profile pressed.");

            AlarmProfileModifier modifier = new AlarmProfileModifier(_realmProvider);
            return modifier.ModifyExistingProfile(_profile.Identity, _runtimeData.DataOfCurrentlyLoggedUser.UserName,
                Comment, GatherAlarmProfileDaysData());
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
