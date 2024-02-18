using MagicVilla_VillaMVC.Models;
using MagicVilla_VillaMVC.Models.DTO;
using MagicVilla_VillaMVC.Services.IServices;
using Utility;

namespace MagicVilla_VillaMVC.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory httpClient;
        public string villaUrl;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory)
        {
            httpClient = httpClientFactory;
            this.villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        public Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + "/api/v1/UsersAuth/login"
            });
        }

        public Task<T> RegisterAsync<T>(RegisterRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + "/api/v1/UsersAuth/register"
            });
        }
    }
}
