using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

public static class WeatherService
{
    public static async Task<XElement> GetWeatherDataAsync(string apiUrl)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                return XElement.Parse(content);
            }
        }
        return null;
    }
}
