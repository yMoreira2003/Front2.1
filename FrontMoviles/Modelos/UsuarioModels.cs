using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Modelos/UsuarioModels.cs
using System.Text.Json.Serialization;

namespace FrontMoviles.Modelos
{


    #region Modelos Base de Usuario

    // Request Models
    public class ReqInsertarUsuario
    {
        [JsonPropertyName("Usuario")]
        public Usuario Usuario { get; set; }
    }

    public class Usuario
    {
        [JsonPropertyName("UsuarioId")]
        public int UsuarioId { get; set; } = 0; // Para nuevos usuarios

        [JsonPropertyName("Provincia")]
        public Provincia Provincia { get; set; }

        [JsonPropertyName("Canton")]
        public Canton Canton { get; set; }


        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("Apellido1")]
        public string Apellido1 { get; set; }

        [JsonPropertyName("Apellido2")]
        public string Apellido2 { get; set; }

        [JsonPropertyName("FechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [JsonPropertyName("Correo")]
        public string Correo { get; set; }

        [JsonPropertyName("FotoPerfil")]
        public string FotoPerfil { get; set; } = "";

        [JsonPropertyName("Telefono")]
        public string Telefono { get; set; }

        [JsonPropertyName("Direccion")]
        public string Direccion { get; set; } = "";

        [JsonPropertyName("Contrasena")]
        public string Contrasena { get; set; }

        [JsonPropertyName("Salt")]
        public string Salt { get; set; } = "";

        [JsonPropertyName("Verificacion")]
        public int Verificacion { get; set; } = 0;

        [JsonPropertyName("Activo")]
        public bool Activo { get; set; } = true;

        [JsonPropertyName("PerfilCompleto")]
        public bool PerfilCompleto { get; set; } = false;

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class Provincia
    {
        [JsonPropertyName("ProvinciaId")]
        public int ProvinciaId { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class Canton
    {
        [JsonPropertyName("CantonId")]
        public int CantonId { get; set; }

        [JsonPropertyName("Provincia")]
        public Provincia Provincia { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

   

    // Response Models
    public class ResInsertarUsuario
    {
        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos de Login

    public class ReqLoginUsuario
    {
        [JsonPropertyName("Usuario")]
        public UsuarioLogin Usuario { get; set; }
    }

    public class UsuarioLogin
    {
        [JsonPropertyName("Correo")]
        public string Correo { get; set; }

        [JsonPropertyName("Contrasena")]
        public string Contrasena { get; set; }
    }

    public class ResLoginUsuario
    {
        [JsonPropertyName("sesion")]
        public Sesion Sesion { get; set; }

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    public class Sesion
    {
        [JsonPropertyName("SesionId")]
        public string SesionId { get; set; }

        [JsonPropertyName("Activo")]
        public bool Activo { get; set; }

        [JsonPropertyName("FechaCreacion")]
        public string FechaCreacion { get; set; } // Cambiar a string para evitar problemas de deserialización

        [JsonPropertyName("FechaCierre")]
        public string FechaCierre { get; set; } // Cambiar a string para evitar problemas de deserialización

        [JsonPropertyName("Token")]
        public string Token { get; set; }

        // Propiedades auxiliares para obtener las fechas como DateTime si es necesario
        public DateTime? FechaCreacionDateTime
        {
            get
            {
                if (DateTime.TryParse(FechaCreacion, out DateTime fecha))
                    return fecha;
                return null;
            }
        }

        public DateTime? FechaCierreDateTime
        {
            get
            {
                if (DateTime.TryParse(FechaCierre, out DateTime fecha))
                    return fecha;
                return null;
            }
        }
    }

    #endregion

    #region Modelos de Verificación

    public class ReqVerificacion
    {
        [JsonPropertyName("Correo")]
        public string Correo { get; set; }

        [JsonPropertyName("Verificacion")]
        public int Verificacion { get; set; }
    }

    public class ResVerificacion
    {
        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos para Obtener Usuario (Perfil)

    public class ReqObtenerUsuario
    {
        [JsonPropertyName("Usuario")]
        public UsuarioObtener Usuario { get; set; }
    }

    public class UsuarioObtener
    {
        [JsonPropertyName("UsuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("Correo")]
        public string Correo { get; set; }

        [JsonPropertyName("Provincia")]
        public Provincia Provincia { get; set; }

        [JsonPropertyName("Canton")]
        public Canton Canton { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("Apellido1")]
        public string Apellido1 { get; set; }

        [JsonPropertyName("Apellido2")]
        public string Apellido2 { get; set; }

        [JsonPropertyName("FechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [JsonPropertyName("FotoPerfil")]
        public string FotoPerfil { get; set; }

        [JsonPropertyName("Telefono")]
        public string Telefono { get; set; }

        [JsonPropertyName("Direccion")]
        public string Direccion { get; set; }

        [JsonPropertyName("Contrasena")]
        public string Contrasena { get; set; }

        [JsonPropertyName("Salt")]
        public string Salt { get; set; }

        [JsonPropertyName("Verificacion")]
        public int Verificacion { get; set; }

        [JsonPropertyName("Activo")]
        public bool Activo { get; set; }

        [JsonPropertyName("PerfilCompleto")]
        public bool PerfilCompleto { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ResObtenerUsuario
    {
        [JsonPropertyName("Usuario")]
        public Usuario Usuario { get; set; }

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos de Categorías

    public class Categoria
    {
        [JsonPropertyName("CategoriaId")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ResListarCategorias
    {
        [JsonPropertyName("Categorias")]
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos de SubCategorías

    public class SubCategoria
    {
        [JsonPropertyName("SubCategoriaId")]
        public int SubCategoriaId { get; set; }

        [JsonPropertyName("Categoria")]
        public Categoria Categoria { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ResListarSubCategorias
    {
        [JsonPropertyName("SubCategorias")]
        public List<SubCategoria> SubCategorias { get; set; } = new List<SubCategoria>();

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos de Servicios

    public class Servicio
    {
        [JsonPropertyName("ServicioId")]
        public int ServicioId { get; set; }

        [JsonPropertyName("Usuario")]
        public Usuario Usuario { get; set; }

        [JsonPropertyName("Categoria")]
        public Categoria Categoria { get; set; }

        [JsonPropertyName("Titulo")]
        public string Titulo { get; set; }

        [JsonPropertyName("Descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("Precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("Disponibilidad")]
        public string Disponibilidad { get; set; }

        [JsonPropertyName("SubCategorias")]
        public List<SubCategoria> SubCategorias { get; set; } = new List<SubCategoria>();

        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ReqInsertarServicio
    {
        [JsonPropertyName("SesionId")]
        public string SesionId { get; set; }

        [JsonPropertyName("Servicio")]
        public Servicio Servicio { get; set; }
    }

    public class ResInsertarServicio
    {
        [JsonPropertyName("ServicioId")]
        public int ServicioId { get; set; }

        [JsonPropertyName("Mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion

    #region Modelos Auxiliares

    // Error Item Model
    public class ErrorItem
    {
        [JsonPropertyName("ErrorCode")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }

    // Modelo auxiliar para la UI de subcategorías
    public class SubCategoriaSeleccionada
    {
        public int SubCategoriaId { get; set; }
        public string Nombre { get; set; }
        public bool IsSelected { get; set; }
        public int CategoriaId { get; set; }
    }

    #endregion

    #region Modelos de Ubicación Corregidos

    // Modelos de Provincias
    public class ResListarProvincias
    {
        [JsonPropertyName("Provincias")]
        public List<Provincia> Provincias { get; set; } = new List<Provincia>();

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    // Modelos de Cantones
    public class ResListarCantones
    {
        [JsonPropertyName("Cantones")]
        public List<Canton> Cantones { get; set; } = new List<Canton>();

        [JsonPropertyName("resultado")]
        public bool Resultado { get; set; }

        [JsonPropertyName("error")]
        public List<ErrorItem> Error { get; set; } = new List<ErrorItem>();
    }

    #endregion


}