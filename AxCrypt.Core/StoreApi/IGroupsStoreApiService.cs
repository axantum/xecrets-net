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

        Task<bool> AddMembersAsync(long group, GroupApiModel createGroupApiModel);

        Task<BusinessGroupListApiModel> GetManageListAsync(string busSubsId, string user);

        Task<BusinessGroupListApiModel> GetViewerListAsync(string busSubsId, string user);

        Task<GroupApiModel> GetAsync(long groupId, string user);

        Task<bool> UpdateGroupInfoAsync(GroupApiModel updateGroupApiModel);

        Task<bool> DeleteGroupAsync(long group, string userEmail);
    }
}