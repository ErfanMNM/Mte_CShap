using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TTManager.PDA.MobyDataPDA
{
    

    public class ApiServer
    {
        public static WebApplication? App;

        public static async Task StartAsync(Action<string> onBarcode)
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            App = builder.Build();

            App.MapPost("/api/scan", (MobyDataScanData data) =>
            {
                onBarcode?.Invoke(data.Code);

                return Results.Ok(new
                {
                    Success = true
                });
            });

            await App.StartAsync();
        }
    }
}
