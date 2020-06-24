using Caliburn.Micro;
using NLog;
using RealmDBHandler.EnumsAndConverters;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.Models;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMSHandlerUI.ViewModels
{
    class SystemEventExplorerFilterViewModel : Screen
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //event aggregator
        private readonly IEventAggregator _eventAggregator;

        private DateTime _startDate;
        private DateTime _endDate;
        private List<SystemEventTypeEnum> _entriesList;
        private List<SystemEventTypeEnum> _allEntriesList;

        public DateTime StartDate { get { return _startDate; } set { _startDate = value; NotifyOfPropertyChange(); } }
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; NotifyOfPropertyChange(); } }

        private BindableCollection<SystemEventsEtriesFilterModel> _entriesFilterList;
        private SystemEventsEtriesFilterModel _selectedEntry;

        public BindableCollection<SystemEventsEtriesFilterModel> EntriesFilterList { get { return _entriesFilterList; } set { _entriesFilterList = value; NotifyOfPropertyChange(); } }
        public SystemEventsEtriesFilterModel SelectedEntry { get { return _selectedEntry; } set { _selectedEntry = value; NotifyOfPropertyChange(() => SelectedEntry); } }

        #endregion

        #region Constructor

        public SystemEventExplorerFilterViewModel(DateTime startDate,
            DateTime endDate, List<SystemEventTypeEnum> entriesList,
            IEventAggregator eventAggregator)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _eventAggregator = eventAggregator;

            _startDate = startDate;
            _endDate = endDate;
            if (entriesList == null) _entriesList = new List<SystemEventTypeEnum>();
            else _entriesList = entriesList;
            EntriesFilterList = new BindableCollection<SystemEventsEtriesFilterModel>();

            CreateListWithAllEntriesTYpes();
            PopulateEntriesListView();

            _logger.Info($"Window for defining filters in system events explorer window created.");
        }

        #endregion

        #region Constructing list with all entries

        private void CreateListWithAllEntriesTYpes()
        {
            _allEntriesList = new List<SystemEventTypeEnum>();

            foreach (SystemEventTypeEnum item in (SystemEventTypeEnum[])Enum.GetValues(typeof(SystemEventTypeEnum)))
            {
                if (item != SystemEventTypeEnum.None)
                {
                    _allEntriesList.Add(item);
                }
            }
        }

        #endregion

        #region Populating list view with entries

        private void PopulateEntriesListView()
        {
            //create bndable collection
            foreach (var item in _allEntriesList)
            {
                EntriesFilterList.Add(new SystemEventsEtriesFilterModel() { EntryType = item });
            }

            //set permitted entries
            foreach (var item in _entriesList)
            {
                EntriesFilterList.Where(x => x.EntryType == item).First().Selected = true;
            }
        }

        #endregion

        #region Applying defined filters

        public void ApplyFilters()
        {
            _logger.Info($"Button for applying filters pressed.");

            SystemEventsFiltersEventMessage message = new SystemEventsFiltersEventMessage()
            {
                StartDate = StartDate,
                EndDate = EndDate,
                EntriesList = GetSelectedEntries(),
            };

            _eventAggregator.BeginPublishOnUIThread(message);

            TryClose();
        }

        private List<SystemEventTypeEnum> GetSelectedEntries()
        {
            List<SystemEventTypeEnum> outputList = new List<SystemEventTypeEnum>();
            foreach (var item in EntriesFilterList)
            {
                if (item.Selected)
                {
                    outputList.Add(item.EntryType);
                }
            }
            return outputList;
        }

        #endregion

        #region Closing the windoe

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing filter windoe of system events explorer pressed.");

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
