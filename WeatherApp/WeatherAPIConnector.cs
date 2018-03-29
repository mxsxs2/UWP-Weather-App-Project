using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using WeatherApp.DataModel;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json;

namespace WeatherApp
{
    public class DataObject
    {
        public string Name { get; set; }
    }



    class WeatherAPIConnector
    {
        //Api key to connect to openweathermap.org
        private const string apiKey = "fcc608c1525a74963b9fac6089af8515";
        //The openweathermap api link
        private const string URL = "https://api.openweathermap.org/data/2.5/";


        /// <summary>
        /// Makes a new http cal to the weather server with the given query string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queries"></param>
        /// <returns></returns>
        private async Task<T> DoQueryAsync<T>(string remoteFile,string queries)
        {

            //System.Diagnostics.Debug.WriteLine(URL + remoteFile + queries + "&units=metric&APPID=" + apiKey);

            //Create a http client and add the base url
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL+ remoteFile);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // Do the http response call with the query string and api key
            HttpResponseMessage response = client.GetAsync(queries+ "&units=metric&APPID=" + apiKey).Result;
            //If the response code is 200
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("asd");
                //Convert the response to string
                string resp=await response.Content.ReadAsStringAsync();
                //Serialize the json string into T type
                T data = JsonConvert.DeserializeObject<T>(resp);
                //Return the data
                return data;

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            //Throw an exception as the reposne was not 200
            throw new Exception(response.ReasonPhrase);
        }


        public async void GetWetaherForCitiesAsync(List<City> cities,Func<CityWeatherList, bool> successCallBack, Func<string, bool> errorCallBack)
        {
            //Base query string for the request
            string queryString = "?id=";
            //Get all the city ids
            foreach(City c in cities)
            {
                //Add the id to the string
                queryString += c.id+",";
            }

            try
            {   
                //Do the call to the server and pass it to a callback
                successCallBack(await this.DoQueryAsync<CityWeatherList>("group",queryString.TrimEnd(',')));
            }
            catch(Exception e)
            {
                //Do the call back with an error
                errorCallBack(e.Message);
            }
            
        }
    }
}
