using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Models
{
    public class UserImage
    {
        public int Id { get; set; }
        public DateTime DateAdded { get; set; }
        public byte[] Image { get; set; }
        public User UserOwner { get; set; }

        //КООРДИНАТЫ! Как? Что? От куда? Максимально просто для юзеров и максимально эффективно
    }
}
