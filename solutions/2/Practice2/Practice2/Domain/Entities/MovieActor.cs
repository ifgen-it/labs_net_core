using System;
using System.Collections.Generic;

namespace Practice2.Domain.Entities
{
    public partial class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }

        public virtual Actor Actor { get; set; }
        public virtual Movie Movie { get; set; }
    }
}
