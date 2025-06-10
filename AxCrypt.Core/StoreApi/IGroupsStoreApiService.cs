using AxCrypt.Api.Model.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IGroupsStoreApiService
    {
        Task<bool> CreateAsync(GroupApiModel createGroupApiModel);

        Task<BusinessGroupListApiModel> GetManageListAsync(string busSubsId, string user);

        Task<BusinessGroupListApiModel> GetMembershipGroupsAsync(string busSubsId, string user);

        Task<GroupApiModel> GetAsync(long groupId, string user);

        Task<GroupApiModel> GetWithMembersAsync(long groupId, string user);

        Task<bool> UpdateGroupInfoAsync(GroupApiModel updateGroupApiModel);

        Task<bool> DeleteGroupAsync(long group, string userEmail);

        Task<bool> UpdateGroupMasterKeyStatusAsync(GroupApiModel updateMemberGroupApiModel);

        Task<GroupKeyPairApiModel> MigrateGroupKeysAsync(GroupApiModel createGroupApiModel, string userEmail);
    }
}