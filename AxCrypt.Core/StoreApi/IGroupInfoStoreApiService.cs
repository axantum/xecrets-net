using AxCrypt.Api.Model.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IGroupInfoStoreApiService
    {
        Task<IEnumerable<GroupKeyPairApiModel>>GetMembershipGroupsInfoAsync(string userEmail);
    }
}
