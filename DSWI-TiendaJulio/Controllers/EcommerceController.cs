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

        //TIENDA

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

        IEnumerable<Cliente> clientes()
        {
            List<Cliente> temporal = new List<Cliente>();
            SqlConnection cn = new SqlConnection(cadena);
            SqlCommand cmd = new SqlCommand(
            "SELECT * FROM CLIENTE", cn);
            cn.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Cliente reg = new Cliente();
                reg.dni = dr.GetString(0);
                reg.nombre = dr.GetString(1);
                reg.telefono = dr.GetString(2);
                reg.direccion = dr.GetString(3);
                reg.COD_DIS = dr.GetString(4);
                reg.idusuario = dr.GetString(5);
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

            List<Item> temporal = (List<Item>)Session["carrito"];

            ViewBag.totalcarrito = temporal.Count;

            //si id no es null, envio los datos del producto
            return View(Buscar(id));
        }

        [HttpPost] public ActionResult Agregar(string cod, Int16 stock, int cantidad)
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

            ViewBag.totalcarrito = temporal.Count;

            return View(reg);
        }

        public ActionResult Canasta()
        {
            //asignar a ViewBag.usuario en nombre del cliente InicioSesion()
            ViewBag.usuario = InicioSesion();

            List<Item> temporal = (List<Item>)Session["carrito"];

            ViewBag.totalcarrito = temporal.Count;

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

        public string InicioSesion()
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

        public ActionResult Inicio(Cliente reg)
        {
            return View(reg);
        }

        [HttpPost] public ActionResult Inicio(string login, string clave)
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
                    return RedirectToAction("CrudProducto", "Ecommerce");
                }
            }
        }

        public ActionResult Registrarse()
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");
            return View(new Cliente());
        }

        [HttpPost] public ActionResult Registrarse(Cliente reg, string nomusuario, string password)
        {
            SqlConnection cn = new SqlConnection(cadena);
            cn.Open();

            SqlCommand cmd = new SqlCommand("SP_CONSULTA_DNI", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@DNI", reg.dni);

            int count = (int)cmd.ExecuteScalar();

            if (count == 0)
            {
                try
                {
                    cmd = new SqlCommand("SP_REGISTRA_CLIENTE", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DNI", reg.dni);
                    cmd.Parameters.AddWithValue("@NOMBRE", reg.nombre);
                    cmd.Parameters.AddWithValue("@TELEFONO", reg.telefono);
                    cmd.Parameters.AddWithValue("@DIRECCION", reg.direccion);
                    cmd.Parameters.AddWithValue("@DISTRITO", reg.COD_DIS);
                    cmd.Parameters.AddWithValue("@NOMUSUARIO", nomusuario);
                    cmd.Parameters.AddWithValue("@PASS", password);
                    int n = cmd.ExecuteNonQuery();
                    ViewBag.mensaje = n.ToString() + "Registro Agregado";
                }
                catch (SqlException ex)
                {
                    ViewBag.mensaje = ex.Message;
                }
                finally
                {
                    cn.Close();
                }

                ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);
                ViewBag.clientes = clientes();
                View(reg);

                return RedirectToAction("Inicio");
            }
            else
            {
                ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);
                ViewBag.mensaje = "Error al ingresar el DNI";
                return View();
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
            List<Item> temporal = (List<Item>)Session["carrito"];

            ViewBag.totalcarrito = temporal.Count;
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
            decimal monto = 0;
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
                    cmd = new SqlCommand("INSERT_DETALLE_VENTA", cn, tr);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idpedido", nropedido);
                    cmd.Parameters.AddWithValue("@idproducto", reg.codigo);
                    cmd.Parameters.AddWithValue("@cantidad", reg.cantidad);
                    cmd.Parameters.AddWithValue("@subtotal", reg.monto);
                    cmd.ExecuteNonQuery();

                    monto = monto + reg.monto;
                }

                cmd = new SqlCommand("Update Venta set monto=@monto where cod_ven = @cod", cn, tr);
                cmd.Parameters.AddWithValue("@monto", monto);
                cmd.Parameters.AddWithValue("@cod", nropedido);
                cmd.ExecuteNonQuery();
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
            Session["carrito"] = new List<Item>();
            return View();
        }

        //vista cruds

        //CRUD EMPLEADO

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
        Empleado buscarEmpleado(string id)
        {
            return empleados().Where(c => c.COD_EMP == id).FirstOrDefault();
        }
        Boolean verificaEmpleado(string id)
        {
            //existe: true, no existe false
            return (buscarEmpleado(id) != null ? true : false);
        }
        public ActionResult CrudEmpleado(string id = "")
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");
            ViewBag.cargos = new SelectList(cargos(), "COD_CARGO", "NOMBRE");
            ViewBag.listusuarios = new SelectList(usuarios(), "idusuario", "nomusuario");


            //envio los proveedores en ViewBag
            ViewBag.empleados = empleados();
            ViewBag.usuario = InicioSesion();

            //preguntamos por id, si esta vacio es un nuevo proveedor, sino ejecutamos buscar
            Empleado reg = (id == "" ? new Empleado() : buscarEmpleado(id));

            //envio reg el Cliente a la Vista
            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudEmpleado(Empleado reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verificaEmpleado(reg.COD_EMP) == false)
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
            ViewBag.listusuarios = new SelectList(usuarios(), "idusuario", "nomusuario", reg.idusuario);

            //envias los clientes
            ViewBag.empleados = empleados();
            ViewBag.usuario = InicioSesion();
            return View(reg);
        }

        public ActionResult SelectEmpleado(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudEmpleado", new { id = id });
        }

        public ActionResult EliminaEmpleado(string id = "")
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

        //CRUD MARCA

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

        Marca buscarMarca(string id)
        {
            return marcas().Where(c => c.COD_MAR == id).FirstOrDefault();
        }

        Boolean verificaMarca(string id)
        {
            //existe: true, no existe false
            return (buscarMarca(id) != null ? true : false);
        }

        public ActionResult CrudMarca(string id = "")
        {
            ViewBag.marcas = marcas();
            ViewBag.usuario = InicioSesion();
            Marca reg = (id == "" ? new Marca() : buscarMarca(id));

            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudMarca(Marca reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verificaMarca(reg.COD_MAR) == false)
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
            ViewBag.marcas = marcas();
            ViewBag.usuario = InicioSesion();
            return View(reg);
        }

        public ActionResult SelectMarca(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudMarca", new { id = id });
        }

        public ActionResult EliminaMarca(string id = "")
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

        //CRUD USUARIO

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

        Usuario buscarUsuario(string id)
        {
            return usuarios().Where(c => c.idusuario == id).FirstOrDefault();
        }

        Boolean verificaUsuario(string id)
        {
            //existe: true, no existe false
            return (buscarUsuario(id) != null ? true : false);
        }

        public ActionResult CrudUsuario(string id = "")
        {
            ViewBag.tipoUsuarios = new SelectList(tipoUsuarios(), "id_tipo", "nombre");

            //envio los usuarios en ViewBag
            ViewBag.listausuarios = usuarios();
            ViewBag.usuario = InicioSesion();

            //preguntamos por id, si esta vacio es un nuevo usuario, sino ejecutamos buscar
            Usuario reg = (id == "" ? new Usuario() : buscarUsuario(id));

            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudUsuario(Usuario reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verificaUsuario(reg.idusuario) == false)
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
            ViewBag.listausuarios = usuarios();
            ViewBag.usuario = InicioSesion();

            return View(reg);
        }

        public ActionResult SelectUsuario(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudUsuario", new { id = id });
        }

        public ActionResult EliminaUsuario(string id = "")
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

        //CRUD PROVEEDOR

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

        Proveedor buscarProveedor(string id)
        {
            return proveedores().Where(c => c.RUC == id).FirstOrDefault();
        }

        Boolean verificaProveedor(string id)
        {
            //existe: true, no existe false
            return (buscarProveedor(id) != null ? true : false);
        }

        public ActionResult CrudProveedor(string id = "")
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");

            ViewBag.proveedores = proveedores();
            ViewBag.usuario = InicioSesion();

            Proveedor reg = (id == "" ? new Proveedor() : buscarProveedor(id));


            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudProveedor(Proveedor reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verificaProveedor(reg.RUC) == false)
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


            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);

            ViewBag.proveedores = proveedores();
            ViewBag.usuario = InicioSesion();
            return View(reg);
        }

        public ActionResult SelectProveedor(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudProveedor", new { id = id });
        }

        public ActionResult EliminaProveedor(string id = "")
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

        //CRUD PRODUCTO

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

        IEnumerable<Producto> listadoProductos()
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

        Producto buscarProducto(string COD_PRO)
        {
            return listadoProductos().Where(c => c.COD_PRO == COD_PRO).FirstOrDefault();
        }

        Boolean verificaProducto(string id)
        {
            //existe: true, no existe false
            return (buscarProducto(id) != null ? true : false);
        }

        public ActionResult CrudProducto(string id = "")
        {

            ViewBag.marcas = new SelectList(marcas(), "COD_MAR", "NOM_MAR");

            ViewBag.categorias = new SelectList(categorias(), "COD_CAT", "NOM_CAT");

            //envio los clientes en ViewBag
            ViewBag.productos = listadoProductos();
            ViewBag.usuario = InicioSesion();

            //preguntamos por id, si esta vacio es un nuevo cliente, sino ejecutamos buscar
            Producto reg = (id == "" ? new Producto() : buscarProducto(id));

            //envio reg el Cliente a la Vista
            return View(reg);
        }

        [HttpPost]
        public ActionResult CrudProducto(Producto reg)
        {
            //este proceso permite Insert(si el idcliente no existe o actualiza si idcliente existe)
            string procedure = "";
            if (verificaProducto(reg.COD_PRO) == false)
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
            ViewBag.productos = listadoProductos();
            ViewBag.usuario = InicioSesion();

            return View(reg);
        }

        public ActionResult SelectProducto(string id)
        {
            //reenvias el id al Index para que imprima los datos
            return RedirectToAction("CrudProducto", new { id = id });
        }

        public ActionResult EliminaProducto(string id = "")
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

        //CRUD CLIENTE

        Cliente buscarCliente(string id = "")
        {
            return clientes().Where(c => c.dni == id).FirstOrDefault();
        }

        Boolean verificaCliente(string id)
        {
            return (buscarCliente(id) != null ? true : false);
        }

        public ActionResult CrudCliente(string id = "")
        {
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE");

            ViewBag.usuarios = new SelectList(usuarios(), "idusuario", "nomusuario");

            ViewBag.clientes = clientes();

            Cliente reg = (id == "" ? new Cliente() : buscarCliente(id));

            return View(reg);
        }

        [HttpPost] public ActionResult CrudCliente(Cliente reg)
        {
            string procedure = "";
            if (verificaCliente(reg.dni) == false)
            {
                procedure = "sp_inserta_cliente";
                ViewBag.mensaje = "Registro Agregado";
            }
            else
            {
                procedure = "sp_actualiza_cliente";
                ViewBag.mensaje = "Registro Actualizado";
            }

            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                SqlCommand cmd = new SqlCommand(procedure, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@dni", reg.dni);
                cmd.Parameters.AddWithValue("@nombre", reg.nombre);
                cmd.Parameters.AddWithValue("@telefono", reg.telefono);
                cmd.Parameters.AddWithValue("@direccion", reg.direccion);
                cmd.Parameters.AddWithValue("@distrito", reg.COD_DIS);
                cmd.Parameters.AddWithValue("@usuario", reg.idusuario);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                ViewBag.mensaje = ex.Message;
            }
            finally
            {
                cn.Close();
            }
            ViewBag.distritos = new SelectList(distritos(), "COD_DIS", "NOMBRE", reg.COD_DIS);
            ViewBag.usuarios = new SelectList(usuarios(), "idusuario", "nomusuario", reg.idusuario);
            ViewBag.clientes = clientes();
            return View(reg);
        }

        public ActionResult EliminarCliente(string id)
        {
            int i = 0;
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                if (id == null)
                {
                    i = -1;
                    ViewBag.mensaje = "El registro ya ha sido eliminado, limpie los valores";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("sp_elimina_cliente", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    i = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.mensaje = ex.Message;
            }
            finally
            {
                cn.Close();
            }
            return RedirectToAction("Clienteid", new { id = "", eliminar = i });
        }

        public ActionResult Selectcli(string id)
        {
            return RedirectToAction("CrudCliente", new { id = id });

        }

        //pedidos

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

        IEnumerable<Cliente> buscarcliente(string id = "") 
        {
            List<Cliente> temporal = new List<Cliente>();
            using (SqlConnection cn = new SqlConnection(cadena)) 
            {
                SqlCommand cmd = new SqlCommand("Select dni, nombre from cliente where id = @id", cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read()) 
                {
                    Cliente reg = new Cliente()
                    {
                        dni = dr.GetString(0),
                        nombre = dr.GetString(1)
                    };
                    temporal.Add(reg);
                }
                cn.Close();
                dr.Close();
            }
            return temporal;
        }

        IEnumerable<Producto> buscarproducto(string id = "")
        {
            List<Producto> temporal = new List<Producto>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("Select cod_pro, nom_pro, precio from Producto where cod_pro = @id", cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Producto reg = new Producto()
                    {
                        COD_PRO = dr.GetString(0),
                        NOM_PRO = dr.GetString(1),
                        PRECIO_VENTA = dr.GetDecimal(2)
                    };
                    temporal.Add(reg);
                }
                cn.Close();
                dr.Close();
            }
            return temporal;
        }

        public ActionResult ListadoPedidos(string nombre = "")
        {

            IEnumerable<Pedido> listado = pedidos(nombre);

            return View(listado);
        }

        public ActionResult DetalledelPedido(string id = "")
        {

            IEnumerable<DetallePedido> listado = detallepedidos(id);

            return View(listado);
        }

        public ActionResult Venta()
        {
            
            ViewBag.idempleado = (Session["login"] as Cliente).dni;
            ViewBag.empleado = (Session["login"] as Cliente).nombre;
            ViewBag.cliente = clientes();

            return View();
        }

        [HttpPost] public ActionResult Venta(Pedido reg, DetallePedido reg2)
        {

            return View();
        }


    }
}
