using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    class SystemEventDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public long DateTime { get; set; }
        public string Text { get; set; }
        public int EventType { get; set; }
    }
}
