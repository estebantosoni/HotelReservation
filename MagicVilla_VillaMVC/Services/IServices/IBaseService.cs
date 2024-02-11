using MagicVilla_VillaMVC.Models;

namespace MagicVilla_VillaMVC.Services.IServices
{
    public interface IBaseService
    {
        //API Response
        APIResponse APIResponse { get; set; }

        //Web Project uses API Request because API receives an API Request from client
        Task<T> SendAsync<T>(APIRequest request);
    }
}
