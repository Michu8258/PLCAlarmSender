namespace RealmDBHandler.EnumsAndConverters
{
    class AccesLevelConverter
    {
        public int GetAccessLevelInt(AccessLevelEnum level)
        {
            switch (level)
            {
                case AccessLevelEnum.None: return 0;
                case AccessLevelEnum.User: return 10;
                case AccessLevelEnum.Operator: return 20;
                case AccessLevelEnum.Administrator:return 30;
                default: return 0;
            }
        }

        public AccessLevelEnum GetAccesLevelEnum(int level)
        {
            if (level >= 30) return AccessLevelEnum.Administrator;
            else if (level >= 20) return AccessLevelEnum.Operator;
            else if (level >= 10) return AccessLevelEnum.User;
            else return AccessLevelEnum.None;
        }
    }
}
