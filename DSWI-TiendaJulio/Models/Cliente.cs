using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSWI_TiendaJulio.Models
{
    public class Cliente
    {
        public string dni { get; set; }
        public string nombre { get; set; }
        public string telefono { get; set; }
        public string direccion { get; set; }
        public string COD_DIS { get; set; }
        public string idusuario { get; set; }
        public int tipo_usuario { get; set; }
    }
}