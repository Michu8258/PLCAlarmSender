using RealmDBHandler.CommonClasses;
using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.SystemEventsHandler;
using SMSsender.DataBaseHandling;
using SMSsender.Models;
using SMSsender.Modlels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMSsender.SMSQueue
{
    internal class SendingTask
    {
        #region Fields and properties

        private readonly List<SMSsendingModel> _messagesToSend;
        private readonly List<SMSsendModel> _sendingDetails;
        private readonly IRealmProvider _realmProvider;

        #endregion

        #region Constructor

        public SendingTask(List<SMSsendingModel> messagesToSend, IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;
            _messagesToSend = messagesToSend;
            _sendingDetails = new List<SMSsendModel>();
        }

        #endregion

        #region StartSendingProcedure

        public void StartSending()
        {
            //check if DB file is ok
            if (_realmProvider.CheckIfDBisAccessible())
            {
                //gather Messages major data
                GetMessagesToBeSent();

                //if there is any message to be send
                if (_messagesToSend.Count > 0)
                {
                    //collect sending permissions
                    CheckSendPermissions();

                    //collect recipients
                    GetRecipientsNumbers();

                    //send SMS
                    SendSMSes();
                }
            }

            //sending done Event
            OnSendingFinished();
        }

        #endregion

        #region Internal methods - completing alarms sending details

        private void GetMessagesToBeSent()
        {
            foreach (var item in _messagesToSend)
            {
                _sendingDetails.Add(new SMSsendModel() { MessageText = item.AlarmText });
            }
        }

        private void CheckSendPermissions()
        {
            for (int i = 0; i < _messagesToSend.Count(); i++)
            {
                //check by new object (checker)
                CanAlarmBeSendChecker checker = new CanAlarmBeSendChecker(_messagesToSend[i].AlarmProfileID, _realmProvider);
                _sendingDetails[i].ToBeSend = checker.CheckSendingPermissions();
            }
        }

        private void GetRecipientsNumbers()
        {
            for (int i = 0; i < _messagesToSend.Count(); i++)
            {
                if (_sendingDetails[i].ToBeSend)
                {
                    RecipientsCollector collector = new RecipientsCollector(_messagesToSend[i].AlarmSMSGroupID, _realmProvider);
                    _sendingDetails[i].PhoneNumbers = collector.GetRacipientsumbers();
                }
                else
                {
                    _sendingDetails[i].PhoneNumbers = new List<string>();
                }
            }
        }

        #endregion

        #region SendingSMSesToDevice

        private void SendSMSes()
        {
            //TODO - add sending to the device
            //for now only saving an event in DB

            SystemEventCreator creator = new SystemEventCreator(_realmProvider);

            for (int i = 0; i < _messagesToSend.Count(); i++)
            {
                if (_sendingDetails[i].ToBeSend)
                {
                    foreach (var item in _sendingDetails[i].PhoneNumbers)
                    {
                        creator.SaveNewEvent(SystemEventTypeEnum.SMSsending, $"Sending new SMS. Mssage Text: {_messagesToSend[i].AlarmText}; Phone number: +{item}.");
                    }
                }
            }
        }

        #endregion

        #region Task finished event

        public delegate void SMSsendingDoneEventHandler(object sender, EventArgs e);
        public event SMSsendingDoneEventHandler SendingFinished;
        public void OnSendingFinished()
        {
            SendingFinished?.Invoke(null, new EventArgs());
        }

        #endregion
    }
}
