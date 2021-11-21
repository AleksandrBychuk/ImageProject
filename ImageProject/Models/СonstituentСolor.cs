using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class СonstituentСolor
    {
        public int Id { get; set; }
        public string HexColor { get; set; }
        public int ValueCount { get; set; }
        public UserImage Image { get; set; }
    }
}
