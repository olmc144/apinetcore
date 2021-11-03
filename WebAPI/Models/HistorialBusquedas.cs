using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class HistorialBusquedas
    {
        public int id { get; set; }
        public string nombre_ciudad { get; set; }
        public string codigo_pais { get; set; }
        public string info { get; set; }
        public int cantidad_s { get; set; }
        public DateTime fecha_consulta { get; set; }
    }
}
