using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;


namespace WeatherApp
{
    class Location
    {
        public static async void GetLocation(Func<Geoposition, bool> successCallBack, Func<string, bool> errorCallBack)
        {
            //Get the location status
            var accessStatus = await Geolocator.RequestAccessAsync();
            //Check the status
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    //_rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator {};

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    //Do the success callback
                    successCallBack(pos);
                    break;

                case GeolocationAccessStatus.Denied:
                    errorCallBack("Access to location is denied.");
                    break;

                case GeolocationAccessStatus.Unspecified:
                    errorCallBack("Unsfecified error happend.");
                    break;
            }

        }


    }
}
