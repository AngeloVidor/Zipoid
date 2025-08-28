using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zipoid.API.Domain.Entities
{
    public class Track
    {
        public string Id { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
    }
}