using NLog;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmDBHandler.SystemEventsHandler
{
    public class SystemEventReader
    {
        #region Constructor

        private readonly Logger _logger;
        private readonly Realm _realm;
        private readonly IRealmProvider _realmProvider;

        public SystemEventReader(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _realm = _realmProvider.GetRealmDBInstance();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info($"System events reader object created.");
        }

        #endregion

        #region Public methods

        public List<SystemEventGUImodel> GetLast100Events()
        {
            _logger.Info($"Method for reading last 100 system events fired.");

            return GetLast100();
        }

        public List<SystemEventGUImodel> GetAllEvents()
        {
            _logger.Info($"Method for reading all saved system events fired.");

            return GetALlEvents();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate">timestamp == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local) in case of no filter</param>
        /// <param name="endTime">timestamp == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local) in case of no filter</param>
        /// <param name="entriesList">List of entries that need to be showed - if null or ampty list - all entries are permitted</param>
        /// <returns></returns>
        public List<SystemEventGUImodel> GetFilteredEvents(DateTime startDate, DateTime endTime, List<SystemEventTypeEnum> entriesList)
        {
            _logger.Info($"Method for reading filtered system events started.");
            return ObtainFilteredEvents(startDate, endTime, entriesList);
        }

        #endregion

        #region Private methods

        private List<SystemEventGUImodel> GetLast100()
        {
            try
            {
                int newestIdentity = GetLastEventIdentity();
                int identityFilter = GetIdentityOfFirstOf100elements(newestIdentity);

                List<SystemEventDefinition> rawData = _realm.All<SystemEventDefinition>().Where(x => x.Identity > identityFilter).ToList();
                rawData.Reverse();
                return ConwertRawListToGUIlist(rawData);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to obtain first 100 system events. Exception: {ex.Message}.");
                return new List<SystemEventGUImodel>();
            }
        }

        private List<SystemEventGUImodel> GetALlEvents()
        {
            try
            {
                List<SystemEventDefinition> rawData = _realm.All<SystemEventDefinition>().ToList();
                rawData.Reverse();
                return ConwertRawListToGUIlist(rawData);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while reading all events from DB. Exception: {ex.Message}.");
                return new List<SystemEventGUImodel>();
            }
        }

        private List<SystemEventGUImodel> ConwertRawListToGUIlist(List<SystemEventDefinition> models)
        {
            List<SystemEventGUImodel> outputList = new List<SystemEventGUImodel>();

            foreach (var item in models)
            {
                outputList.Add(ConvertRawModelToGUImodel(item));
            }

            return outputList;
        }

        private SystemEventGUImodel ConvertRawModelToGUImodel(SystemEventDefinition model)
        {
            SystemEventGUImodel guiModel = new SystemEventGUImodel()
            {
                Identity = model.Identity,
                Timestamp = TimeStampConverter.UnixTimeStampToDateTime(model.DateTime),
                Text = model.Text,
                Entry = SystemEventTypeConverter.ConvertIntegerToEnum(model.EventType).ToString(),
            };

            return guiModel;
        }

        private int GetLastEventIdentity()
        {
            NewIdentityProvider provider = new NewIdentityProvider(_realmProvider);
            return provider.ProvideNewIdentity(new SystemEventDefinition()) - 1;
        }

        private int GetIdentityOfFirstOf100elements(int lastIdentity)
        {
            if (lastIdentity <= 100) return 0;
            else return lastIdentity - 100;
        }

        #endregion

        #region Reading filtered events

        private List<SystemEventGUImodel> ObtainFilteredEvents(DateTime startDate, DateTime endTime, List<SystemEventTypeEnum> entriesList)
        {
            List<int> entriesInt = ConvertEntriesListToIntList(entriesList);
            long olderDate = ConvertDateToUnix(startDate, true);
            long youngerDate = ConvertDateToUnix(endTime, false);

            try
            {
                List<SystemEventDefinition> rawData = _realm.All<SystemEventDefinition>().
                Where(x => x.DateTime >= olderDate && x.DateTime <= youngerDate).ToList();

                //lame entry type filter because of Realm DB lameness
                if (entriesInt.Count > 0)
                {
                    FilterEntriesTypes(entriesInt, ref rawData);
                }

                rawData.Reverse();

                return ConwertRawListToGUIlist(rawData);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to read Filtered system events. Exception: {ex.Message}.");
                return new List<SystemEventGUImodel>();
            }

        }

        private void FilterEntriesTypes(List<int> entriesInt, ref List<SystemEventDefinition> rawData)
        {
            List<SystemEventDefinition> itemsToRemove = new List<SystemEventDefinition>();

            //collect items to remove
            foreach (var item in rawData)
            {
                if (!entriesInt.Contains(item.EventType))
                {
                    itemsToRemove.Add(item);
                }
            }

            //remove filters
            foreach (var item in itemsToRemove)
            {
                rawData.Remove(item);
            }
        }

        private List<int> ConvertEntriesListToIntList(List<SystemEventTypeEnum> entriesList)
        {
            List<int> entriesIntList = new List<int>();
            foreach (var item in entriesList)
            {
                entriesIntList.Add(SystemEventTypeConverter.ConvertEnumToInteger(item));
            }

            return entriesIntList;
        }

        /// <summary>
        ///  Converts date time to unix
        /// </summary>
        /// <param name="timestamp">Date</param>
        /// <param name="startOrEnd">true - startDate / false - endDate</param>
        /// <returns></returns>
        private long ConvertDateToUnix(DateTime timestamp, bool startOrEnd)
        {
            if (startOrEnd) //start date
            {
                if (timestamp == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local)) //no filter
                {
                    return 0;
                }
                else //filter set
                {
                    return TimeStampConverter.ConvertToInteger(timestamp);
                }
            }
            else //end date
            {
                if (timestamp == new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local)) //no filter
                {
                    return TimeStampConverter.ConvertToInteger(DateTime.Now.AddYears(100));
                }
                else //filter set
                {
                    return TimeStampConverter.ConvertToInteger(timestamp);
                }
            }
        }

        #endregion
    }
}
