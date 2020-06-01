using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfClient.Entities
{
    public partial class Movie
    {
        public Movie() => MovieActors = new HashSet<MovieActor>();

        public int Id { get; set; }
        //[Required]
        //[StringLength(60, MinimumLength = 3, ErrorMessage = "Name must be from 3 to 60 symbols")]
        public string Name { get; set; }
        //[Range(1700,2030, ErrorMessage = "Incorrect year")]
        public int Year { get; set; }
        //[Range(1.0, 9.99, ErrorMessage = "Rating must be from 1,0 to 9,99")]
        public float Rating { get; set; }
        //[Range(0, 1000000, ErrorMessage = "Incorrect price")]
        public float Price { get; set; }
        public int? GenreId { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual ICollection<MovieActor> MovieActors { get; set; }
        public override string ToString()
        {
            return $"Id={Id}, Name={Name}, Year={Year}, Rating={Rating}, Price={Price}, GenreId={GenreId}, Genre={Genre?.Name}";
        }
    }
}
