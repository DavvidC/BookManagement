using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BookManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Autor jest wymagany.")]
        public string Author { get; set; }

        public string Description { get; set; }

        public bool IsPublic { get; set; } = true;

        public int? UserId { get; set; }
        public User User { get; set; }

        [NotMapped]
        public double AverageRating { get; set; } 
    }
}
