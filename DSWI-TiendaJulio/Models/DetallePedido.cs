using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSWI_TiendaJulio.Models
{
    public class DetallePedido
    {
        public string cod_ven { get; set; }
        public string cod_pro { get; set; }
        public int cantidad_pro { get; set; }
        public decimal subtotal { get; set; }
    }
}