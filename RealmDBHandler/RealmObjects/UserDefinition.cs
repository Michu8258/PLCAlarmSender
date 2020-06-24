using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class UserDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int AccessLevel { get; set; }
        public byte[] Salt { get; set; }
        public bool LogoutEnabled { get; set; }
        public int LogoutTime { get; set; }
        public int LanguageEditorPrevilages { get; set; }
    }
}
