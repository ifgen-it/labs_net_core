using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practice3.Domain.Entities
{
    public partial class Actor
    {
        public Actor() => MovieActors = new HashSet<MovieActor>();

        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public virtual ICollection<MovieActor> MovieActors { get; set; }
        public override string ToString()
        {
            return $"Id={Id}, FirstName={FirstName}, LastName={LastName}";
        }
    }
}
