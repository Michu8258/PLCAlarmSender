using Caliburn.Micro;
using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.SystemEventsHandler;
using SMSHandlerUI.EventMessages;
using SMSHandlerUI.RuntimeData;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SMSHandlerUI.ViewModels
{
    class SystemEventsExplorerViewModel : Screen, IHandle<SystemEventsFiltersEventMessage>
    {
        #region Fields and properties

        //logger
        private readonly Logger _logger;

        //window manager
        private readonly IWindowManager _manager;

        //realm provider
        private readonly IRealmProvider _realmProvider;

        //event aggregattor
        private readonly IEventAggregator _eventAggregator;

        //list ciew with events
        private BindableCollection<SystemEventGUImodel> _eventsList;
        private SystemEventGUImodel _selectedEvent;

        public BindableCollection<SystemEventGUImodel> EventsList { get { return _eventsList; } set { _eventsList = value; NotifyOfPropertyChange(); } }
        public SystemEventGUImodel SelectedEvent { get { return _selectedEvent; } set { _selectedEvent = value; NotifyOfPropertyChange(() => SelectedEvent); } }

        //for filtering
        private DateTime _startDate;
        private DateTime _endDate;
        private List<SystemEventTypeEnum> _permittedEntries;

        #endregion

        #region Constructor

        public SystemEventsExplorerViewModel(IWindowManager manager, IEventAggregator eventAggregator, IRealmProvider realmPRovider)
        {
            _realmProvider = realmPRovider;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _manager = manager;
            _eventAggregator = eventAggregator;
            if (_eventAggregator != null) _eventAggregator.Subscribe(this);

            //initialize EventList
            EventsList = new BindableCollection<SystemEventGUImodel>();

            DateTime tempDate = DateTime.Now;

            //clear filters at startup
            _startDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, 0, 0, 0, DateTimeKind.Local);
            _endDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, 23, 59, 59, DateTimeKind.Local);
            _permittedEntries = new List<SystemEventTypeEnum>();

            _logger.Info($"System explorer windoe created.");
        }

        #endregion

        #region Reading all events

        public void GetAllEvents()
        {
            _logger.Info($"Button for reading all events from DB pressed.");

            MessageBoxResult msgRes = MessageBox.Show("Are you sure you want to read all system events from DB? It may take a few minutes.",
                "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (msgRes == MessageBoxResult.OK)
            {
                _logger.Info($"User confirmed that he wants to read all events from DB.");
                ReadAllevents();
            }
        }

        private void ReadAllevents()
        {
            _logger.Info($"Reading all events from DB started.");

            //create reader instance
            SystemEventReader reader = new SystemEventReader(_realmProvider);

            //read values
            List<SystemEventGUImodel> models = reader.GetAllEvents();

            //populate list view 
            PopulateListViewWithEvents(models);
        }

        #endregion

        #region Filtering events from DB

        public void DefineFilters()
        {
            _logger.Info($"Button for opening System events filtering window pressed. Opening window.");

            SystemEventExplorerFilterViewModel seefvm = new SystemEventExplorerFilterViewModel(_startDate, _endDate, _permittedEntries, _eventAggregator);
            _manager.ShowDialog(seefvm);
        }

        //Catching event from filter window with filters update
        public void Handle(SystemEventsFiltersEventMessage message)
        {
            _startDate = message.StartDate;
            _endDate = message.EndDate;
            _permittedEntries = message.EntriesList;

            ReadFilteredEvents();
        }

        private void ReadFilteredEvents()
        {
            _logger.Info($"Reading filtered events from DB started.");

            //create reader instance
            SystemEventReader reader = new SystemEventReader(_realmProvider);

            //read values
            List<SystemEventGUImodel> models = reader.GetFilteredEvents(_startDate, _endDate, _permittedEntries);

            //populate list view 
            PopulateListViewWithEvents(models);
        }

        #endregion

        #region Converting List of GUImodels to BindableCollection

        private void PopulateListViewWithEvents(List<SystemEventGUImodel> modelsList)
        {
            EventsList.Clear();

            foreach (var item in modelsList)
            {
                EventsList.Add(item);
            }
        }

        #endregion

        #region Closing window

        public void CloseTheWindow()
        {
            _logger.Info($"Button for closing system events explorer window precced.");

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
