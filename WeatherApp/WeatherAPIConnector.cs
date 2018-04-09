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
using WeatherApp.DataModel.Forecast5Day;

namespace WeatherApp
{
    public class DataObject
    {
        public string Name { get; set; }
    }



    class WeatherAPIConnector
    {
        //Api key to connect to accuweather.com
        private const string apiKey = "SxRlaYFLOygGB1Ium763sVMxQyPACAli";
        //The openweathermap api link
        private const string URL = "http://dataservice.accuweather.com/";


        /// <summary>
        /// Makes a new http cal to the weather server with the given query string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queries"></param>
        /// <returns></returns>
        private async Task<T> DoQueryAsync<T>(string remoteFile,string queries)
        {

            System.Diagnostics.Debug.WriteLine(URL + remoteFile +"?"+ queries + "&apikey=" + apiKey);

            //Create a http client and add the base url
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL+ remoteFile);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // Do the http response call with the query string and api key
            HttpResponseMessage response = client.GetAsync("?"+queries+ "&metric=true&apikey=" + apiKey).Result;
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

        public async void GetCityListByAutoComplete(string search, Func<List<City>, bool> successCallBack, Func<string, bool> errorCallBack)
        {
            try
            {
                //Do the call to the server and pass it to a callback
                successCallBack(await this.DoQueryAsync<List<City>>("locations/v1/cities/autocomplete", "q="+search));
            }
            catch (Exception e)
            {
                //Do the call back with an error
                errorCallBack(e.Message);
            }
        }





        /// <summary>
        /// Loads the weather information for a city
        /// </summary>
        /// <param name="cityKey"></param>
        /// <param name="successCallBack"></param>
        /// <param name="errorCallBack"></param>
        public async void GetWetaherForCityAsync(string cityKey,Func<CityWeather, bool> successCallBack, Func<string, bool> errorCallBack)
        {
            try
            {
                //Do the call to the server and pass it to a callback
                successCallBack((await this.DoQueryAsync<List<CityWeather>>("currentconditions/v1/locationKey", "locationKey="+cityKey))[0]);
            }
            catch(Exception e)
            {
                //Do the call back with an error
                errorCallBack(e.Message);
            }
            
        }
        /// <summary>
        /// Loads 5 day forecast for a city
        /// </summary>
        /// <param name="cityKey"></param>
        /// <param name="successCallBack"></param>
        /// <param name="errorCallBack"></param>
        public async void Get5DayForecastForCityAsync(string cityKey, Func<Forecast5Day, bool> successCallBack, Func<string, bool> errorCallBack)
        {
            try
            {
                //Do the call to the server and pass it to a callback
                successCallBack((await this.DoQueryAsync<Forecast5Day>("forecasts/v1/daily/5day/" + cityKey, "locationKey=" + cityKey)));
            }
            catch (Exception e)
            {
                //Do the call back with an error
                errorCallBack(e.Message);
            }
        }
    }
}
