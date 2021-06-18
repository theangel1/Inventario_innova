using Inventory.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory.Utility
{
    public class SD
    {
        public const string Externo = "Externo"; //tiene que ver ciertas opciones
        public const string Admin = "Admin"; //Creara usuarios y administrara todo
        public const string Control = "Control"; //usuario normal, por determinar        
        public const string folder = @"resources";
        public const string folderLpn = @"resources/lpn";

        public const string ImageFolder= @"images\ProductImage";
        public const string DefaultProductImage = "default_image.png";


    }
}
