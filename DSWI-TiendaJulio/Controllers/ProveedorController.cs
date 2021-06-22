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
    public class ProveedorController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

        IEnumerable<Distrito> distritos()
        {
            List<Distrito> temporal = new List<Distrito>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select*from DISTRITO", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Distrito reg = new Distrito()
                    {
                        COD_DIS = dr.GetString(0),
                        NOMBRE = dr.GetString(1)
                    };
                    temporal.Add(reg);
                }
                dr.Close();
                cn.Close();
            }
            return temporal;
        }

        IEnumerable<Proveedor> proveedores()
        {
            List<Proveedor> temporal = new List<Proveedor>();
            //if (nombre == null) return temporal;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_lista_proveedores", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Proveedor reg = new Proveedor()
                    {
                        RUC = dr.GetString(0),
                        NOMBRE = dr.GetString(1),
                        NOMBRE_CONTACTO = dr.GetString(2),
                        TELEFONO_CONTACTO = dr.GetString(3),
                        DIRECCION = dr.GetString(4),
                        COD_DIS = dr.GetString(5)
                    };
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }

        Proveedor buscar(string id)
        {
            return proveedores().Where(c => c.RUC == id).FirstOrDefault();
        }
        Boolean verifica(string id)
        {
            //existe: true, no existe false
            return (buscar(id) != null ? true : false);
        }
        public ActionResult CrudProveedor(string id = "")
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");

            //envio los proveedores en ViewBag
            ViewBag.proveedores = proveedores();

            //preguntamos por id, si esta vacio es un nuevo proveedor, sino ejecutamos buscar
            Proveedor reg = (id == "" ? new Proveedor() : buscar(id));

            //envio reg el Cliente a la Vista
            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudProveedor(Proveedor reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verifica(reg.RUC) == false)
            {
                procedure = "sp_inserta_proveedor";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_proveedor";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ruc", reg.RUC);
                cmd.Parameters.AddWithValue("@nombre", reg.NOMBRE);
                cmd.Parameters.AddWithValue("@contacto", reg.NOMBRE_CONTACTO);
                cmd.Parameters.AddWithValue("@telefono", reg.TELEFONO_CONTACTO);
                cmd.Parameters.AddWithValue("@direccion", reg.DIRECCION);
                cmd.Parameters.AddWithValue("@distrito", reg.COD_DIS);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) { ViewBag.mensaje = ex.Message; }
            finally { cn.Close(); }

            //envias los distritos
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);
            //envias los clientes
            ViewBag.proveedores = proveedores();
            return View(reg);
        }

        public ActionResult Select(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudProveedor", new { id = id });
        }

        public ActionResult Elimina(string id = "")
        {
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_elimina_proveedor", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ruc", id);
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
            return RedirectToAction("CrudProveedor");
        }
    }
}