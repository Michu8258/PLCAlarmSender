using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using SMSsender.DataBaseHandling;
using SMSsender.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSsender.SMSQueue
{
    public static class AlarmsToSend
    {
        #region Fields and properties

        internal static Queue<SMSsendingModel> _alarmsToSned;
        private static bool _taskIsRunning;
        private static IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        static AlarmsToSend()
        {
            _alarmsToSned = new Queue<SMSsendingModel>();

            //scan timer prodices an event that dhould start sending SMS procedure
            QueueAnalyzeTimer.StartQueueScan += QueueAnalyzeTimer_StartQueueScan;
            QueueAnalyzeTimer.StartTimer(2500);
        }

        #endregion

        #region Public methods

        public static void AddNewAlarmToSend(int profileID, int GroupID,
            S7AlarmDefinition newAlarm, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;

            string text = SaveAlarmOccurencesToDB(newAlarm);

            SMSsendingModel model = new SMSsendingModel()
            {
                AlarmText = text,
                AlarmProfileID = profileID,
                AlarmSMSGroupID = GroupID,
            };

            _alarmsToSned.Enqueue(model);
        }

        #endregion

        #region Saving new alarm occurences into DB

        private static string SaveAlarmOccurencesToDB(S7AlarmDefinition newAlarm)
        {
            AlarmOccurencesEventSaver saver = new AlarmOccurencesEventSaver(newAlarm, _realmProvider);
            saver.SaveAlarmOccurences();
            return saver.GetTextOfSavedAlarm();
        }

        #endregion

        #region Scanning Queue for new alarms

        // Sanding SMS procedure start
        private static void QueueAnalyzeTimer_StartQueueScan(object sender, System.EventArgs e)
        {
            //if there is no running task
            if (!_taskIsRunning)
            {
                List<SMSsendingModel> messagesToSend = new List<SMSsendingModel>();

                //get all alarms to be send
                while (_alarmsToSned.Count > 0)
                {
                    try
                    {
                        //get single queue model
                        SMSsendingModel model = _alarmsToSned.Dequeue();

                        //add to list
                        messagesToSend.Add(model);
                    }
                    catch (Exception ex)
                    {
                        var logger = NLog.LogManager.GetCurrentClassLogger();
                        logger.Error($"Unable to dequeue the wueue of pending alarms to send. Exception: {ex.Message}.");
                    }

                }
                if (messagesToSend.Count > 0)
                {
                    //blosk starting new task is one is still running
                    _taskIsRunning = true;
                    Task.Run(() => CreateNewSendingTask(messagesToSend));
                }
            }
        }

        //create new sending task and start it
        private static void CreateNewSendingTask(List<SMSsendingModel> messagesToSend)
        {
            SendingTask sender = new SendingTask(messagesToSend, _realmProvider);
            sender.SendingFinished += Sender_SendingFinished;
            sender.StartSending();
        }

        //when one task finishes, stop blocking new tasks to start
        private static void Sender_SendingFinished(object sender, EventArgs e)
        {
            _taskIsRunning = false;
        }

        #endregion
    }
}
