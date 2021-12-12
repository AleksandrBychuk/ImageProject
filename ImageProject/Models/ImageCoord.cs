using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class ImageCoord
    {
        [Key]
        public int Id { get; set; }
        public int ImageId { get; set; }
        public UserImage Image { get; set; }

        [Range(-90, 90)]
        public int LatitudeDegree { get; set; } // Широта/Градус

        [Range(0, 59)]
        public int LatitudeMinute { get; set; } // Широта/Минута

        [Range(0, 59)]
        public decimal LatitudeSecond { get; set; } // Широта/Секунда

        [Range(-180, 180)]
        public int LongitudeDegree { get; set; } // Долгота/Градус

        [Range(0, 59)]
        public int LongitudeMinute { get; set; } // Долгота/Минута

        [Range(0, 59)]
        public decimal LongitudeSecond { get; set; } // Долгота/Секунда
        public decimal Altitude { get; set; } // Высота
    }
}
