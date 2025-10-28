
using BD_Assignment.Services;

namespace BD_Assignment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Register In-Memory Storage as a singleton
            builder.Services.AddSingleton<InMemoryStorage>();

            // Register the Geolocation Service
            builder.Services.AddHttpClient<IGeolocationService, GeolocationService>();

            // Register the Background Service for cleaning up temporal blocks
            builder.Services.AddHostedService<TemporalBlockCleanupService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
