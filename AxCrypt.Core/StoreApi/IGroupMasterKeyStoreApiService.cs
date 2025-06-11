using AxCrypt.Api.Model.Groups;
using AxCrypt.Api.Model.Masterkey;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IGroupMasterKeyStoreApiService
    {
        Task<bool> CreateAsync(GroupMasterKeyApiModel masterKeyApiModel, string adminEmail);

        Task<bool> UpdateAsync(GroupMasterKeyApiModel masterKeyApiModel, string adminEmail);

        Task<GroupMasterKeyApiModel> GetAsync(long groupid, string userEmail);

        Task<bool> ApproveMasterKeyStatusAsync(long groupid, PrivateMasterKeyInfo memberInfo);

        Task<bool> AddMemberAsync(long groupid, GroupMasterKeyApiModel masterKeyApiModel, string adminEmail);

        Task<bool> RemoveMemberAsync(long groupid, string userEmail, string adminEmail);

        Task<bool> UpdateGroupMasterKeyAccessAsync(long groupid, IEnumerable<PrivateMasterKeyInfo> memberInfos);

        Task<IEnumerable<GroupMasterKeyApiModel>> GetGroupMasterKeysAsync(string adminEmail);

        Task<bool> ChangeMasterKeyOwnerAsync(IEnumerable<GroupMasterKeyApiModel> privateMasterKeyInfos, string existingMasterKeyOwner);
    }
}