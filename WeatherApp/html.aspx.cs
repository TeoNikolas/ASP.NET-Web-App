using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web.UI.WebControls;
using System.Linq;
using System.Diagnostics;
using System.Web.UI;
using System.Web.SessionState;
namespace WeatherApp
{
    public partial class html : System.Web.UI.Page
    {
        // Loaded the 1st time when the app started

        protected async void Page_Load(object sender, EventArgs e)
        {
            mainDiv.Attributes.Add("style", $"background-image: url('{GetBackgroundImageUrl()}')");

            try
            {
                if (!IsPostBack)
                {
                    // This block will only execute during the first load of the page
                    List<string> initialLocations = new List<string> { "Helsinki" };
                    Session["locations"] = initialLocations;
                    Session["AddedHours"] = 0;
                    Session["NextHourClicked"] = false;
                    await FetchAndDisplayWeatherData();
                    DisplayPreviousLocations();
                }
                else
                {
                    // This block will execute on PostBacks (like button clicks)
                    if (Session["NextHourClicked"] != null)
                    {
                        // You can put logic related to NextHourClicked here
                        Session["NextHourClicked"] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLabel.Text = $"An error occurred: {ex.Message}";
            }
        }

        //Calling API
        private async Task FetchAndDisplayWeatherData()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string baseUri = "https://opendata.fmi.fi/wfs";
                string query = "?service=WFS&version=2.0.0&request=getFeature&storedquery_id=ecmwf::forecast::surface::cities::timevaluepair&place=helsinki&parameters=Temperature,WindUMS,WindVMS,Humidity";
                HttpResponseMessage response = await httpClient.GetAsync($"{baseUri}{query}");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    XElement xmlData = XElement.Parse(content);
                    Session["xmlData"] = xmlData;
                    HandleXMLData(xmlData);
                }
                else
                {
                    LogLabel.Text = $"Failed to fetch data: {response.StatusCode}";
                }
            }
        }

        protected async void SearchIcon_Click(object sender, EventArgs e)
        {
            string newLocation = LocationInput.Value;

            if (string.IsNullOrEmpty(newLocation))
            {
               
                ScriptManager.RegisterStartupScript(this, GetType(), "alertMessage", "alert('Please enter a location.');", true);
            }
            else
            {
                AddLocation(newLocation);
            }
            if (Session["xmlData"] != null)
            {
                HandleXMLData((XElement)Session["xmlData"]);
            }
            else
            {
                await FetchAndDisplayWeatherData();
            }
        }
        private void DisplayPreviousLocations()
        {
            List<string> locations = Session["locations"] as List<string>;
            if (locations != null)
            {
                // Display the unique capitalized locations
                List<string> uniqueCapitalizedLocations = locations
                    .Where(location => char.IsUpper(location[0]))
                    .Distinct()
                    .ToList();

                LocationRepeater.DataSource = uniqueCapitalizedLocations;
                LocationRepeater.DataBind();
            }
        }
        private void AddLocation(string newLocation)
        {
            List<string> locations = Session["locations"] as List<string>;

            if (locations == null)
            {
                locations = new List<string>();
            }

            //Add the new locations at the top of the list
            locations.Insert(0, char.ToUpper(newLocation[0]) + newLocation.Substring(1));

           
            // Remove duplicates and ensure that only unique locations are stored
            locations = locations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();


            while (locations.Count > 4)
            {
                locations.RemoveAt(locations.Count - 1);
            }

           
            // Save the updated list to the session
            Session["locations"] = locations;

            // Refresh the history display
            DisplayPreviousLocations();
        }
        protected async void btnNextDays_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("button was clicked");
            Session["NextHourClicked"] = true;
            int addedHours = (int)Session["AddedHours"];
            addedHours += 1;
            Session["AddedHours"] = addedHours;
            if (Session["xmlData"] != null)
            {
                HandleXMLData((XElement)Session["xmlData"]);
                
            }
            else
            {
                await FetchAndDisplayWeatherData();
            }
        }



        private void HandleXMLData(XElement xmlData)
        {
            
            bool isNextHourClicked = (Session["NextHourClicked"] != null) ? (bool)Session["NextHourClicked"] : false;

            if (Session["NextHourClicked"] != null)
            {
                isNextHourClicked = (bool)Session["NextHourClicked"];
                Session["NextHourClicked"] = false;
              
            }
            Debug.WriteLine("isNextHourClicked: " + isNextHourClicked);
            List<string> locations = Session["locations"] as List<string>;

            // Common logic for setting user location
            string userLocation = isNextHourClicked ? (locations?.LastOrDefault() ?? "Helsinki") : LocationInput.Value;
            if (string.IsNullOrEmpty(userLocation))
            {
                userLocation = "Helsinki";
            }
            else 
            {
                userLocation = locations[0];
            }

            var locationElements = xmlData.Descendants().Where(x => 
            x.Name.LocalName == "name"
            && string.Equals(x.Value, userLocation, StringComparison.OrdinalIgnoreCase) // ignore Case Sensitive Locations
            ).ToList();
            Debug.WriteLine("locationElement: ", locationElements);
            Location.Text = locationElements.Any() ? locationElements.First().Value : "Helsinki";
            LocationLabel2.Text = locationElements.Any() ? locationElements.First().Value : "Helsinki";

            if (!isNextHourClicked && locationElements.Any())
            {
                Session["AddedHours"] = 0;
                // Add to session if the location exists
                if (locations != null && !locations.Contains(userLocation))
                {
                    locations.Add(userLocation);
                    Session["locations"] = locations;
                }
                // Update the display
                DisplayPreviousLocations();
                string current = DateTime.Now.ToString("yyyy-MM-ddTHH:00:00Z");
                Dictionary<string, List<string>> measurements = new Dictionary<string, List<string>>();
                string[] parameters = { "Temperature", "WindUMS", "WindVMS", "Humidity" };
                foreach (var element in locationElements)
                {
                    XElement parentElement = element.Ancestors().FirstOrDefault(x => x.Name.LocalName == "LocationCollection");
                    XAttribute idAttribute = parentElement?.Attribute(XName.Get("id", "http://www.opengis.net/gml/3.2"));

                    if (idAttribute != null)
                    {
                        string targetPart = string.Join("-", idAttribute.Value.Split('-').Skip(2).Take(2));

                        foreach (var param in parameters)
                        {
                            string newId = $"mts-{targetPart}-{param}";
                            Debug.WriteLine($"Looking for measurementElement with newId: {newId}");
                            XElement measurementElement = xmlData.Descendants().FirstOrDefault(x => x.Name.LocalName == "MeasurementTimeseries" && x.Attribute(XName.Get("id", "http://www.opengis.net/gml/3.2"))?.Value == newId);
                            // Debug.WriteLine($"Looking for measurementElement ,,,,,,,,,,,,,,,,,,,,,,: {measurementElement}");
                            if (measurementElement != null)
                            {
                                var ls = measurementElement.Descendants().Where(x => x.Name.LocalName == "point").SelectMany(point =>
                                {
                                    bool foundMatchedPair = false;
                                    var timeElement = point.Descendants().FirstOrDefault(x => x.Name.LocalName == "time");
                                    string dateData = timeElement.Value.ToString();
                                    var valueElement = point.Descendants().FirstOrDefault(x => x.Name.LocalName == "value");
                                    if (timeElement != null && valueElement != null && dateData == current)
                                    {
                                        foundMatchedPair = true;
                                        if (foundMatchedPair)
                                        {
                                            Debug.WriteLine("timeElement Value: ", timeElement.Value);
                                            Debug.WriteLine("current: ", current);
                                            Debug.WriteLine("value returned: ", valueElement.Value);
                                            DateTime now = DateTime.Now;  // This will give you the current local system time
                                            string formattedNow = now.ToString("HH:mm - dddd, dd MMM yy");
                                            Date.Text = formattedNow;
                                            DateLabel2.Text = now.ToString("HH:mm");
                                        }
                                        return new[] { valueElement.Value };
                                    }

                                    return new string[0];
                                }).ToList();
                                Debug.WriteLine($"ls contains: {string.Join(", ", ls)}");

                                measurements[param] = ls;
                                Debug.WriteLine("measurements: ", measurements[param]);
                            }
                        }
                    }
                }
                if (!locationElements.Any())
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alertMessage", "alert('Location name not found');", true);
                }
                else
                {
                    UpdateUI(measurements);
                }
            }
            if (isNextHourClicked && locationElements.Any())
            {
                // Add to session if the location exists
                if (locations != null && !locations.Contains(userLocation))
                {
                    locations.Add(userLocation);
                    Session["locations"] = locations;
                }
                // Update the display
                DisplayPreviousLocations();
                int addedHours = (int)Session["AddedHours"];
                DateTime now = DateTime.Now;
                DateTime futureTime = now.AddHours(addedHours);
                string futureTimeString = futureTime.ToString("yyyy-MM-ddTHH:00:00Z");
                
                Dictionary<string, List<string>> measurements = new Dictionary<string, List<string>>();
                string[] parameters = { "Temperature", "WindUMS", "WindVMS", "Humidity" };
                foreach (var element in locationElements)
                {
                    XElement parentElement = element.Ancestors().FirstOrDefault(x => x.Name.LocalName == "LocationCollection");
                    XAttribute idAttribute = parentElement?.Attribute(XName.Get("id", "http://www.opengis.net/gml/3.2"));

                    if (idAttribute != null)
                    {
                        string targetPart = string.Join("-", idAttribute.Value.Split('-').Skip(2).Take(2));

                        foreach (var param in parameters)
                        {
                            string newId = $"mts-{targetPart}-{param}";
                            Debug.WriteLine($"Looking for measurementElement with newId: {newId}");
                            XElement measurementElement = xmlData.Descendants().FirstOrDefault(x => x.Name.LocalName == "MeasurementTimeseries" && x.Attribute(XName.Get("id", "http://www.opengis.net/gml/3.2"))?.Value == newId);
                            // Debug.WriteLine($"Looking for measurementElement ,,,,,,,,,,,,,,,,,,,,,,: {measurementElement}");
                            if (measurementElement != null)
                            {
                                var ls = measurementElement.Descendants().Where(x => x.Name.LocalName == "point").SelectMany(point =>
                                {
                                    bool foundMatchedPair = false;
                                    var timeElement = point.Descendants().FirstOrDefault(x => x.Name.LocalName == "time");
                                    string dateData = timeElement.Value.ToString();
                                    var valueElement = point.Descendants().FirstOrDefault(x => x.Name.LocalName == "value");
                                    if (timeElement != null && valueElement != null && dateData == futureTimeString)
                                    {
                                        foundMatchedPair = true;
                                        if (foundMatchedPair)
                                        {
                                            Debug.WriteLine("timeElement Value: ", timeElement.Value);
                                            
                                            Debug.WriteLine("value returned: ", valueElement.Value);
                                          //  DateTime now = DateTime.Now;  // This will give you the current local system time
                                            string formattedNow = futureTime.ToString("HH:mm - dddd, dd MMM yy");
                                            Date.Text = formattedNow;
                                            DateLabel2.Text = futureTime.ToString("HH:mm");
                                        }
                                        return new[] { valueElement.Value };
                                    }

                                    return new string[0];
                                }).ToList();
                                Debug.WriteLine($"ls contains: {string.Join(", ", ls)}");

                                measurements[param] = ls;
                                Debug.WriteLine("measurements: ", measurements[param]);
                            }
                        }
                    }
                }
                if (!locationElements.Any())
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alertMessage", "alert('Location name not found');", true);
                }
                else
                {
                    UpdateUI(measurements);
                }
            }     
        }

        public string GetBackgroundImageUrl()
        {
            var hour = DateTime.Now.Hour;

            if (hour >= 6 && hour < 12)
            {
                // Morning image
                return "public/6-12.jpg";
            }
            else if (hour >= 12 && hour < 18)
            {
                // Afternoon image
                return "public/12-18.jpg";
            }
          
            else
            {
                // Night image
                return "public/18-6.jpg";
            }
        }


        private void UpdateUI(Dictionary<string, List<string>> measurements)
        {
            if (measurements.ContainsKey("Temperature"))
            {
                Temperature.Text = $"{string.Join(", ", measurements["Temperature"])}";
            }

            if (measurements.ContainsKey("Humidity"))
            {
                Humidity.Text = $"{string.Join(", ", measurements["Humidity"].Select(x => x + "%"))}";
            }

            if (measurements.ContainsKey("WindUMS") && measurements.ContainsKey("WindVMS"))
            {
                var windUmsValues = measurements["WindUMS"].Select(double.Parse).ToList();
                var windVmsValues = measurements["WindVMS"].Select(double.Parse).ToList();
                var temperatureValues = measurements["Temperature"].Select(double.Parse).ToList();

                if (windUmsValues.Count > 0 && windVmsValues.Count > 0)
                {
                    var windSpeeds = windUmsValues.Zip(windVmsValues, (u, v) => Math.Round(Math.Sqrt(u * u + v * v))).ToList();
                    WindSpeed.Text = $"{string.Join(", ", windSpeeds)} km/h";

                    // Lấy giá trị đầu tiên từ danh sách windSpeeds
                    var firstWindSpeed = windSpeeds.FirstOrDefault();

                    // Tính chỉ số nhiệt độ cảm nhận (Wind Chill)
                    var windChill = Math.Round(13.12 + 0.6215 * temperatureValues.First() - 11.37 * Math.Pow(firstWindSpeed, 0.16) + 0.3965 * temperatureValues.First() * Math.Pow(firstWindSpeed, 0.16));

                    // Hiển thị giá trị Wind Chill
                    WindChill.Text = $" {windChill} °C";
                }
            }
        }

    }
}