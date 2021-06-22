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
    public class ProductoController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

        IEnumerable<Categoria> categorias()
        {
            List<Categoria> temporal = new List<Categoria>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select*from CATEGORIA", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Categoria reg = new Categoria()
                    {
                        COD_CAT = dr.GetString(0),
                        NOM_CAT = dr.GetString(1)
                    };
                    temporal.Add(reg);
                }
                dr.Close();
                cn.Close();
            }
            return temporal;
        }

        IEnumerable<Marca> marcas()
        {
            List<Marca> temporal = new List<Marca>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select*from MARCA", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Marca reg = new Marca()
                    {
                        COD_MAR = dr.GetString(0),
                        NOM_MAR = dr.GetString(1)
                    };
                    temporal.Add(reg);
                }
                dr.Close();
                cn.Close();
            }
            return temporal;
        }

        IEnumerable<Producto> productos()
        {
            List<Producto> temporal = new List<Producto>();
            //if (nombre == null) return temporal;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                //SqlCommand cmd = new SqlCommand(
                //"Select P.COD_PRO, P.NOM_PRO, P.PRECIO_VENTA, P.stock, M.NOM_MAR, C.NOM_CAT " +
                //"from PRODUCTO P join MARCA M on P.COD_MAR = M.COD_MAR " +
                //"join CATEGORIA C on P.COD_CAT = C.COD_CAT" +
                //"where P.NOM_PRO like @nombre+'%'", cn);
                //cmd.Parameters.AddWithValue("@nombre", nombre);
                SqlCommand cmd = new SqlCommand("sp_lista_productos", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Producto reg = new Producto()
                    {
                        COD_PRO = dr.GetString(0),
                        NOM_PRO = dr.GetString(1),
                        PRECIO_VENTA = dr.GetDecimal(2),
                        STOCK = dr.GetInt32(3),
                        COD_MAR = dr.GetString(4),
                        COD_CAT = dr.GetString(5)
                    };
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }

        Producto buscar(string COD_PRO)
        {
            return productos().Where(c => c.COD_PRO == COD_PRO).FirstOrDefault();
        }
        Boolean verifica(string id)
        {
            //existe: true, no existe false
            return (buscar(id) != null ? true : false);
        }
        public ActionResult CrudProducto(string id = "")
        {
            ViewBag.categorias = new SelectList(categorias(), "COD_CAT", "NOM_CAT");
            ViewBag.marcas = new SelectList(marcas(), "COD_MAR", "NOM_MAR");

            //envio los clientes en ViewBag
            ViewBag.productos = productos();

            //preguntamos por id, si esta vacio es un nuevo cliente, sino ejecutamos buscar
            Producto reg = (id == "" ? new Producto() : buscar(id));

            //envio reg el Cliente a la Vista
            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudProducto(Producto reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verifica(reg.COD_PRO) == false)
            {
                procedure = "sp_inserta_producto";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_producto";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", reg.COD_PRO);
                cmd.Parameters.AddWithValue("@nombre", reg.NOM_PRO);
                cmd.Parameters.AddWithValue("@precio", reg.PRECIO_VENTA);
                cmd.Parameters.AddWithValue("@stock", reg.STOCK);
                cmd.Parameters.AddWithValue("@idmar", reg.COD_MAR);
                cmd.Parameters.AddWithValue("@idcat", reg.COD_CAT);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) { ViewBag.mensaje = ex.Message; }
            finally { cn.Close(); }

            //envias los marcas
            ViewBag.marcas = new SelectList(marcas(), "COD_MAR", "NOM_MAR", reg.COD_MAR);
            //envias los categorias
            ViewBag.categorias = new SelectList(categorias(), "COD_CAT", "NOM_CAT", reg.COD_CAT);
            //envias los clientes
            ViewBag.productos = productos();
            return View(reg);
        }

        public ActionResult Select(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudProducto", new { id = id });
        }

        public ActionResult Elimina(string id = "")
        {
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_elimina_producto", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                cmd.ExecuteNonQuery();
                ViewBag.mensaje = "Registro Eliminado";
            }
            catch (SqlException ex)
            {
                ViewBag.mensaje = ex.Message;
            }
            finally
            {
                cn.Close();
            }
            return RedirectToAction("CrudProducto");
        }

    }
}