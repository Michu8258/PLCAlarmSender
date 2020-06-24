using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using Realms;
using System;

namespace RealmDBHandler.SystemEventsHandler
{
    public class SystemEventCreator
    {
        #region Constructor

        //nlog instance (logger)
        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        //constructor
        public SystemEventCreator(IRealmProvider realmPRovider)
        {
            _realmProvider = realmPRovider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"System event creator object created");
        }

        #endregion

        #region Public methods

        public void SaveNewEvent(SystemEventTypeEnum eventType, string eventText)
        {
            _logger.Info($"Method for saving new system event to DB fired. Event type: {eventType.ToString()}.");
            AddNewEventToDB(eventType, eventText);
        }

        #endregion

        #region Internal methods

        //method for providing new ID number
        private int GetNewIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new SystemEventDefinition());
        }

        //adding new event
        private void AddNewEventToDB(SystemEventTypeEnum eventType, string eventText)
        {
            try
            {
                int newID = GetNewIdentity();

                _realm.Write(() =>
                {
                    _realm.Add(new SystemEventDefinition()
                    {
                        Identity = newID,
                        DateTime = TimeStampConverter.ConvertToInteger(DateTime.Now),
                        Text = eventText,
                        EventType = SystemEventTypeConverter.ConvertEnumToInteger(eventType),
                    });
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to save new system event to DB. EventType: {eventType.ToString()}, text: {eventText}. Error: {ex.Message}.");
            }
        }

        #endregion
    }
}
