using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_VillaAPI.Models
{
    public class VillaNumber
    {
        [Key]
        // I prevent the database from generating the ID automatically, since the room number of a hotel does not necessarily start at 1
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VillaNro { get; set; }
        public string SpecialDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        //If we add a foreign key and our database already contains records that do not have a field for the foreign key,
        //when we add a migration and update the database with that foreign key, it is necessary to delete all records from Villa Number
        [ForeignKey("Villa")]
        public int VillaId { get; set; }
        public Villa Villa { get; set; }

    }
}
