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
    public class MarcaController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

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

        Marca buscar(string id)
        {
            return marcas().Where(c => c.COD_MAR == id).FirstOrDefault();
        }
        Boolean verifica(string id)
        {
            //existe: true, no existe false
            return (buscar(id) != null ? true : false);
        }

        public ActionResult CrudMarca(string id= "")
        {
            ViewBag.marcas = marcas();
            Marca reg = (id == "" ? new Marca() : buscar(id));

            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudMarca(Marca reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verifica(reg.COD_MAR) == false)
            {
                procedure = "sp_inserta_marca";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_marca";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", reg.COD_MAR);
                cmd.Parameters.AddWithValue("@nombre", reg.NOM_MAR);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) { ViewBag.mensaje = ex.Message; }
            finally { cn.Close(); }

            //envias los clientes
            ViewBag.marca = marcas();
            return View(reg);
        }

        public ActionResult Select(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudMarca", new { id = id });
        }

        public ActionResult Elimina(string id = "")
        {
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_elimina_marca", cn);
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
            return RedirectToAction("CrudMarca");
        }

    }
}