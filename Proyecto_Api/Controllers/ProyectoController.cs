using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Data;
using System.Data.SqlClient;
using Proyecto_Api.Models;

namespace Proyecto_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {
        private readonly string _rutaServidor;
        private readonly string _cadenaConexion;

        public DocumentoController(IConfiguration config)
        {
            _rutaServidor = config.GetSection("Configuracion").GetSection("RutaServidor").Value;
            _cadenaConexion = config.GetConnectionString("CadenaConexion");
        }

        [HttpPost]
        [Route("Subir")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult Subir([FromForm] Documento request)
        {
            string rutaDocumento = Path.Combine(_rutaServidor, request.Archivo.FileName);

            try{

                using (FileStream newFile = System.IO.File.Create(rutaDocumento))
                {
                    request.Archivo.CopyTo(newFile);
                    newFile.Flush();

                }

                using (var conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("SpGuardarDocumento", conexion);
                    cmd.Parameters.AddWithValue("descripcion", request.Descripcion);
                    cmd.Parameters.AddWithValue("ruta", rutaDocumento);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "Documento guardado correctamente" });
            }
            catch (Exception error)
            {
                return StatusCode(StatusCodes.Status200OK, new { mensaje = error.Message });
            }
        }


    }
}
