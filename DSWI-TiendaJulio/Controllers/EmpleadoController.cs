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
    public class EmpleadoController : Controller
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
        IEnumerable<Cargo> cargos()
        {
            List<Cargo> temporal = new List<Cargo>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select*from CARGO", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Cargo reg = new Cargo()
                    {
                        COD_CARGO = dr.GetString(0),
                        NOMBRE = dr.GetString(1)
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

        IEnumerable<Empleado> empleados()
        {
            List<Empleado> temporal = new List<Empleado>();

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_lista_empleados", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Empleado reg = new Empleado()
                    {
                        COD_EMP = dr.GetString(0),
                        NOMBRE = dr.GetString(1),
                        DNI = dr.GetString(2),
                        TELEFONO = dr.GetString(3),
                        DIRECCION = dr.GetString(4),
                        COD_DIS = dr.GetString(5),
                        COD_CARGO = dr.GetString(6),
                        idusuario = dr.GetString(7),
                    };
                    temporal.Add(reg);
                }
                dr.Close(); cn.Close();
            }
            return temporal;
        }
        Empleado buscar(string id)
        {
            return empleados().Where(c => c.COD_EMP == id).FirstOrDefault();
        }
        Boolean verifica(string id)
        {
            //existe: true, no existe false
            return (buscar(id) != null ? true : false);
        }
        public ActionResult CrudEmpleado(string id = "")
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");
            ViewBag.cargos = new SelectList(cargos(), "COD_CARGO", "NOMBRE");
            ViewBag.usuarios = new SelectList(usuarios(), "idusuario", "nomusuario");


            //envio los proveedores en ViewBag
            ViewBag.empleados = empleados();

            //preguntamos por id, si esta vacio es un nuevo proveedor, sino ejecutamos buscar
            Empleado reg = (id == "" ? new Empleado() : buscar(id));

            //envio reg el Cliente a la Vista
            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudEmpleado(Empleado reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verifica(reg.COD_EMP) == false)
            {
                procedure = "sp_inserta_empleado";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_empleado";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", reg.COD_EMP);
                cmd.Parameters.AddWithValue("@nombre", reg.NOMBRE);
                cmd.Parameters.AddWithValue("@dni", reg.DNI);
                cmd.Parameters.AddWithValue("@telefono", reg.TELEFONO);
                cmd.Parameters.AddWithValue("@direccion", reg.DIRECCION);
                cmd.Parameters.AddWithValue("@distrito", reg.COD_DIS);
                cmd.Parameters.AddWithValue("@cargo", reg.COD_CARGO);
                cmd.Parameters.AddWithValue("@usuario", reg.idusuario);


                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) { ViewBag.mensaje = ex.Message; }
            finally { cn.Close(); }

            //envias los distritos
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);
            ViewBag.cargos = new SelectList(cargos(), "COD_CARGO", "NOMBRE", reg.COD_CARGO);
            ViewBag.cargos = new SelectList(usuarios(), "idusuario", "nomusuario", reg.idusuario);

            //envias los clientes
            ViewBag.empleado = empleados();
            return View(reg);
        }

        public ActionResult Select(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudEmpleado", new { id = id });
        }

        public ActionResult Elimina(string id = "")
        {
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand("sp_elimina_empleado", cn);
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
            return RedirectToAction("CrudEmpleado");
        }

    }
}