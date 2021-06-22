using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSWI_TiendaJulio.Models
{
    public class Item
    {
        //el Session almacenara una coleccion de esta estructura
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal precio { get; set; }
        public int cantidad { get; set; }//cantidad solicitada
        public decimal monto { get { return precio * cantidad; } }//retorna el producto
    }
}