using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Ocena musi być w zakresie od 1 do 5.")]
        public int Value { get; set; }

        public Book Book { get; set; }
    }
}
