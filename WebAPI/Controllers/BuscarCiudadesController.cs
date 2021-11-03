using DevExpress.Utils.About;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuscarCiudadesController : ControllerBase
    {
        //url api estado del tiempo
        private string url_tiempo = "http://api.openweathermap.org";
        private string key_tiempo = "02471af37d4afd26d8307dbe6516e04d";

        //url api de noticias
        private string url_noticias = "https://newsapi.org";
        private string key_noticias = "81d3bfdca0d142a2be4d6704c8c2b067";

        [HttpGet("[action]/{city}")]
        public async Task<IActionResult> City(string city)
        {
            using (var client_tiempo = new HttpClient())
            {
                try
                {
                    client_tiempo.BaseAddress = new Uri(url_tiempo);
                    var response = await client_tiempo.GetAsync($"/data/2.5/weather?q={city}&appid={key_tiempo}&lang=es");
                    response.EnsureSuccessStatusCode();

                    var stringResultTime = await response.Content.ReadAsStringAsync();
                    var rawWeather = JsonConvert.DeserializeObject<CurrentWeather>(stringResultTime);

                    IActionResult data_news = await News(rawWeather.sys.country);                    

                    var data_consol = new Dictionary<string, object>();
                    data_consol.Add("news", data_news);
                    data_consol.Add("current_weather", rawWeather);                    
                    

                    string respuesta = Guardar(rawWeather.sys.country, rawWeather.name, "Busqueda");

                    if (respuesta == "Busqueda Registrada con Exito")
                    {
                        return Ok(data_consol);
                    }
                    else
                    {
                        return BadRequest("Error al insertar busqueda en la base de datos");
                    }                    

                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error al obtener el clima de la API OpenWeather: {httpRequestException.Message}");
                }
            }

        }

        [HttpGet("[action]/{cod_pais}")]
        public async Task<IActionResult> News(string cod_pais)
        {
            using (var newsclient = new HttpClient())
            {
                try
                {
                    newsclient.BaseAddress = new Uri(url_noticias);
                    var response = await newsclient.GetAsync($"/v2/top-headlines?country={cod_pais}&apiKey={key_noticias}");
                    response.EnsureSuccessStatusCode();

                    var stringResultNews = await response.Content.ReadAsStringAsync();
                    var rawNews = JsonConvert.DeserializeObject<NewsT>(stringResultNews);
                    return Ok(rawNews);                   
                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error al obtener el clima de la API newsapi: {httpRequestException.Message}");
                }
            }
        }
        
        private string Guardar(string codigopais, string nombreciudad, string info)
        {
            try
            {
                using (SqlConnection conx = new SqlConnection("Server=LAPTOP-KEO5DT6T;Database=Historial;User Id=sa;Password=motor2021.;"))
                {                    

                    SqlCommand cmd = new SqlCommand("Sp_Insertar_Busqueda", conx);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@op", "INSERTAR");
                    cmd.Parameters.AddWithValue("@codigo_pais", codigopais);
                    cmd.Parameters.AddWithValue("@nombre_ciudad", nombreciudad);
                    cmd.Parameters.AddWithValue("@info", info);

                    conx.Open();

                    cmd.ExecuteNonQuery();
                    conx.Close();
                    return "Busqueda Registrada con Exito";
                }

            }
            catch (Exception ex)
            {                
                return "Error " + ex.Message;
            }
        }

        [Route("Historial")]
        [HttpGet]
        public string Historial()
        {
            try
            {
                using (SqlConnection conx = new SqlConnection("Server=LAPTOP-KEO5DT6T;Database=Historial;User Id=sa;Password=motor2021.;"))
                {
                    List<HistorialBusquedas> model = new List<HistorialBusquedas>();
                    conx.Open();
                    SqlCommand cmd = new SqlCommand("Sp_Insertar_Busqueda", conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@op", "LISTAR");
                    SqlDataReader reads = cmd.ExecuteReader();

                    while (reads.Read())
                    {
                        HistorialBusquedas histrs = new HistorialBusquedas
                        {
                            id = Convert.ToInt32(reads.GetValue(0).ToString()),
                            nombre_ciudad = reads.GetValue(1).ToString(),
                            codigo_pais = reads.GetValue(2).ToString(),
                            info = reads.GetValue(3).ToString(),
                            cantidad_s = Convert.ToInt32(reads.GetValue(4).ToString()),
                            fecha_consulta = Convert.ToDateTime(reads.GetValue(5).ToString())
                        };
                        model.Add(histrs);
                    }

                    conx.Close();
                    return JsonConvert.SerializeObject(model);
                }
            }
            catch (Exception ex)
            {                
                return "Error " + ex.Message;
            }
        }
    }
}
