using RealmDBHandler.CommonClasses;
using Realms;

namespace RealmDBHandler.RealmObjects
{
    public class SMSrecipientDefinition : RealmObject, IIdentityPrimaryKeyInterface
    {
        [PrimaryKey]
        public int Identity { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public int AreaCode { get; set; }
        public long PhoneNumber { get; set; }
    }
}
