using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSWI_TiendaJulio.Models
{
    public class Pedido
    {
        public string cod_ven { get; set; }
        public string dni { get; set; }
        public DateTime fecha_ped { get; set; }
        public decimal monto { get; set; }
    }
}