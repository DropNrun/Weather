using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

var app = builder.Build();

HttpClient MeteoClient = new()
{
    BaseAddress = new Uri($"https://api.open-meteo.com")
};
MemoryCacheOptions cacheOptions = new MemoryCacheOptions() { };
MemoryCache cache = new MemoryCache(cacheOptions);

app.MapWhen(context => context.Request.Path == "/", appBuilder =>//"/", appBuilder =>
{ 
    appBuilder.Use(async (context, next) =>
    {
        Console.WriteLine("������� ������: ");
        string latitude = Console.ReadLine();
        Console.WriteLine("������� �������: ");
        string longitude = Console.ReadLine();
        HttpResponseMessage response = await MeteoClient.GetAsync($"/v1/forecast?latitude={latitude} &longitude= {longitude}&current_weather=true");
        
        GettingW serv = new GettingW(context, cache);

        WeatherForecast wForecast = await serv.GetWeather(latitude, longitude, MeteoClient);

        Console.WriteLine($"�����������: { wForecast.current_weather.temperature}\n�������� �����: {wForecast.current_weather.windspeed}");
        
        await next();
    });
});

app.Run();

public class GettingW
{
    HttpContext context;
    private readonly IMemoryCache cache;
    MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) };
    public GettingW(HttpContext cont, IMemoryCache mCache)
    {
        context = cont;
        cache = mCache;
    }

    public GettingW(HttpContext cont)
    {
        context = cont;
    }

    public async Task<WeatherForecast?>GetWeather(string latitude, string longitude, HttpClient MeteoClient)
    {
        landl data = new landl(latitude,longitude);

        cache.TryGetValue(data, out WeatherForecast? w);

        if (w == null)
        {
            WeatherForecast? weather = await MeteoClient.GetFromJsonAsync<WeatherForecast>($"/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true");
            cache.Set(data, weather, cacheOptions);
            Console.WriteLine("������ � �����");
            return weather;

        }
        else
        {
            Console.WriteLine("������ �� ����");
            return w;
        }
    }
}
public record landl (string latitude, string longitude);
public record WeatherForecast(W2Forecast current_weather);
public record W2Forecast(double temperature, double windspeed);

