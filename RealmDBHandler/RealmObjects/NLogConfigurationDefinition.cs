using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class NLogConfigurationDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool OldLogDeletion { get; set; }
        public int OldLogDeletionDays { get; set; }
        public int HoursToCreateNewLogFile { get; set; }
        public bool ConfigActivated { get; set; }
    }
}
