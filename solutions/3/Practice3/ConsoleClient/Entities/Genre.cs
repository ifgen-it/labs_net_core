using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsoleClient.Entities
{
    public partial class Genre
    {
        public Genre() => Movies = new HashSet<Movie>();

        public int Id { get; set; }
        //[Required]
        public string Name { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }
        public override string ToString()
        {
            return $"Id={Id}, Name={Name}";
        }
    }
}
