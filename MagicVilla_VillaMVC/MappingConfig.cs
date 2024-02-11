using AutoMapper;
using MagicVilla_VillaMVC.Models;
using MagicVilla_VillaMVC.Models.DTO;

namespace MagicVilla_VillaMVC
{
    public class MappingConfig : Profile
    {
        public MappingConfig() {

            CreateMap<VillaDTO, VillaCreateDTO>().ReverseMap();
            CreateMap<VillaDTO, VillaUpdateDTO>().ReverseMap();

            CreateMap<VillaNumberDTO, VillaNumberCreateDTO>().ReverseMap();
            CreateMap<VillaNumberDTO, VillaNumberUpdateDTO>().ReverseMap();

        }
    }
}
