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
    public class UsuarioController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

        IEnumerable<TipoUsuario> tipoUsuarios()
        {
            List<TipoUsuario> temporal = new List<TipoUsuario>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select*from tipo_usuario", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    TipoUsuario reg = new TipoUsuario()
                    {
                        id_tipo = dr.GetString(0),
                        nombre = dr.GetString(1)
                    };
                    temporal.Add(reg);
                }
                dr.Close();
                cn.Close();
            }
            return temporal;
        }

        IEnumerable<Usuario> usuarios()
        {
            List<Usuario> temporal = new List<Usuario>();
            //if (nombre == null) return temporal;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_lista_usuarios", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Usuario reg = new Usuario()
                    {
                        idusuario = dr.GetString(0),
                        nomusuario = dr.GetString(1),
                        contraseña = dr.GetString(2),
                        tipo = dr.GetString(3)
                    };
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }

        Usuario buscar(string id)
        {
            return usuarios().Where(c => c.idusuario == id).FirstOrDefault();
        }
        Boolean verifica(string id)
        {
            //existe: true, no existe false
            return (buscar(id) != null ? true : false);
        }

        public ActionResult CrudUsuario(string id="")
        {
            ViewBag.tipoUsuarios = new SelectList(tipoUsuarios(), "id_tipo", "nombre");

            //envio los usuarios en ViewBag
            ViewBag.usuarios = usuarios();

            //preguntamos por id, si esta vacio es un nuevo usuario, sino ejecutamos buscar
            Usuario reg = (id == "" ? new Usuario() : buscar(id));

            return View(reg);
        }
        [HttpPost]
        public ActionResult CrudUsuario(Usuario reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verifica(reg.idusuario) == false)
            {
                procedure = "sp_inserta_usuario";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_usuario";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", reg.idusuario);
                cmd.Parameters.AddWithValue("@nombre", reg.nomusuario);
                cmd.Parameters.AddWithValue("@contraseña", reg.contraseña);
                cmd.Parameters.AddWithValue("@tipo", reg.tipo);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) { ViewBag.mensaje = ex.Message; }
            finally { cn.Close(); }

            //envias los distritos
            ViewBag.tipoUsuarios = new SelectList(tipoUsuarios(), "id_tipo", "nombre", reg.tipo);
            //envias los clientes
            ViewBag.usuario = usuarios();
            return View(reg);
        }
        public ActionResult Select(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudUsuario", new { id = id });
        }

        public ActionResult Elimina(string id = "")
        {
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_elimina_usuario", cn);
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
            return RedirectToAction("CrudUsuario");
        }
    }
}