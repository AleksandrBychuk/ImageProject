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
        public decimal LatitudeDegree { get; set; } // Широта/Градус
        public decimal LatitudeMinute { get; set; } // Широта/Минута
        public decimal LatitudeSecond { get; set; } // Широта/Секунда
        public decimal LongitudeDegree { get; set; } // Долгота/Градус
        public decimal LongitudeMinute { get; set; } // Долгота/Минута
        public decimal LongitudeSecond { get; set; } // Долгота/Секунда
        public decimal Altitude { get; set; } // Высота
    }
}
