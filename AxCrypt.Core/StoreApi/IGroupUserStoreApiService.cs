using AxCrypt.Api.Model.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IGroupUserStoreApiService
    {
        Task<BusinessGroupListApiModel> GetGroupUserListAsync(string busSubsId, string user);

        Task<bool> AddMembersAsync(long group, string adminEmail, GroupApiModel groupApiModel);

        Task<bool> RemoveMembersAsync(long group, string adminEmail, string removeUser);

        Task<bool> UpdateGroupMemberRoleAsync(long group, string adminEmail, GroupMemberApiModel groupMemberApiModel);

        Task<bool> IsAdminInAnyGroupsAsync(string userEmail);

        Task<bool> IsGroupAdminAsync(long groupId, string userEmail);

        Task<bool> DisableGroupMemberAccessAsync(long groupid, string adminEmail, IEnumerable<GroupMemberApiModel> groupMembers);
    }
}