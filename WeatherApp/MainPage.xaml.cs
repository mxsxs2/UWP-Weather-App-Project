using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WeatherApp.DataModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WeatherApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //List of all cities
        private List<City> cities = new List<City>();
        //List of the cities which the user wants
        private List<City> userCities = new List<City>();
        //Data storage class
        private UserDataStorage userDataStorage = new UserDataStorage();

        public MainPage()
        {
            this.InitializeComponent();
            //Load every city from csv
            this.LoadCities();
            //Load user data from data storage
            this.LoadUserData();
        }

        /// <summary>
        /// Loads the user specific data from the local storage
        /// </summary>
        private void LoadUserData()
        {
            //Load the user data
            this.userDataStorage.LoadData((data)=> {
                //STore the cities to memory
                this.userCities = data.cities;
                //Load the city tiles from the user data
                this.AddLocationTiles(this.userCities);
                return true;
            });
            
            
        }

        /// <summary>
        /// Adds the city adding tile to the list of cities
        /// </summary>
        private void AddCityAddingTile()
        {
            //Create new panel
            RelativePanel panel = new RelativePanel();
            panel.Height = 200;
            panel.Width = 200;
            panel.BorderThickness = new Thickness(2);
            panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
            TextBlock tb = new TextBlock();
            tb.Text = "Add";
            tb.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            tb.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);
            panel.Children.Add(tb);
            panel.Tapped += this.rlpAddLocation_Tapped;
            spCities.Children.Add(panel);
        }

        /// <summary>
        /// Adds all items form a list of cities to the city tile holder panel. 
        /// </summary>
        /// <param name="cities"></param>
        private void AddLocationTiles(List<City> cities)
        {
            //Clear the storage stack panel
            spCities.Children.Clear();
            //Loop the cityes
            foreach(City city in cities)
            {
                //Create new panel
                RelativePanel panel = new RelativePanel();
                panel.Height = 200;
                panel.Width = 200;
                panel.BorderThickness = new Thickness(2);
                panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
                //Add a text block into the relative panel 
                TextBlock tb =new TextBlock();
                tb.Text = city.name + "," + city.countrycode;
                tb.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
                tb.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);
                panel.Children.Add(tb);
                //Add mouse hover effect
                panel.PointerEntered += Panel_PointerEntered;
                panel.PointerExited += Panel_PointerExited;
                //Add to the parent stack panel
                spCities.Children.Add(panel);
            }

            this.AddCityAddingTile();
        }

        /// <summary>
        /// Sets the background of the city tile to white and the cursor to the original arrow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RelativePanel p = (RelativePanel)sender;
            p.Background= new SolidColorBrush(Colors.White);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
        /// <summary>
        /// Sets the background of the city tile to aqua and the cursor to the hand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            RelativePanel p = (RelativePanel)sender;
            p.Background = new SolidColorBrush(Colors.Aqua);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        /// <summary>
        /// Loads every city from a a csv file and adds the to the global city list
        /// </summary>
        public async void LoadCities()
        {
            //CsvParse.cs file from https://github.com/matthiasxc/uwp-kickstart to parse the file
            //Stream str =  new FileStream("data/city-list.scv", FileMode.Open, FileAccess.Read);
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/city_list.csv"));
            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (WeatherApp.CsvFileReader csvReader = new WeatherApp.CsvFileReader(classicStream))
            {
                //Create a new empty city holder
                City city = null;
                WeatherApp.CsvRow row = new WeatherApp.CsvRow();
                while (csvReader.ReadRow(row))
                {
                    //Create an empty city
                    city = new City();

                    // add the columns one at a time
                    for (int i = 0; i < row.Count; i++)
                    {
                        
                        //Parse the data into the city
                        if (i == 0)
                        {
                            city.id = Int32.Parse(row[i]);
                        }
                        else if(i == 1)
                        {
                            city.name = row[i];
                        }
                        else if (i == 2)
                        {
                            city.latitude = Double.Parse(row[i]);
                        }
                        else if (i == 3)
                        {
                            city.longitude = Double.Parse(row[i]);
                        }
                        else if (i == 4)
                        {
                            city.countrycode = row[i];
                        }
                        
                    }
                    
                    //Add to the list
                    this.cities.Add(city);
                }
            }
        }

        /// <summary>
        /// Opens up the city choser fly out when the add button is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rlpAddLocation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Load the cities into the list
            lvCities.ItemsSource = this.cities;
            fpFlyoutDetails.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Adds the tapped city to the users city storage and closes the fly out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Add the city to the user data storage
            this.userDataStorage.AddCity(this.cities[lvCities.SelectedIndex]);
            //Add city to the memory storage
            this.userCities.Add(this.cities[lvCities.SelectedIndex]);
            //Rerender the user city tiles
            this.AddLocationTiles(this.userCities);
            //Close the fylout
            this.btnFlyoutClose_Tapped(fpFlyoutDetails, e);
        }

        /// <summary>
        /// Closes the fly out when the close button is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFlyoutClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Hide the fly out
            fpFlyoutDetails.Visibility = Visibility.Collapsed;
        }
    }
}
