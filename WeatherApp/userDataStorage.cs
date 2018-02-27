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
        private string userDataFileName = "userData.json";

        public async void AddCity(City city)
        {
 

            //Create a user data holder
            UserData ud = null;
            //File holder
            StorageFile file = null ;
            
            //If the storage file does not exists yet
            if (ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName) == null)
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

        public async void LoadData(Func<UserData,bool> callBack)
        {


            //Create a user data holder
            UserData ud = null;
            //File holder
            StorageFile file = null;

            //If the storage file does not exists yet
            if (ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName) == null)
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
    }
}
