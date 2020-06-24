using Caliburn.Micro;
using RealmDBHandler.AlarmUrgencyProfiler;
using RealmDBHandler.CommonClasses;
using RealmDBHandler.RealmObjects;
using RealmDBHandler.SMSrecipientsGroupHandling;
using SMSHandlerUI.Models;
using System.Collections.Generic;

namespace SMSHandlerUI.ViewModels.AlarmManagement
{
    public static class AlarmProfilesAndSMSgroupsReader
    {
        public static BindableCollection<AlarmProfileComboBoxModel> GetListOfAlarmProfileModels(IRealmProvider realmProvider)
        {
            //outputList
            BindableCollection<AlarmProfileComboBoxModel> output = new BindableCollection<AlarmProfileComboBoxModel>();

            //read data
            AlarmProfileReader reader = new AlarmProfileReader(realmProvider);
            List<AlarmProfileDefinition> profilesList = reader.GetListOfAllProfiles();

            //assign new items to comboBox
            foreach (var item in profilesList)
            {
                output.Add(new AlarmProfileComboBoxModel()
                {
                    Identity = item.Identity,
                    Name = item.ProfileName,
                });
            }

            return output;
        }

        public static BindableCollection<SMSgroupsComboBoxModel> GetListOfSMSrecipientsModels(IRealmProvider realmProvider)
        {
            //output list
            BindableCollection<SMSgroupsComboBoxModel> output = new BindableCollection<SMSgroupsComboBoxModel>();

            //read data
            SMSrecipientsGroupReader reader = new SMSrecipientsGroupReader(realmProvider);
            Dictionary<int, string> groups = reader.GetAllGroupsNamesOnlyWithAtLeastOneRecipient();

            //assign new items to comboBox
            foreach (var item in groups)
            {
                output.Add(new SMSgroupsComboBoxModel()
                {
                    Identity = item.Key,
                    Name = item.Value,
                });
            }

            return output;
        }
    }
}
