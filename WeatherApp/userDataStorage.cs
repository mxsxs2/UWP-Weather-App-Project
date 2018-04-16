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
    /// <summary>
    /// Class used to store an opened StorageFile and wether it was newly created or not and the user data from the file
    /// </summary>
    public class OpenedFile
    {
        public StorageFile file { get; }
        public bool newFile { get; }
        public UserData userData { get; set; }

        /// <summary>
        /// Constructor for a new pened file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newFile"></param>
        /// <param name="userData"></param>
        public OpenedFile(StorageFile file, bool newFile, UserData userData)
        {
            this.file = file;
            this.newFile = newFile;
            this.userData = userData;
        }
        /// <summary>
        /// Saves the user data to the opened file.
        /// </summary>
        /// <param name="userData"></param>
        public async void Save(UserData userData = null)
        {
            //If there is an opened file and there is any user data
            if (this.file != null && (userData ?? this.userData) != null)
            {
                try
                {
                    //Write the user data to the file
                    await Windows.Storage.FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(userData ?? this.userData));
                }
                catch (Exception e)
                {
                    //Check if it is used by another process
                    if ("System.IO.FileLoadException" == e.GetType().ToString())
                    {
                        //Try to save again
                        this.Save(userData ?? this.userData);
                    }
                    System.Diagnostics.Debug.WriteLine(e.Message + " " + e.GetType());
                }
            }
        }


    }
    /// <summary>
    /// Class to handle user storage
    /// </summary>
    public class UserDataStorage
    {
        //The name of the data file 
        private string userDataFileName = "userDataFile.json";

        /// <summary>
        /// Loads the user data json file from the application storage
        /// </summary>
        /// <param name="callBack"></param>
        public async void LoadData(Func<UserData, bool> callBack)
        {
            //Run in a different thread so no blocking
            await System.Threading.Tasks.Task.Run(async () =>
            {
                //Open the file
                OpenedFile of = await this.GetFile();

                //Do the callback in UI thread
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    callBack(of.userData);
                });

            });
        }

        /// <summary>
        /// Gets the storage file. If it is not created yet then it will create one
        /// </summary>
        /// <returns></returns>
        private async Task<OpenedFile> GetFile()
        {
            System.Diagnostics.Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
            //File holder
            StorageFile file = null;
            //Wether it is a new file or not
            bool newFile = false;
            //The user data to return
            UserData ud = null;
            //If the storage file does not exists yet
            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync(this.userDataFileName) == null)
            {
                //Create a new file
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(this.userDataFileName);
                //Set the flag as the file is new
                newFile = true;
                //Create new user data object
                ud = new UserData
                {
                    //Create a new list
                    cities = new List<City>(0)
                };

            }
            else
            {
                //Load the file
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(this.userDataFileName);
                //Read the contents of the file
                string fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
                //Deserialize json 
                ud = JsonConvert.DeserializeObject<UserData>(fileContent);
                //Check if the file is empty
                if (ud == null)
                {
                    //Create new user data object
                    ud = new UserData
                    {
                        //Create a new list
                        cities = new List<City>(0)
                    };
                }
                //Check if the city list is null
                else if(ud.cities==null)
                {
                    //Create a new list
                    ud.cities = new List<City>(0);
                }

            }
            return new OpenedFile(file, newFile, ud);
        }

        /// <summary>
        /// Removes a city from the storage
        /// </summary>
        /// <param name="city"></param>
        public async void RemoveCity(City city)
        {
            //Run in a different thread so no blocking
            await System.Threading.Tasks.Task.Run(async () =>
            {
                //Open the file
                OpenedFile of = await this.GetFile();
                //Check if the city exists in the list
                int index = this.FindCity(of.userData, city);
                //If it exists
                if (index != -1)
                {
                    //Remove the city
                    of.userData.cities.RemoveAt(index);
                    //Save the user data
                    of.Save();
                }
            });
        }

        /// <summary>
        /// Saves a city as default city by moving it to the first position
        /// </summary>
        /// <param name="city"></param>
        public async void SetCityAsDefault(City city)
        {
            //Run in a different thread so no blocking
            await System.Threading.Tasks.Task.Run(async () => {
                //Open the file
                OpenedFile of = await this.GetFile();
                //Check if the city exists in the list
                int index = this.FindCity(of.userData, city);
                //If it exists
                if (index != -1)
                {
                    //If the two keys match then the city was found
                    if (of.userData.cities[index].Key == city.Key)
                    {
                        //Remove the city
                        of.userData.cities.RemoveAt(index);
                        //Add the city as first item
                        of.userData.cities.Insert(0, city);
                        //Save the user data
                        of.Save();
                    }
                }

            });
        }

        /// <summary>
        /// Saves a city's full data to the storage
        /// </summary>
        /// <param name="city"></param>
        public async void SaveCity(City city)
        {
            //Run in a different thread so no blocking
            await System.Threading.Tasks.Task.Run(async () =>
            {
                //Open the file
                OpenedFile of = await this.GetFile();
                //Check if the city exists in the list
                int index = this.FindCity(of.userData, city);
                //If it exists
                if (index != -1)
                {
                    //Override the city data
                    of.userData.cities[index] = city;
                }
                else
                {
                    //Add the city to the list
                    of.userData.cities.Add(city);
                }

                //Save the user data
                of.Save();
            });
        }

        /// <summary>
        /// Checks if the city exists in the user data. If it does, returns the index of the city otherwise -1
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        private int FindCity(UserData userData, City city)
        {
            //If there is any data to check against
            if (userData != null && userData.cities != null && userData.cities.Count > 0)
            {
                //Find the city
                for (int i = 0; i < userData.cities.Count; i++)
                {
                    //If the two keys match then the city was found
                    if (userData.cities[i].Key == city.Key)
                    {
                        return i;
                    }
                }
            }
            //Not found
            return -1;
        }
    }
}
