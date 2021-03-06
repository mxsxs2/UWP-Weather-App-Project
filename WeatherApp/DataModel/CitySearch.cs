﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.DataModel.Forecast12Hours;

namespace WeatherApp.DataModel
{
    public class City
    {
        public int Version { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public int Rank { get; set; }
        public string LocalizedName { get; set; }
        public Country Country { get; set; }
        public AdministrativeArea AdministrativeArea { get; set; }
        public CityWeather Weather { get; set; }
        public Forecast5Day.Forecast5Day Forecast5Day { get; set; }
        public List<HourlyForecast> Forecast12Hours { get; set; }
        public DateTime LastUpdated { get; set; }
    }


    public class Country
    {
        public string ID { get; set; }
        public string LocalizedName { get; set; }
    }

    public class AdministrativeArea
    {
        public string ID { get; set; }
        public string LocalizedName { get; set; }
    }
}
