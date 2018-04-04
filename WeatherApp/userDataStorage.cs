using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.DataModel;
using Windows.Storage;

namespace WeatherApp
{
    public class UserDataStorage
    {
        private string userDataFileName = "userDataFile.json";


        /// <summary>
        /// Saves a new city to the user storage
        /// </summary>
        /// <param name="city"></param>
        public async void AddCity(City city)
        {
            //Create a user data holder
            UserData ud = null;
            //File holder
            StorageFile file = null ;
            
            //If the storage file does not exists yet
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync(this.userDataFileName) == null)
            {
                //Create new user data object
                ud = new UserData();
                //Create a new list
                ud.cities = new List<City>(1);
                //Add the city
                ud.cities.Add(city);

                //Create a new file
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(this.userDataFileName);
            }
            else
            {
                //Load the file
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName);
                //Read the contents of the file
                string fileContent=await Windows.Storage.FileIO.ReadTextAsync(file);
                //Deserialize json 
                ud= JsonConvert.DeserializeObject<UserData>(fileContent);
                //Add the city
                ud.cities.Add(city);
            }

            //If file and user data was created
            if (ud != null && file!=null)
            {
                //Write the user data to the file
                await Windows.Storage.FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(ud));
            }
        }

        /// <summary>
        /// Loads the user data json file from the application storage
        /// </summary>
        /// <param name="callBack"></param>
        public async void LoadData(Func<UserData,bool> callBack)
        {
            //Create a user data holder
            UserData ud = null;
            //File holder
            StorageFile file = null;
            System.Diagnostics.Debug.WriteLine(ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName));
            //If the storage file does not exists yet
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync(this.userDataFileName) == null)
            {
                //Create new user data object
                ud = new UserData();
                //Create a new list
                ud.cities = new List<City>(0);
            }
            else
            {
                //Load the file
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName);
                //Read the contents of the file
                string fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
                //Deserialize json 
                ud = JsonConvert.DeserializeObject<UserData>(fileContent);
            }
            //Do the callback
            callBack(ud);
        }

        /// <summary>
        /// Saves a a city's full data to the storage
        /// </summary>
        /// <param name="city"></param>
        public async void SaveCity(City city)
        {
            System.Diagnostics.Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);

           //Load the file
           StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName);
            //Read the contents of the file
            string fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
            //Deserialize json 
            UserData ud = JsonConvert.DeserializeObject<UserData>(fileContent);
            //Find the city
            for (int i = 0; i<ud.cities.Count; i++)
            {
                //If the two keys match then the city was found
                if (ud.cities[i].Key == city.Key)
                {
                    //Override the city data
                    ud.cities[i] = city;
                    break;
                }
            }

            this.SaveUserData(ud);
        }

        /// <summary>
        /// Saves the user data into the storage file. If the file does not exists, the function will create one.
        /// </summary>
        /// <param name="ud"></param>
        private async void SaveUserData(UserData ud)
        {
            //File holder
            StorageFile file = null;

            //If the storage file does not exists yet
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync(this.userDataFileName) == null)
            {
                //Create a new file
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(this.userDataFileName);
            }
            else
            {
                //Load the file
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName);
            }

            //If file and user data was created
            if (ud != null && file != null)
            {
                //Write the user data to the file
                await Windows.Storage.FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(ud));
            }
        }
    }
}
