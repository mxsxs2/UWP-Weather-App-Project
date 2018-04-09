using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using WeatherApp.DataModel;
using WeatherApp.DataModel.Forecast5Day;
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
            //this.LoadCities();
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
            //Add city adding icon
            Image img = new Image();
            img.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/add.png"));
            img.Width = 100;
            img.Height = 100;
            img.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            img.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);
            //Create new panel
            RelativePanel panel = new RelativePanel();
            panel.Height = 200;
            panel.Width = 200;
            panel.Margin = new Thickness(5);
            panel.BorderThickness = new Thickness(1);
            panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
            panel.Children.Add(img);
            //Add mouse hover effect
            panel.PointerEntered += City_Tile_Panel_PointerEntered;
            panel.PointerExited += City_Tile_Panel_PointerExited;
            panel.Tapped += this.rlpAddLocation_Tapped;
            spCities.Children.Add(panel);
        }

        /// <summary>
        /// Adds all items form a list of cities to the city tile holder panel. 
        /// </summary>
        /// <param name="cities"></param>
        private void AddLocationTiles(List<City> cities)
        {

            //Clear the tile holder
            spCities.Children.Clear();

            //Flag if the city is the first
            bool firstCity = true;
            //Loop al the cities in the list
            foreach(City city in cities)
            {

                //Load the weather data for the city
                this.WAC.GetWetaherForCityAsync(city.Key,
                    (weather) =>
                    {
                        //Set the weather for the city
                        city.Weather = weather;
                        //Add timestamp to the city
                        city.LastUpdated = DateTime.Now;
                        //Add the tile
                        this.AddCityTile(city);
                        //If it is the first city then we need the forecast as well
                        if (firstCity)
                        {
                            //Load forecast for the default city. This will save to the storage as well
                            this.LoadForecastForCity(city);

                        }
                        else
                        {
                            //Save the city to the storage
                            this.userDataStorage.SaveCity(city);
                        }
                        return true;
                    },
                    (error) =>
                    {
                        //Add the tile with the old data
                        this.AddCityTile(city);

                        System.Diagnostics.Debug.WriteLine(error);
                        return true;
                    });
                //Set the first city flag
                firstCity = false;
            }
            //Add the city adding tile
            this.AddCityAddingTile();
        }

        /// <summary>
        /// Adds a new city tile to the ui
        /// </summary>
        /// <param name="city"></param>
        private void AddCityTile(City city)
        {
            //Create new panel
            RelativePanel panel = new RelativePanel();
            panel.Height = 200;
            panel.Width = 200;
            panel.Margin = new Thickness(5);
            panel.BorderThickness = new Thickness(1);
            panel.BorderBrush = new SolidColorBrush(Colors.Aqua);
            panel.Name = city.Key + "";
            //Create tile content
            //Add name
            //Add temperature
            TextBlock tbn = new TextBlock();
            tbn.Text = city.LocalizedName;
            tbn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            tbn.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
            tbn.FontSize = 22;
            tbn.Foreground = new SolidColorBrush(Colors.White);
            tbn.FontWeight = FontWeights.Bold;
            panel.Children.Add(tbn);


            //Add weather picture
            Image img = new Image();
            img.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + city.Weather.WeatherIcon + ".png"));
            img.Margin = new Thickness(30, 65, 0, 0);
            img.Width = 60;
            img.Height = 60;
            panel.Children.Add(img);
            //Add temperature
            TextBlock tb = new TextBlock();
            tb.Text = Math.Round(city.Weather.Temperature.Metric.Value) + "°C";
            tb.Margin = new Thickness(90, 60, 10, 60);
            tb.FontSize = 40;
            tb.Foreground = new SolidColorBrush(Colors.White);
            tb.FontWeight = FontWeights.Bold;
            panel.Children.Add(tb);
            //Add temperature max min
            TextBlock tbmn = new TextBlock();
            tbmn.Text = city.Weather.WeatherText;
            tbmn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            tbmn.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
            tbmn.Margin = new Thickness(7, -50, 7, 0);
            tbmn.FontSize = 19;
            tbmn.Foreground = new SolidColorBrush(Colors.White);
            tbmn.FontWeight = FontWeights.Bold;
            panel.Children.Add(tbmn);

            //Add mouse hover effect
            panel.PointerEntered += City_Tile_Panel_PointerEntered;
            panel.PointerExited += City_Tile_Panel_PointerExited;
            //Add tap event to the panel
            panel.Tapped += City_Tile_Panel_Tapped;
            //Add to the parent stack panel
            spCities.Children.Add(panel);
        }

        /// <summary>
        /// Loads the forecast for a specific city
        /// </summary>
        /// <param name="cityKey"></param>
        private void LoadForecastForCity(City city)
        {
            //Set the forecast city name
            tblForecastCityName.Text = city.LocalizedName;

            //Load the city data
            this.WAC.Get5DayForecastForCityAsync(city.Key,
                (forecast) => {
                    //Clear the storage grid
                    spForecastDays.Children.Clear();
                    //Add the forecast to the city
                    city.Forecast5Day = forecast;
                    //Add timestamp to the city
                    city.LastUpdated = DateTime.Now;
                    //Loop the days
                    for (int i=0; i<city.Forecast5Day.DailyForecasts.Count; i++)
                    {
                        this.AddDayPanel(city.Forecast5Day.DailyForecasts[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();
                    //Save the city to the storage
                    this.userDataStorage.SaveCity(city);
                    return true;
                },
                (error) =>
                {
                    //Clear the storage grid
                    spForecastDays.Children.Clear();
                    //Loop the days and update with the old data
                    for (int i = 0; i < city.Forecast5Day.DailyForecasts.Count; i++)
                    {
                        this.AddDayPanel(city.Forecast5Day.DailyForecasts[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();
                    return true;
                });
        }

        /// <summary>
        /// Loads the forecast for the given day when a tile is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void City_Tile_Panel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Find City
            for (int i = 0; i < this.userCities.Count; i++)
            {
                //If the city was found
                if (this.userCities[i].Key == ((RelativePanel)sender).Name)
                {
                    this.LoadForecastForCity(this.userCities[i]);
                    break;
                }
            }

                    
        }

        /// <summary>
        /// Adds a day panel into the forecast grid
        /// </summary>
        /// <param name="day"></param>
        /// <param name="col"></param>
        private void AddDayPanel(DailyForecast day, int col)
        {
            
            //Get day name from epoch time
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(day.EpochDate).ToLocalTime();
            //Create day name
            TextBlock tbn = new TextBlock();
            tbn.Text = dtDateTime.DayOfWeek.ToString();
            tbn.HorizontalAlignment = HorizontalAlignment.Center;
            tbn.VerticalAlignment = VerticalAlignment.Center;
            tbn.FontSize = 20;
            tbn.Foreground = new SolidColorBrush(Colors.White);
            tbn.FontWeight = FontWeights.Bold;
            spForecastDays.Children.Add(tbn);
            Grid.SetColumn(tbn, col);
            Grid.SetRow(tbn, 0);

            //Create stackpanels for day/night pictures
            StackPanel panel = new StackPanel();
            StackPanel dayPanel = new StackPanel();
            dayPanel.Orientation = Orientation.Horizontal;
            dayPanel.HorizontalAlignment = HorizontalAlignment.Center;
            StackPanel nightPanel = new StackPanel();
            nightPanel.Orientation = Orientation.Horizontal;
            nightPanel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Children.Add(dayPanel);
            panel.Children.Add(nightPanel);
            spForecastDays.Children.Add(panel);
            Grid.SetColumn(panel, col);
            Grid.SetRow(panel, 1);
            //Day
            TextBlock tbd = new TextBlock();
            tbd.Text = "Day  ";
            tbd.HorizontalAlignment = HorizontalAlignment.Center;
            tbd.VerticalAlignment = VerticalAlignment.Center;
            tbd.FontSize = 18;
            tbd.Foreground = new SolidColorBrush(Colors.White);
            tbd.FontWeight = FontWeights.Bold;
            dayPanel.Children.Add(tbd);
            //Day Picture
            Image img = new Image();
            img.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + day.Day.Icon + ".png"));
            img.Width = 60;
            img.Height = 60;
            dayPanel.Children.Add(img);
            //Night
            TextBlock tbni = new TextBlock();
            tbni.Text = "Night";
            tbni.HorizontalAlignment = HorizontalAlignment.Center;
            tbni.VerticalAlignment = VerticalAlignment.Center;
            tbni.FontSize = 18;
            tbni.Foreground = new SolidColorBrush(Colors.White);
            tbni.FontWeight = FontWeights.Bold;
            nightPanel.Children.Add(tbni);
            //DayPicture
            Image img2 = new Image();
            img2.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + day.Night.Icon + ".png"));
            img2.Width = 60;
            img2.Height = 60;
            nightPanel.Children.Add(img2);

            //Min/Max temperatures
            //Add temperature
            TextBlock tbt = new TextBlock();
            tbt.Text = "Min " + Math.Round(day.Temperature.Minimum.Value) + "°C / Max " + Math.Round(day.Temperature.Maximum.Value) + "°C";
            tbt.HorizontalAlignment = HorizontalAlignment.Center;
            tbt.FontSize = 15;
            tbt.Foreground = new SolidColorBrush(Colors.White);
            tbt.FontWeight = FontWeights.Bold;
            spForecastDays.Children.Add(tbt);
            Grid.SetColumn(tbt, col);
            Grid.SetRow(tbt, 2);
        }


        /// <summary>
        /// Sets the background of the city tile to white and the cursor to the original arrow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void City_Tile_Panel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RelativePanel p = (RelativePanel)sender;
            p.Background= new SolidColorBrush(Colors.Transparent);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }
        /// <summary>
        /// Sets the background of the city tile to aqua and the cursor to the hand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void City_Tile_Panel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            RelativePanel p = (RelativePanel)sender;
            p.Background = new SolidColorBrush(Colors.Aqua);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
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
            //Set the focus to the serach box
            tbxCitySearch.Focus(FocusState.Pointer);
        }

        /// <summary>
        /// Adds the tapped city to the users city storage and closes the fly out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Add the city to the user data storage
            //this.userDataStorage.AddCity(this.cities[lvCities.SelectedIndex]);
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

        private void tbxCitySearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //Cast the sender
            TextBox tb = (TextBox)sender;
            //Dont search unles there is atleast three characters
            if (tb.Text.Length > 2)
            {
                //Load the cities
                WAC.GetCityListByAutoComplete(tb.Text,
                    (Cities) => {
                        //Set the new city list
                        this.cities = Cities;
                        //Update the view
                        lvCities.ItemsSource = Cities;
                        return true;
                    },
                    (error) =>
                    {
                        System.Diagnostics.Debug.WriteLine(error);
                        return true;
                    });
            }
        }
        /// <summary>
        /// Removes placeholder from the city search box 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void tbxCitySearch_GotFocus(object sender, RoutedEventArgs args)
        {
            if(tbxCitySearch.Text.Trim()== "Search City...")
                tbxCitySearch.Text = "";
        }
        /// <summary>
        /// Adds place holder to the city search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void tbxCitySearch_LostFocus(object sender, RoutedEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(tbxCitySearch.Text))
                tbxCitySearch.Text = "Search City...";
        }
    }
}
