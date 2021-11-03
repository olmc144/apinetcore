using System;

namespace WebAPI
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 422 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
