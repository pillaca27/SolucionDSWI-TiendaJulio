using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DSWI_TiendaJulio.Models;

namespace DSWI_TiendaJulio.Controllers
{
    public class ECommerceController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        IEnumerable<Producto> productos()
        {
            List<Producto> temporal = new List<Producto>();
            SqlConnection cn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand(
            "SELECT COD_PRO, NOM_PRO, PRECIO_VENTA, STOCK FROM PRODUCTO", cn);
            cn.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Producto reg = new Producto();
                reg.COD_PRO = dr.GetString(0);
                reg.NOM_PRO = dr.GetString(1);
                reg.PRECIO_VENTA = dr.GetDecimal(2);
                reg.STOCK = dr.GetInt32(3);
                temporal.Add(reg);
            }
            dr.Close(); cn.Close();
            return temporal;
        }

        IEnumerable<Producto> filtro(string nombre = null)
        {
            List<Producto> temporal = new List<Producto>();
            SqlConnection cn = new SqlConnection(cadena);
            if (nombre == null)
            {
                nombre = " ";
                SqlCommand cmd = new SqlCommand("SP_FILTRO_PRODUCTO", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Producto reg = new Producto();
                    reg.COD_PRO = dr.GetString(0);
                    reg.NOM_PRO = dr.GetString(1);
                    reg.PRECIO_VENTA = dr.GetDecimal(2);
                    reg.STOCK = dr.GetInt32(3);
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
                return temporal;
            }
            else 
            {
                SqlCommand cmd = new SqlCommand("SP_FILTRO_PRODUCTO", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Producto reg = new Producto();
                    reg.COD_PRO = dr.GetString(0);
                    reg.NOM_PRO = dr.GetString(1);
                    reg.PRECIO_VENTA = dr.GetDecimal(2);
                    reg.STOCK = dr.GetInt32(3);
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
                return temporal;
            }
        }

        Producto Buscar(string id = null)
        {
            if (id == null)
                return new Producto();
            else
                return productos().Where(p => p.COD_PRO == id).FirstOrDefault();

        }
        public ActionResult Tienda(string nombre = null)
        {
            //primero evaluo si existe el Session["carrito"]
            //el cual permitira almacenar los productos seleccionados(1 vez)
            if (Session["carrito"] == null)
                Session["carrito"] = new List<Item>();

            //asignar a ViewBag.usuario en nombre del cliente InicioSesion()
            ViewBag.usuario = InicioSesion();

            List<Item> temporal = (List<Item>)Session["carrito"];

            ViewBag.totalcarrito = temporal.Count;

            return View(filtro(nombre));
        }




        public ActionResult Agregar(string id = null)
        {
            if (id == null) return RedirectToAction("Tienda");

            //asignar a ViewBag.usuario en nombre del cliente InicioSesion()
            ViewBag.usuario = InicioSesion();

            //si id no es null, envio los datos del producto
            return View(Buscar(id));
        }
        [HttpPost]
        public ActionResult Agregar(string cod, Int16 stock, int cantidad)
        {
            if (cantidad > stock)
            {
                ViewBag.mensaje = "Ingrese una cantidad menor al stock";
                return View(Buscar(cod));
            }
            // a continuacion recupero el Producto por su cod
            Producto reg = Buscar(cod);
            //instanciar Item y pasar sus datos
            Item it = new Item()
            {
                codigo = reg.COD_PRO,
                descripcion = reg.NOM_PRO,
                precio = reg.PRECIO_VENTA,
                cantidad = cantidad,
            };
            //agregar it al Session carrito, referenciar con temporal
            List<Item> temporal = (List<Item>)Session["carrito"];
            temporal.Add(it);
            //asignar a ViewBag.usuario en nombre del cliente InicioSesion()
            ViewBag.usuario = InicioSesion();

            ViewBag.mensaje = "Producto Agregado";
            return View(reg);
        }
        public ActionResult Canasta()
        {
            //asignar a ViewBag.usuario en nombre del cliente InicioSesion()
            ViewBag.usuario = InicioSesion();

            //visualizar el contenido del Session["carrito"], productos seleccionado
            return View((List<Item>)Session["carrito"]);
        }
        public ActionResult Delete(string id)
        {
            //eliminar el Item del Session["carrito"]
            List<Item> temporal = (List<Item>)Session["carrito"];
            //buscar
            Item reg = temporal.Find(i => i.codigo == id);
            temporal.Remove(reg);

            return RedirectToAction("Canasta");
        }
        string InicioSesion()
        {
            if (Session["login"] == null)
                return null;
            else
                return (Session["login"] as Cliente).nombre;
        }
        Cliente Buscar(string login, string clave)
        {
            Cliente reg = null; //inicializar
            SqlConnection cn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand("sp_logueo", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", login);
            cmd.Parameters.AddWithValue("@clave", clave);
            cn.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                reg = new Cliente();
                reg.dni = dr["DNI"].ToString();
                reg.nombre = dr["NOMBRE"].ToString();
                reg.direccion = dr["DIRECCION"].ToString();
                reg.telefono = dr["TELEFONO"].ToString();
                reg.tipo_usuario = Convert.ToInt32(dr["id_tipo"].ToString());
            }
            dr.Close();
            cn.Close();

            return reg;

        }
        public ActionResult Inicio()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Inicio(string login, string clave)
        {
            //ejecutar el Buscar y lo almaceno en Session["login"]
            Session["login"] = Buscar(login, clave);
            if (Session["login"] == null)
            {
                ViewBag.mensaje = "Usuario o Clave Incorrecta";
                return View();
            }
            else
            {
                int tipo_usuario = Buscar(login, clave).tipo_usuario;
                if (tipo_usuario == 4)
                {
                    return RedirectToAction("Tienda");
                }
                else
                {
                    return RedirectToAction("CrudProducto", "Producto");
                }
            }
        }
        public ActionResult Cerrar()
        {
            //cerrar la sesión del usuario
            Session["login"] = null;
            return RedirectToAction("Tienda");
        }
        public ActionResult Comprar()
        {
            //Verifico si la sessión es null
            if (Session["login"] == null)
            {
                return RedirectToAction("Inicio");
            }
            else
            {
                //envio a la vista los siguientes datos Comprar
                ViewBag.usuario = InicioSesion();
                ViewBag.carrito = Session["carrito"] as List<Item>;
                return View(Session["login"] as Cliente);
            }
        }

        String autogenerado()
        {
            //ejecutar la funcion del autogenerado
            string cod = "";
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("select dbo.autogenera()", cn);
                cn.Open();
                //ejecuta y retorna el valor (object) convertido a String
                cod = (string)cmd.ExecuteScalar();
                cn.Close();
            }
            return cod;
        }

        public ActionResult Pedido()
        {
            string nropedido = autogenerado();
            string mensaje = "";//almaceno el mensaje del proceso
            string idcliente = (Session["login"] as Cliente).dni;

            //definir la transacción
            SqlConnection cn = new SqlConnection(cadena);
            cn.Open();
            SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                SqlCommand cmd = new SqlCommand("INSERT VENTA(COD_VEN, DNI, FECHA_PED, MONTO) values(@idpedido, @idcliente, @fecha, @monto)", cn, tr);
                cmd.Parameters.AddWithValue("@idpedido", nropedido);
                cmd.Parameters.AddWithValue("@idcliente", idcliente);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@monto", 0);
                cmd.ExecuteNonQuery();

                foreach (Item reg in Session["carrito"] as List<Item>)
                {
                    cmd = new SqlCommand("INSERT DETALLE_VENTA values (@idpedido, @idproducto, @cantidad, @subtotal)", cn, tr);
                    cmd.Parameters.AddWithValue("@idpedido", nropedido);
                    cmd.Parameters.AddWithValue("@idproducto", reg.codigo);
                    cmd.Parameters.AddWithValue("@cantidad", reg.cantidad);
                    cmd.Parameters.AddWithValue("@subtotal", reg.monto);
                    cmd.ExecuteNonQuery();
                }
                //Si todo está OK
                tr.Commit();
                mensaje = string.Format("El pedido {0} ha sido registrado", nropedido);

            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                tr.Rollback();
            }
            finally { cn.Close(); }
            //este pedido direcciona a una ventana final después de ejecutar el pedido
            return RedirectToAction("Mensajes", new { m = mensaje });

        }
        public ActionResult Mensajes(String m)
        {
            //envio el de m
            ViewBag.mensaje = m;
            //finalizo la sesión
            Session.Abandon();
            return View();
        }
    }
}
