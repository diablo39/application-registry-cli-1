using System;
using System.IO;
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
        public static int Main(string[] args)
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

                switch (swashbuckleVersion)
                {
                    case 3:
                    case 2:
                        Swagger3(host, swaggerProvider, swaggerProviderType);
                        break;

                    case 4:
                        Swagger4(host, swaggerProvider, swaggerProviderType);
                        break;
                    default:
                        return -2;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static void Swagger4(IWebHost host, ISwaggerProvider swaggerProvider, Type swaggerProviderType)
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
                serializer.Serialize(Console.Out, swagger);

            }
        }

        private static void Swagger3(IWebHost host, ISwaggerProvider swaggerProvider, Type swaggerProviderType)
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
                serializer.Serialize(Console.Out, swagger);

            }
        }
    }
}