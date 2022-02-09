using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

namespace ApplicationRegistry
{
    public class ApplicationRegistryProgram
    {
        public class Startup
        {

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                services
                    .AddMvcCore()
                    .AddApiExplorer();

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(
						"v1",
						new OpenApiInfo()
						{
                            Version = "v1",
                            Title = "API",
                            Description = ""
						});
                });
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app)
            {
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                });
            }

        }

        public static string Path { get; private set; } = string.Empty;

        public static int Main(string[] args)
        {
            Path = args[0];

            var result = RunDefault();

            if (result != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Running fallback mechanism");
                result = RunFallback();
            }

            return result;

        }


        private static int RunDefault()
        {
            try
            {
                var host = new WebHostBuilder()
                           .ConfigureAppConfiguration((context, builder) =>
                           {
                               var env = context.HostingEnvironment;

                               builder.Sources.Clear();
                               builder.SetBasePath(Directory.GetCurrentDirectory());

                               builder
                                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
                                   .AddEnvironmentVariables();
                           })
                           .UseStartup(typeof(ApplicationRegistryProgram).Assembly.FullName!)
                           .Build();

                var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();

                var swaggerProviderType = swaggerProvider.GetType();
                var swashbuckleVersion = swaggerProviderType.Assembly.GetName().Version?.Major ?? -1;
                using (var writer = new StreamWriter(File.Create(Path)))
                {
                    switch (swashbuckleVersion)
                    {
                        case 6:
                            Swagger6(host, swaggerProvider, swaggerProviderType, writer);
                            break;
                        default:
                            return -2;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        static int RunFallback()
        {
            try
            {
                var host = new WebHostBuilder()
                           .ConfigureAppConfiguration((context, builder) =>
                           {
                               builder.Sources.Clear();
                               builder.SetBasePath(Directory.GetCurrentDirectory());

                               builder
                                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                   .AddEnvironmentVariables();
                           })
                           .UseStartup<ApplicationRegistryProgram.Startup>()
                           .Build();

                var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();

                var swaggerProviderType = swaggerProvider.GetType();
                var swashbuckleVersion = swaggerProviderType.Assembly.GetName().Version?.Major ?? -1;
                using (var writer = new StreamWriter(File.Create(Path)))
                {
                    switch (swashbuckleVersion)
                    {
                        case 6:
                            Swagger6(host, swaggerProvider, swaggerProviderType, writer);
                            break;
                        default:
                            return -2;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }


        private static void Swagger6(IWebHost host, ISwaggerProvider swaggerProvider, Type swaggerProviderType, TextWriter writer)
        {
            var settingMemberInfo = swaggerProviderType.GetMember("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var innerMemberInfo = (settingMemberInfo?[0] as System.Reflection.FieldInfo);

            if(innerMemberInfo == null) {
                throw new ArgumentNullException(nameof(innerMemberInfo));
            }
            
            dynamic settings = (innerMemberInfo.GetValue(swaggerProvider)!);
            var enumerator = settings.SwaggerDocs.Keys.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var key = enumerator.Current;

                var swagger = swaggerProvider.GetSwagger(key);
                var jsonWriter = new OpenApiJsonWriter(writer ?? Console.Out);

                swagger.SerializeAsV2(jsonWriter);
            }
        }
    }
}
