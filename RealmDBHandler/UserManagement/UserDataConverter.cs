using RealmDBHandler.EnumsAndConverters;
using RealmDBHandler.RealmObjects;
using System.Collections.Generic;

namespace RealmDBHandler.UserManagement
{
    internal class UserDataConverter
    {
        public List<UserManagementListViewModel> ToViewModelConverter(List<UserDefinition> userDefinition)
        {
            List<UserManagementListViewModel> outputList = new List<UserManagementListViewModel>();

            foreach (var item in userDefinition)
            {
                AccesLevelConverter converter = new AccesLevelConverter();

                UserManagementListViewModel newData = new UserManagementListViewModel()
                {
                    UserID = item.Identity,
                    UserName = item.UserName,
                    AccessLevel = converter.GetAccesLevelEnum(item.AccessLevel),
                    AccessLevelString = converter.GetAccesLevelEnum(item.AccessLevel).ToString(),
                    LogoutEnabled = item.LogoutEnabled,
                    LogoutTime = item.LogoutTime,
                    LanguageEditionCode = item.LanguageEditorPrevilages,
                };

                outputList.Add(newData);
            }

            return outputList;
        }

    }
}
