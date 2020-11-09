using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class CoverType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Cover Type Name")]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
