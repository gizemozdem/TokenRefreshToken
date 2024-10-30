using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenRefreshToken.Models
{
    public class ApiUser
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public string Rol { get; set; }
        public string RefreshToken { get; set; }
        public object RefreshTokenEndDate { get; internal set; }
    }
}
