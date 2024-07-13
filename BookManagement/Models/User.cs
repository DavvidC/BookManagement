using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Możemy zainicjalizować Books jako pustą listę, aby uniknąć błędów
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
