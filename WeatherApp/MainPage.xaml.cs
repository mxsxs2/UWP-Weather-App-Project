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
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
        //Weather api conncetor
        private WeatherAPIConnector WAC = new WeatherAPIConnector();

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

        private RelativePanel CreatePanelForTile(Windows.UI.Xaml.UIElement content)
        {
            //Create new panel
            RelativePanel panel = new RelativePanel();
            panel.Height = 200;
            panel.Width = 200;
            panel.BorderThickness = new Thickness(2);
            panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
            panel.Children.Add(content);
            return panel;
        }

        /// <summary>
        /// Adds the city adding tile to the list of cities
        /// </summary>
        private void AddCityAddingTile()
        {
            //Create tile content
            TextBlock tb = new TextBlock();
            tb.Text = "Add";
            tb.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            tb.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

            //Create new panel
            RelativePanel panel = this.CreatePanelForTile(tb);
            panel.Tapped += this.rlpAddLocation_Tapped;
            spCities.Children.Add(panel);
        }

        /// <summary>
        /// Adds all items form a list of cities to the city tile holder panel. 
        /// </summary>
        /// <param name="cities"></param>
        private void AddLocationTiles(List<City> cities)
        {

            //Load the city data
            this.WAC.GetWetaherForCitiesAsync(cities,
                (cityList) => {
                    //Clear the storage stack panel
                    spCities.Children.Clear();
                    //Loop the cityes
                    foreach (CityWeather city in cityList.list)
                    {


                        /*
                         <RelativePanel Height="200
                           " Width="200">
            <TextBlock FontSize="20" FontWeight="Bold" >Name</TextBlock>
                <Image Height="60" Width="60" Margin="30,70" Source="ms-appx:///Assets/icons/01d.png"/>
                <TextBlock FontSize="40" FontWeight="Bold" Margin="100,70,30,70">9°C</TextBlock>
            <TextBlock FontSize="19" FontWeight="Bold" Margin="7,-50,7,0" RelativePanel.AlignBottomWithPanel="True"> Max 19°C / Min 10°C</TextBlock>
        </RelativePanel>*/
                        //Create new panel
                        RelativePanel panel = new RelativePanel();
                        panel.Height = 200;
                        panel.Width = 200;
                        panel.BorderThickness = new Thickness(2);
                        panel.BorderBrush = new SolidColorBrush(Colors.Aqua);

                        //Create tile content
                        //Add name
                        //Add temperature
                        TextBlock tbn = new TextBlock();
                        tbn.Text = city.name;
                        tbn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
                        tbn.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                        tbn.FontSize = 22;
                        tbn.FontWeight = FontWeights.Bold;
                        panel.Children.Add(tbn);
                        

                        //Add weather picture
                        Image img = new Image();
                        img.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/"+city.weather[0].icon+".png"));
                        img.Margin = new Thickness(30, 65,0,0);
                        img.Width = 60;
                        img.Height = 60;
                        panel.Children.Add(img);
                        //Add temperature
                        TextBlock tb = new TextBlock();
                        tb.Text = Math.Round(city.main.temp)+ "°C";
                        tb.Margin = new Thickness(90, 60, 10, 60);
                        tb.FontSize = 40;
                        tb.FontWeight = FontWeights.Bold;
                        panel.Children.Add(tb);
                        //Add temperature max min
                        TextBlock tbmn = new TextBlock();
                        tbmn.Text = "Max "+city.main.temp_max+"°C / Min " + city.main.temp_min + "°C";
                        tbmn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
                        tbmn.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                        tbmn.Margin = new Thickness(7, -50, 7, 0);
                        tbmn.FontSize = 19;
                        tbmn.FontWeight = FontWeights.Bold;
                        panel.Children.Add(tbmn);

                        //Add mouse hover effect
                        panel.PointerEntered += Panel_PointerEntered;
                        panel.PointerExited += Panel_PointerExited;
                        //Add to the parent stack panel
                        spCities.Children.Add(panel);
                    }
                    this.AddCityAddingTile();
                    return true;
                },
                (error)=>
                {
                    //Create tile content
                    TextBlock tb = new TextBlock();
                    tb.Text = "Could not load weather data";
                    tb.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
                    tb.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);

                    //Create new panel
                    RelativePanel panel = this.CreatePanelForTile(tb);
                    //Add to the parent stack panel
                    spCities.Children.Add(panel);
                    return true;
                });
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
