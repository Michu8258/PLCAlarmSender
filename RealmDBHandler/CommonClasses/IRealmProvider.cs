using Realms;
using static RealmDBHandler.CommonClasses.RealmDBLocator;

namespace RealmDBHandler.CommonClasses
{
    public interface IRealmProvider
    {
        bool CheckIfDBisAccessible();
        Realm GetRealmDBInstance();
        event RealmDbUncreatableEventHandler CouldNotCreateDB;
        event RealmDBReadingErrorEventHandler RealmDBfileAlreadyOpened;
    }
}