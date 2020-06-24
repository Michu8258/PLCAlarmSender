using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class AlarmLanguagesDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public int AlarmIdentity { get; set; } //Foreign key
        public int PLCconnectionID { get; set; } //Foregin key
        public string SysLang1 { get; set; }
        public string SysLang2 { get; set; }
        public string SysLang3 { get; set; }
        public string SysLang4 { get; set; }
        public string SysLang5 { get; set; }
        public string SysLang6 { get; set; }
        public string SysLang7 { get; set; }
        public string UserLang1 { get; set; }
        public string UserLang2 { get; set; }
        public string UserLang3 { get; set; }
        public string UserLang4 { get; set; }
        public string UserLang5 { get; set; }
        public string UserLang6 { get; set; }
        public string UserLang7 { get; set; }
        public string UserLang8 { get; set; }
        public string UserLang9 { get; set; }
    }
}
