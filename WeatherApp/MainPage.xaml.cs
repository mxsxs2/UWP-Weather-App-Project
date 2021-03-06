﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using WeatherApp.DataModel;
using WeatherApp.DataModel.Forecast12Hours;
using WeatherApp.DataModel.Forecast5Day;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
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
        #region ClassMembers
        //List of all cities
        private List<City> cities = new List<City>();
        //List of the cities which the user wants
        private List<City> userCities = new List<City>();
        //Data storage class
        private UserDataStorage userDataStorage = new UserDataStorage();
        //Weather api conncetor
        private WeatherAPIConnector WAC = new WeatherAPIConnector();
        //City For settings
        private int cityIndexForSettings=-1;
        #endregion

        #region InitializationAndPointOfEntry
        public MainPage()
        {
            this.InitializeComponent();
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
                //If there is atleast one user city
                if (this.userCities.Count() > 0)
                {
                    //Show the labels
                    this.ToggleLabels();
                }

                return true;
            });
            
            
        }
        #endregion

        #region CityTiles
        /// <summary>
        /// Adds the city adding tile to the list of cities
        /// </summary>
        private void AddCityAddingTile()
        {
            //Add city adding icon
            Image img = new Image
            {
                Source = new BitmapImage(new System.Uri("ms-appx:///Assets/add.png")),
                Width = 100,
                Height = 100
            };
            img.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            img.SetValue(RelativePanel.AlignVerticalCenterWithPanelProperty, true);
            //Create new panel
            RelativePanel panel = new RelativePanel
            {
                Height = 200,
                Width = 200,
                Margin = new Thickness(5),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Aqua)
            };
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

            //Counter for the number of cities
            int index = 0;
            //Loop al the cities in the list
            foreach(City city in cities)
            {

                //Show the loading image
                imgLoading.Visibility = Visibility.Visible;
                //Hide connection error image
                imgConnectionError.Visibility = Visibility.Collapsed;
                //Load the weather data for the city
                this.WAC.GetWetaherForCityAsync(city.Key,
                    (weather) =>
                    {
                        //Set the weather for the city
                        city.Weather = weather;
                        //Add timestamp to the city
                        city.LastUpdated = DateTime.Now;
                        //Add the tile
                        this.AddCityTile(city,index);
                        //If it is the first city then we need the forecast as well
                        if (index==0)
                        {
                            //Load forecast for the default city. This will save to the storage as well and load the hourly forecast too
                            this.LoadForecastForCity(city);
                        }
                        else
                        {
                            //Save the city to the storage
                            this.userDataStorage.SaveCity(city);
                        }

                        //Hide the loading image
                        imgLoading.Visibility = Visibility.Collapsed;
                        return true;
                    },
                    (error) =>
                    {
                        //Add the tile with the old data
                        this.AddCityTile(city,index);
                        //If it is the first city then we need the forecast as well
                        if (index == 0)
                        {
                            //Load forecast for the default city. This will save to the storage as well
                            this.LoadForecastForCity(city);

                        }
                        //Show connection error image
                        imgConnectionError.Visibility = Visibility.Visible;

                        //Hide the loading image
                        imgLoading.Visibility = Visibility.Collapsed;
                        return true;
                    });
                //Increase the counter
                index++;
            }
            //Add the city adding tile
            this.AddCityAddingTile();
        }

        /// <summary>
        /// Adds a new city tile to the ui
        /// </summary>
        /// <param name="city"></param>
        /// <param name="index"></param>
        private void AddCityTile(City city,int index)
        {
            //Create new panel
            RelativePanel panel = new RelativePanel
            {
                Height = 200,
                Width = 200,
                Margin = new Thickness(5),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Aqua),
                Name = city.Key + ""
            };

            //Create tile content
            //Add name
            TextBlock tbn = new TextBlock
            {
                Text = city.LocalizedName,
                FontSize = 22,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold

            };
            tbn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            panel.Children.Add(tbn);


            //Add weather picture
            Image img = new Image
            {
                Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + city.Weather.WeatherIcon + ".png")),
                Margin = new Thickness(30, 65, 0, 0),
                Width = 60,
                Height = 60
            };
            panel.Children.Add(img);
            //Add temperature
            TextBlock tb = new TextBlock
            {
                Text = Math.Round(city.Weather.Temperature.Metric.Value) + "°C",
                Margin = new Thickness(90, 60, 10, 60),
                FontSize = 40,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            panel.Children.Add(tb);
            //Add temperature max min
            TextBlock tbmn = new TextBlock
            {
                Text = city.Weather.WeatherText,
                Margin = new Thickness(7, -50, 7, 0),
                FontSize = 19,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            tbmn.SetValue(RelativePanel.AlignHorizontalCenterWithPanelProperty, true);
            tbmn.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
            panel.Children.Add(tbmn);


            //Add settings picture
            Image imgCh = new Image
            {
                Source = new BitmapImage(new System.Uri("ms-appx:///Assets/change.png")),
                Width = 20,
                Height = 20,
                Name = index + ""
            };
            imgCh.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
            imgCh.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
            imgCh.Tapped += ImgCh_Tapped;
            panel.Children.Add(imgCh);


            //Add mouse hover effect
            panel.PointerEntered += City_Tile_Panel_PointerEntered;
            panel.PointerExited += City_Tile_Panel_PointerExited;
            //Add tap event to the panel
            panel.Tapped += City_Tile_Panel_Tapped;
            //Add to the parent stack panel
            spCities.Children.Add(panel);
        }
        #endregion

        #region ForecastLoaders
        /// <summary>
        /// Makes the forecast labels and the last updated time visible
        /// </summary>
        private void ToggleLabels()
        {
            //If they are hidden
            if (stplDailyForecastLabel.Visibility == Visibility.Collapsed)
            {
                stplDailyForecastLabel.Visibility = Visibility.Visible;
                stplHourlyForecastLabel.Visibility = Visibility.Visible;
                stplLastUpdated.Visibility = Visibility.Visible;
            }
            else
            {
                stplDailyForecastLabel.Visibility = Visibility.Collapsed;
                stplHourlyForecastLabel.Visibility = Visibility.Collapsed;
                stplLastUpdated.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Loads 5 days and 12 hours forecast for city
        /// </summary>
        /// <param name="city"></param>
        private void LoadForecastForCity(City city)
        {
            //Set the loading image to visible
            imgLoading.Visibility = Visibility.Visible;
            //Set the forecast city name
            tblForecastCityName.Text = city.LocalizedName;
            //Hide connection error image
            imgConnectionError.Visibility = Visibility.Collapsed;
            //Load the city data
            this.WAC.Get5DayForecastForCityAsync(city.Key,
                (forecast) => {
                    //Clear the storage grid
                    grdForecastDays.Children.Clear();
                    //Add the forecast to the city
                    city.Forecast5Day = forecast;
                    //Add timestamp to the city
                    city.LastUpdated = DateTime.Now;
                    //Loop the days
                    for (int i = 0; i < city.Forecast5Day.DailyForecasts.Count; i++)
                    {
                        this.AddDayPanel(city.Forecast5Day.DailyForecasts[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();

                    //Load the hourly forecast. This will save the city to the storage
                    this.LoadHourlyForecastForCity(city);

                    //Save the city to the storage. The previous method will save it
                    //this.userDataStorage.SaveCity(city);
                    //Hide the loading image
                    imgLoading.Visibility = Visibility.Collapsed;
                    return true;
                },
                (error) =>
                {
                    //Clear the storage grid
                    grdForecastDays.Children.Clear();
                    //Loop the days and update with the old data
                    for (int i = 0; i < city.Forecast5Day.DailyForecasts.Count; i++)
                    {
                        this.AddDayPanel(city.Forecast5Day.DailyForecasts[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();
                    //Show connection error image
                    imgConnectionError.Visibility = Visibility.Visible;
                    //Hide the loading image
                    imgLoading.Visibility = Visibility.Collapsed;
                    return false;
                });
        }

        /// <summary>
        /// Loads the hourly forecast for a specific city
        /// </summary>
        /// <param name="cityKey"></param>
        private void LoadHourlyForecastForCity(City city)
        {
            //Set the loading image to visible
            imgLoading.Visibility = Visibility.Visible;
            //Set the forecast city name
            tblForecastCityNameHours.Text = city.LocalizedName;
            //Hide connection error image
            imgConnectionError.Visibility = Visibility.Collapsed;
            //Load the city data
            this.WAC.Get12HourForecastForCityAsync(city.Key,
                (forecast) => {
                    //Clear the storage grid
                    grdForecastHours.Children.Clear();
                    //Add the forecast to the city
                    city.Forecast12Hours = forecast;
                    //Add timestamp to the city
                    city.LastUpdated = DateTime.Now;
                    //Loop the days
                    for (int i = 0; i < city.Forecast12Hours.Count; i++)
                    {
                        this.AddHourPanel(city.Forecast12Hours[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();
                    //Save the city to the storage
                    this.userDataStorage.SaveCity(city);
                    //Hide the loading image
                    imgLoading.Visibility = Visibility.Collapsed;
                    return true;
                },
                (error) =>
                {
                    //Clear the storage grid
                    grdForecastHours.Children.Clear();
                    //Loop the days and update with the old data
                    for (int i = 0; i < city.Forecast12Hours.Count; i++)
                    {
                        this.AddHourPanel(city.Forecast12Hours[i], i);
                    }
                    //Update the ui with the last updated time
                    tblForecastLastUpdated.Text = city.LastUpdated.ToString();
                    //Show connection error image
                    imgConnectionError.Visibility = Visibility.Visible;
                    //Hide the loading image
                    imgLoading.Visibility = Visibility.Collapsed;
                    return true;
                });
        }
        #endregion

        #region ForecastLayout
        /// <summary>
        /// Adds a day panel into the forecast grid
        /// </summary>
        /// <param name="day"></param>
        /// <param name="col"></param>
        private void AddHourPanel(HourlyForecast hour, int col)
        {

            //Get day name from epoch time
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(hour.EpochDateTime).ToLocalTime();
            //Create day name
            TextBlock tbn = new TextBlock
            {
                Text = dtDateTime.Day + "/" + dtDateTime.Hour + ":00",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            grdForecastHours.Children.Add(tbn);
            Grid.SetColumn(tbn, col);
            Grid.SetRow(tbn, 0);

            //Weather Picture
            Image img = new Image
            {
                Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + hour.WeatherIcon + ".png")),
                Width = 60,
                Height = 60
            };
            grdForecastHours.Children.Add(img);
            Grid.SetColumn(img, col);
            Grid.SetRow(img, 1);
            //Night
            TextBlock tbni = new TextBlock
            {
                Text = hour.IconPhrase,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 15,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            grdForecastHours.Children.Add(tbni);
            Grid.SetColumn(tbni, col);
            Grid.SetRow(tbni, 2);


            //Add temperature
            TextBlock tbt = new TextBlock
            {
                Text = Math.Round(hour.Temperature.Value) + "°C",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 25,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            grdForecastHours.Children.Add(tbt);
            Grid.SetColumn(tbt, col);
            Grid.SetRow(tbt, 3);
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
            TextBlock tbn = new TextBlock
            {
                Text = dtDateTime.DayOfWeek.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            grdForecastDays.Children.Add(tbn);
            Grid.SetColumn(tbn, col);
            Grid.SetRow(tbn, 0);

            //Create stackpanels for day/night pictures
            StackPanel panel = new StackPanel();
            StackPanel dayPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            StackPanel nightPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(dayPanel);
            panel.Children.Add(nightPanel);
            grdForecastDays.Children.Add(panel);
            Grid.SetColumn(panel, col);
            Grid.SetRow(panel, 1);
            //Day
            TextBlock tbd = new TextBlock
            {
                Text = "Day  ",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            dayPanel.Children.Add(tbd);
            //Day Picture
            Image img = new Image
            {
                Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + day.Day.Icon + ".png")),
                Width = 60,
                Height = 60
            };
            dayPanel.Children.Add(img);
            //Night
            TextBlock tbni = new TextBlock
            {
                Text = "Night",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            nightPanel.Children.Add(tbni);
            //DayPicture
            Image img2 = new Image();
            img2.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/icons/" + day.Night.Icon + ".png"));
            img2.Width = 60;
            img2.Height = 60;
            nightPanel.Children.Add(img2);

            //Min/Max temperatures
            //Add temperature
            TextBlock tbt = new TextBlock
            {
                Text = "Min " + Math.Round(day.Temperature.Minimum.Value) + "°C / Max " + Math.Round(day.Temperature.Maximum.Value) + "°C",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 15,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold
            };
            grdForecastDays.Children.Add(tbt);
            Grid.SetColumn(tbt, col);
            Grid.SetRow(tbt, 2);
        }
        #endregion

        #region EventHandlers
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
        /// Opens up the city choser fly out when the add button is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rlpAddLocation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Load the cities into the list
            lvCities.ItemsSource = this.cities;
            fpCitySearch.Visibility = Visibility.Visible;
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
            System.Diagnostics.Debug.WriteLine("tapped");

            //Add the city to the user data storage
            //this.userDataStorage.AddCity(this.cities[lvCities.SelectedIndex]);
            this.userDataStorage.SaveCity(this.cities[lvCities.SelectedIndex]);
            //Add city to the memory storage
            this.userCities.Add(this.cities[lvCities.SelectedIndex]);
            //Rerender the user city tiles
            this.AddLocationTiles(this.userCities);
            //Close the fylout
            this.btnFlyoutClose_Tapped(fpCitySearch, e);
            //If there this is the first city added
            if (this.userCities.Count() == 1)
            {
                //Show the labels
                this.ToggleLabels();
            }
        }

        /// <summary>
        /// Closes the fly out when the close button is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFlyoutClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Hide the fly out
            fpCitySearch.Visibility = Visibility.Collapsed;
        }

        private async void tbxCitySearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //Hide error message
            tblLocationDisabledMessage.Visibility = Visibility.Collapsed;
            //Hide search error message
            tblNoSearchResult.Visibility = Visibility.Collapsed;
            //Hide unknown error message
            tblUnknownError.Visibility = Visibility.Collapsed;
            //Hide connection error image
            imgConnectionError.Visibility = Visibility.Collapsed;
            //Cast the sender
            TextBox tb = (TextBox)sender;
            //Dont search unles there is atleast three characters
            if (tb.Text.Length > 2)
            {
                //Set the loading image to visible
                imgLoading.Visibility = Visibility.Visible;
                //Run in a different thread so no blocking
                await System.Threading.Tasks.Task.Run(async () =>
                {
                    //Cast the sender
                    String searchstring="";
                    //Has to get hold of the textbox from the UI
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        searchstring = tb.Text;
                    });
                        //Load the cities
                        WAC.GetCityListByAutoComplete(searchstring,
                        (Cities) => {
                            //Do the callback in UI thread
                            IAsyncAction a = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                //Set the new city list
                                this.cities = Cities;
                                //Update the view
                                lvCities.ItemsSource = Cities;
                                //Hide the loading image
                                imgLoading.Visibility = Visibility.Collapsed;
                            });
                            return true;
                        },
                        (error) =>
                        {
                            //Do the callback in UI thread
                            IAsyncAction a = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                System.Diagnostics.Debug.WriteLine(error);
                                //Hide the list view
                                lvCities.Visibility = Visibility.Collapsed;
                                //Show unknown error message
                                tblUnknownError.Visibility = Visibility.Visible;
                                //Hide the loading image
                                imgLoading.Visibility = Visibility.Collapsed;
                                //Show connection error image
                                imgConnectionError.Visibility = Visibility.Visible;
                            });
                            return true;
                        });

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

        /// <summary>
        /// Toggles the tile setting flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgCh_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //If the flyout is not visible yet
            if (fpTileSettings.Visibility != Visibility.Visible)
            {
                //Get the absolute position of the clicked cog
                var ttv = ((Image)sender).TransformToVisual(Window.Current.Content);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));
                //Set the position of the flyout
                fpTileSettings.Margin = new Thickness(screenCoords.X, screenCoords.Y + 20, 10, 10);
                fpTileSettings.Visibility = Visibility.Visible;
                //Set the city for settings
                this.cityIndexForSettings = Convert.ToInt32(((Image)sender).Name);
            }
            else
            {
                //Close the flyout
                this.tblClose_Tapped(sender, e);
            }
        }

        /// <summary>
        /// Hides the settings flyout presenter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            fpTileSettings.Visibility = Visibility.Collapsed;
            //Remove the city for settings
            this.cityIndexForSettings = -1;
        }

        /// <summary>
        /// Sets the current sity as default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblSetDeafult_Tapped(object sender, TappedRoutedEventArgs e)
        {
           
            //Set the city as default
            this.userDataStorage.SetCityAsDefault(this.userCities[this.cityIndexForSettings]);
            //Get a backup of the city
            City tmp = this.userCities[this.cityIndexForSettings];
            //Remove from the current position
            this.userCities.RemoveAt(this.cityIndexForSettings);
            //Add to the front
            this.userCities.Insert(0, tmp);
            //Re do the tiles
            this.AddLocationTiles(this.userCities);
            //Close the fylout
            this.tblClose_Tapped(sender, e);
        }

        /// <summary>
        /// Ask for confirmation of city removal from the user's preferred list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tblRemove_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog("Would you like to remove "+this.userCities[this.cityIndexForSettings].LocalizedName+"?");
            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand("Yes",new UICommandInvokedHandler(this.RemoveConfirmation_Tapped)));
            messageDialog.Commands.Add(new UICommand("Close",new UICommandInvokedHandler(this.RemoveConfirmation_Tapped)));
            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;
            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;
            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        /// <summary>
        /// If the yes was tapped in the confirmation block it removes the city
        /// </summary>
        /// <param name="command"></param>
        private void RemoveConfirmation_Tapped(IUICommand command)
        {
            //If yes was selected
            if (command.Label == "Yes")
            {
                //Remove the city
                this.userDataStorage.RemoveCity(this.userCities[this.cityIndexForSettings]);
                //Remove from the list
                this.userCities.RemoveAt(this.cityIndexForSettings);
                //Re do the tiles
                this.AddLocationTiles(this.userCities);
                //If there is nothing left in thelist
                if (this.userCities.Count() < 1)
                {
                    this.ToggleLabels();
                    //Clear the forecast
                    grdForecastDays.Children.Clear();
                    grdForecastHours.Children.Clear();
                }
                else
                {
                    //Load the forecast for the default city
                    this.LoadForecastForCity(this.userCities[0]);
                }
            }

            //Close the flyout
            fpTileSettings.Visibility = Visibility.Collapsed;
            //Remove the city for settings
            this.cityIndexForSettings = -1;
        }

        /// <summary>
        /// Sets the cursor to the hand when the pointer enters the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsText_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        /// <summary>
        /// Sets the cursor to the original arrow when the pointer leves the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsText_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        /// <summary>
        /// Sets the background of the city tile to white and the cursor to the original arrow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void City_Tile_Panel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RelativePanel p = (RelativePanel)sender;
            p.Background = new SolidColorBrush(Colors.Transparent);
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
        /// Tries to use the users location do get a city list from weather api
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUseLocation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Hide error message
            tblLocationDisabledMessage.Visibility = Visibility.Collapsed;
            //Hide search error message
            tblNoSearchResult.Visibility = Visibility.Collapsed;
            //Hide unknown error message
            tblUnknownError.Visibility = Visibility.Collapsed;
            //Hide connection error image
            imgConnectionError.Visibility = Visibility.Collapsed;
            //Set the loading image to visible
            imgLoading.Visibility = Visibility.Visible;
            //Run in a different thread so no blocking
            await System.Threading.Tasks.Task.Run(() =>
            {
                //Get the location  
                Location.GetLocation(
                (pos) =>
                {
                    //Load a city from accu Weather
                    this.WAC.FindCityByCoordinates(pos,
                        (result) =>
                        {

                            //Convert the result to City
                            City city = new City
                            {
                                Version = result.Version,
                                Key = result.Key,
                                Type = result.Type,
                                Rank = result.Rank,
                                LocalizedName = result.LocalizedName,
                                AdministrativeArea = new AdministrativeArea {
                                    ID = result.AdministrativeArea.ID,
                                    LocalizedName = result.AdministrativeArea.LocalizedName
                                },
                                Country = new Country {
                                    ID = result.Country.ID,
                                    LocalizedName = result.Country.LocalizedName
                                }
                            };

                            //Do the callback in UI thread
                            IAsyncAction a = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                //Add the city to the list
                                this.cities = new List<City>
                                {
                                    city
                                };
                                //Show the list view
                                lvCities.Visibility = Visibility.Visible;
                                //Place the city to the search result list view
                                lvCities.ItemsSource = this.cities;
                            });
                            return true;
                        },
                        (err) =>
                        {
                            //Do the callback in UI thread
                            IAsyncAction a = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                //Hide the list view
                                lvCities.Visibility = Visibility.Collapsed;
                                //Show error message
                                tblNoSearchResult.Visibility = Visibility.Visible;
                                //Hide the loading image
                                imgLoading.Visibility = Visibility.Collapsed;
                                //Show connection error image
                                imgConnectionError.Visibility = Visibility.Visible;
                            });
                            return true;
                        });

                    return true;
                },
                (err) =>
                {
                    System.Diagnostics.Debug.WriteLine(err);
                    //Check if it is a permission error or it is an unknonwn error
                    if (err == "Access to location is denied.")
                    {
                        //Show error message
                        tblLocationDisabledMessage.Visibility = Visibility.Visible;
                        //Hide the list view
                        lvCities.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //Show unknown error message
                        tblUnknownError.Visibility = Visibility.Visible;
                        //Show the list view
                        lvCities.Visibility = Visibility.Visible;
                    }
                    //Hide the loading image
                    imgLoading.Visibility = Visibility.Collapsed;

                    return true;
                }
            );

            });
        }
        #endregion
    }
}
