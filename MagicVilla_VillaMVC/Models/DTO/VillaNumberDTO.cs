using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_VillaMVC.Models.DTO
{
    public class VillaNumberDTO
    {
        [Required]
        public int VillaNro { get; set; }

        [Required]
        public int VillaId { get; set; }

        public string SpecialDetails { get; set; }
    
        public VillaDTO Villa { get; set; }
    }
}
