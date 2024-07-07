using Services; // Adjust namespace as per your project structure

namespace NewsAggregatorRSSFeed
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            // Register ApiService and RssService in the dependency injection container
            builder.Services.AddTransient<ApiService>();
            builder.Services.AddHttpClient<OpenPageRankService>();

            builder.Services.AddControllersWithViews();

            //https://stackoverflow.com/a/55276201
            //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0
            //+ various links
            var config = builder.Configuration;
            builder.WebHost.ConfigureKestrel(options =>
            {
                var httpUrl = config["Kestrel:Endpoints:Http:Url"];
                if (!string.IsNullOrEmpty(httpUrl))
                {
                    options.ListenAnyIP(new Uri(httpUrl).Port); // Listen on all IP addresses
                }

                var httpsUrl = config["Kestrel:Endpoints:Https:Url"];
                if (!string.IsNullOrEmpty(httpsUrl))
                {
                    options.ListenAnyIP(new Uri(httpsUrl).Port, listenOptions =>
                    {
                        listenOptions.UseHttps(); // Enable HTTPS
                    });
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Apply CORS policy globally
            app.UseCors("AllowSpecificOrigin");

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            Main1();
            app.Run();

        }
        public static async Task Main1()
        {
            var apiKey = "00ccwgoockgc0k8g0s88koggw84ww0w4o4s08k4o";
            var domains = new List<string> { "google.com", "apple.com", "unknowndomain.com" };
            var apiUrl = "https://openpagerank.com/api/v1.0/getPageRank";
        
            var query = $"?domains[]={Uri.EscapeDataString(domains[0])}&domains[]={Uri.EscapeDataString(domains[1])}&domains[]={Uri.EscapeDataString(domains[2])}";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("API-OPR", apiKey);

            var url = apiUrl + query;
        
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request exception: {e.Message}");
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}