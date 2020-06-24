using RealmDBHandler.EnumsAndConverters;

namespace RealmDBHandler.UserManagement
{
    public class LoggedUserData
    {
        public int Identity { get; set; }
        public string UserName { get; set; }
        public string AccessLevel { get; set; }
        public AccessLevelEnum AccessLevelEnum { get; set; }
        public bool LogoutEnabled { get; set; }
        public int LogoutTime { get; set; }
        public int LangEditPrevilages { get; set; }
    }
}
