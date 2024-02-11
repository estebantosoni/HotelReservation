using MagicVilla_VillaMVC.Models.DTO;

namespace MagicVilla_VillaMVC.Services.IServices
{
    public interface IVillaService
    {
        Task<T> GetAllAsync<T>();
        Task<T> GetAsync<T>(int id);

        Task<T> CreateAsync<T>(VillaCreateDTO villaCreateDTO);

        Task<T> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO);

        Task<T> DeleteAsync<T>(int id);


    }
}
