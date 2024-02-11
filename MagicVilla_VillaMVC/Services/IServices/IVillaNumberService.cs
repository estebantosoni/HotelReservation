using MagicVilla_VillaMVC.Models.DTO;

namespace MagicVilla_VillaMVC.Services.IServices
{
    public interface IVillaNumberService
    {
        Task<T> GetAllAsync<T>();
        Task<T> GetAsync<T>(int id);

        Task<T> CreateAsync<T>(VillaNumberCreateDTO villaNumberCreateDTO);

        Task<T> UpdateAsync<T>(VillaNumberUpdateDTO villaNumberUpdateDTO);

        Task<T> DeleteAsync<T>(int id);


    }
}
