using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IPaymentLogStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetPaymentLogAsync(string userEmail, string transactionId);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetBusinessPaymentTransactionAsync(string businessId, string userEmail, string transactionId);

        Task<AxCrypt.Abstractions.Rest.RestResponse> ListAsync(string userEmail);

        Task<AxCrypt.Abstractions.Rest.RestResponse> ListBusinessPaymentsAsync(string businessId);

        Task<AxCrypt.Abstractions.Rest.RestResponse> ListLatestBusPaymentsAsync(string businessId, string userEmail);

        Task<bool> UpdatePaymentLogAsync(string transactionId, AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> SavePaymentLogAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> CreatePaymentLogAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> CopyPaymentLogAsync(string fromEmail, string toEmail);

        Task<bool> MovePaymentLogAsync(string fromEmail, string toEmail);

        Task<bool> DeletePaymentLogAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> IsPaymentTransactionExistsAsync(string transactionId, string buyerEmail);
    }
}