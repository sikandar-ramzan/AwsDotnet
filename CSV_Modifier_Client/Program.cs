using Amazon.DynamoDBv2;
using CSV_Modifier_Client.Services;

namespace CSV_Modifier_Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration setup
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .Build();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<AwsSecretsService>();

            // Inject the IConfiguration instance into the AwsSecretsService
            builder.Services.AddSingleton(configuration);


            //add the aws and dynamo db services
            builder.Services.AddAWSService<IAmazonDynamoDB>();
            builder.Services.AddTransient<DynamoDbService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}