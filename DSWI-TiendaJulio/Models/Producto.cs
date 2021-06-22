using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSWI_TiendaJulio.Models
{
    public class Producto
    {
        public string COD_PRO { get; set; }
        public string NOM_PRO { get; set; }
        public decimal PRECIO_VENTA { get; set; }
        public int STOCK { get; set; }
        public string COD_MAR { get; set; }
        public string COD_CAT { get; set; }
    }
}