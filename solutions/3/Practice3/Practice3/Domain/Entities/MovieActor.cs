using System;
using System.Collections.Generic;

namespace Practice3.Domain.Entities
{
    public partial class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }

        public virtual Actor Actor { get; set; }
        public virtual Movie Movie { get; set; }
        public override string ToString()
        {
            return $"MovieId={MovieId}, MovieName={Movie?.Name}, ActorId={ActorId}, ActorFirstName={Actor?.FirstName}, ActorLastName={Actor?.LastName}";
        }
    }
}
