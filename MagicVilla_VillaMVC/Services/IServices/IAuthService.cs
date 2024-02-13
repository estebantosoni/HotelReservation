using MagicVilla_VillaMVC.Models.DTO;

namespace MagicVilla_VillaMVC.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO loginRequestDTO);
        Task<T> RegisterAsync<T>(RegisterRequestDTO registerRequestDTO);
    }
}
