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
    public class ConsultaController : Controller
    {
        string cadena = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;

        IEnumerable<Cliente> clientes()
        {
            List<Cliente> temporal = new List<Cliente>();
            SqlConnection cn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand(
            "SELECT DNI, NOMBRE FROM CLIENTE", cn);
            cn.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Cliente reg = new Cliente();
                reg.dni = dr.GetString(0);
                reg.nombre = dr.GetString(1);
                temporal.Add(reg);
            }
            dr.Close(); cn.Close();
            return temporal;
        }

        IEnumerable<Pedido> pedidos(string nombre = "")
        {
            List<Pedido> temporal = new List<Pedido>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("MUESTRA_VENTA", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NOMBRE", nombre);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Pedido reg = new Pedido()
                    {
                        cod_ven = dr.GetString(0),
                        dni = dr.GetString(1),
                        fecha_ped = dr.GetDateTime(2),
                        monto = dr.GetDecimal(3)
                    };
                    temporal.Add(reg);
                }
                cn.Close();
                dr.Close();
            }
            return temporal;
        }

        IEnumerable<DetallePedido> detallepedidos(string id = "") 
        {
            List<DetallePedido> temporal = new List<DetallePedido>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("MUESTRA_DETALLEVENTA", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COD", id);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    DetallePedido reg = new DetallePedido()
                    {
                        cod_ven = dr.GetString(0),
                        cod_pro = dr.GetString(1),
                        cantidad_pro = dr.GetInt32(2),
                        subtotal = dr.GetDecimal(3)
                    };
                    temporal.Add(reg);
                }
                cn.Close();
                dr.Close();
            }
            return temporal;

        }

        public ActionResult Index(string nombre = "")
        {
           
            IEnumerable<Pedido> listado = pedidos(nombre);

            return View(listado);
        }

        public ActionResult Selecciona(string id = "")
        {

            IEnumerable<DetallePedido> listado = detallepedidos(id);

            return View(listado);
        }


    }
}