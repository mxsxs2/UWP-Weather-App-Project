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
        private List<City> cities = new List<City>();
        private UserDataStorage userDataStorage = new UserDataStorage();

        public MainPage()
        {
            this.InitializeComponent();
            this.LoadCities();
            this.LoadUserData();
            
        }


        private void LoadUserData()
        {
            //Load the user data
            this.userDataStorage.LoadData((data)=> {
                //Load the city tiles from the user data
                this.AddLocationTiles(data.cities);
                //User data is loaded
                System.Diagnostics.Debug.WriteLine(data.cities.Count);
                return true;
            });
            
            
        }


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

        private void AddLocationTiles(List<City> cities)
        {


            //Loop the cityes
            foreach(City city in cities)
            {
                //Create new panel
                RelativePanel panel = new RelativePanel();
                panel.Height = 200;
                panel.Width = 200;
                panel.BorderThickness = new Thickness(2);
                panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
                TextBlock tb =new TextBlock();
                tb.Text = city.name + "," + city.countrycode;
                tb.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
                tb.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);
                panel.Children.Add(tb);
                spCities.Children.Add(panel);

                /*
                < RelativePanel x: Name = "rlpAddLocation" Height = "200" Width = "200" BorderBrush = "{ThemeResource AppBarBorderThemeBrush}" BorderThickness = "2" >
          
                          < TextBlock Text = "Add"  RelativePanel.AlignHorizontalCenterWithPanel = "True" RelativePanel.AlignVerticalCenterWithPanel = "True" />
               
                           </ RelativePanel >*/
            }

            this.AddCityAddingTile();
        }

        public async void LoadCities()
        {

            

            // then user the CsvParse.cs file from https://github.com/matthiasxc/uwp-kickstart to parse the file
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

        private void rlpAddLocation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Load the cities into the list
            lvCities.ItemsSource = this.cities;
            fpFlyoutDetails.Visibility = Visibility.Visible;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            

            //Add the city to the user data storage
            this.userDataStorage.AddCity(this.cities[lvCities.SelectedIndex]);
            //Close the fylout
            this.btnFlyoutClose_Tapped(fpFlyoutDetails, e);



        }


        private void btnFlyoutClose_Tapped(object sender, TappedRoutedEventArgs e)
        {

            fpFlyoutDetails.Visibility = Visibility.Collapsed;
        }
    }
}
