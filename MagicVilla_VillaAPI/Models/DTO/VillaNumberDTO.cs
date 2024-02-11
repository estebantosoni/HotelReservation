using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_VillaAPI.Models.DTO
{
    public class VillaNumberDTO
    {
        [Required]
        public int VillaNro { get; set; }

        [Required]
        public int VillaId { get; set; }

        public string SpecialDetails { get; set; }
    
        //I need this property only for see the villa name
        //previously i need add this property like a navigation property (includeProperties)
        //then (or before the previous requirement), i need a foreign key relation between Villa and VillaNumber
        public VillaDTO Villa { get; set; }
    }
}
