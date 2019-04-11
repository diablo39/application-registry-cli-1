using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                    c.SwaggerDoc("v1", new Info
                    {
                        Version = "v1",
                        Title = "API",
                        Description = "",
                        TermsOfService = "None"
                    });
                    c.DescribeAllEnumsAsStrings();
                });
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

        public static string Path { get; private set; }

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
                               builder.Sources.Clear();
                               builder.SetBasePath(Directory.GetCurrentDirectory());

                               builder
                                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                   .AddEnvironmentVariables();
                           })
                           .UseStartup(typeof(ApplicationRegistryProgram).Assembly.FullName)
                           .Build();

                var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();

                var swaggerProviderType = swaggerProvider.GetType();
                var swashbuckleVersion = swaggerProviderType.Assembly.GetName().Version.Major;
                using (var writer = new StreamWriter(File.Create(Path)))
                {
                    switch (swashbuckleVersion)
                    {
                        case 3:
                        case 2:
                            Swagger3(host, swaggerProvider, swaggerProviderType, writer);
                            break;

                        case 4:
                            Swagger4(host, swaggerProvider, swaggerProviderType, writer);
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
                var swashbuckleVersion = swaggerProviderType.Assembly.GetName().Version.Major;
                using (var writer = new StreamWriter(File.Create(Path)))
                {
                    switch (swashbuckleVersion)
                    {
                        case 3:
                        case 2:
                            Swagger3(host, swaggerProvider, swaggerProviderType, writer);
                            break;

                        case 4:
                            Swagger4(host, swaggerProvider, swaggerProviderType, writer);
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


        private static void Swagger4(IWebHost host, ISwaggerProvider swaggerProvider, Type swaggerProviderType, TextWriter writer)
        {
            var settingMemberInfo = swaggerProviderType.GetMember("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dynamic settings = ((settingMemberInfo[0] as System.Reflection.FieldInfo).GetValue(swaggerProvider));
            var enumerator = settings.SwaggerDocs.Keys.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var key = enumerator.Current;

                var swagger = swaggerProvider.GetSwagger(key);

                var mvcOptionsAccessor = (IOptions<MvcJsonOptions>)host.Services.GetService(typeof(IOptions<MvcJsonOptions>));

                var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);
                serializer.Serialize(writer ?? Console.Out, swagger);

            }
        }

        private static void Swagger3(IWebHost host, ISwaggerProvider swaggerProvider, Type swaggerProviderType, TextWriter writer)
        {
            var settingsMemberInfo = swaggerProviderType.GetMember("_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            dynamic settings = ((settingsMemberInfo[0] as System.Reflection.FieldInfo).GetValue(swaggerProvider));
            var enumerator = settings.SwaggerDocs.Keys.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var key = enumerator.Current;

                var swagger = swaggerProvider.GetSwagger(key);

                var mvcOptionsAccessor = (IOptions<MvcJsonOptions>)host.Services.GetService(typeof(IOptions<MvcJsonOptions>));

                var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);
                serializer.Serialize(writer ?? Console.Out, swagger);

            }
        }
    }
}