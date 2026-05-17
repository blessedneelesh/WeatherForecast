public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        //// Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}

        // Enable Swagger in all environments for Azure App Service testing
        app.UseSwagger();
        app.UseSwaggerUI();

        // Health check endpoint mapped BEFORE HTTPS redirection
        // This ensures it responds to both HTTP and HTTPS
        app.MapHealthChecks("/health");

        // Only redirect to HTTPS in production/staging, not for health checks
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}