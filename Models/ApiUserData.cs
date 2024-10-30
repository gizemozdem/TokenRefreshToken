using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenRefreshToken.Models
{
    public class ApiUserData
    {
        public static List<ApiUser> Kullanici = new()
        {
            new ApiUser { Id = 1, KullaniciAdi = "gizem", Sifre = "134", Rol = "Yönetici" },
            new ApiUser { Id = 2, KullaniciAdi = "demo", Sifre = "1345", Rol = "Kullanıcı" }
        };
    }
}
