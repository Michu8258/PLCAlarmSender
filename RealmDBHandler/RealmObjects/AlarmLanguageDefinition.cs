using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class AlarmLanguageDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public string LanguageName { get; set; }
        public string LanguageText { get; set; }
        public bool LanguageEnabled { get; set; }
        public bool LanguageSelected { get; set; }
    }
}
