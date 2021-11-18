using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConnectionManager.Common.Services;
using ConnectionManager.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ConnectionManager
{
    using Common.Models;
    using ConnectionManager.Services;

    public static class Startup
    {
        private static IConfigurationRoot _configurationRoot;

        //public static IServiceProvider ServiceProvider { get; set; }

        public static void Init()
        {
            //StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;

            //var host = new HostBuilder()
            //            .ConfigureHostConfiguration(c =>
            //            {
            //                // Tell the host configuration where to file the file (this is required for Xamarin apps)
            //                c.AddCommandLine(new string[] { $"ContentRoot={LocalFolder.Path}" });

            //                c.SetBasePath(Package.Current.InstalledLocation.Path)
            //                 .AddJsonFile("appsettings.json", optional: false);
            //            })
            //            .ConfigureServices((c, x) =>
            //            {
            //                // Configure our local services and access the host configuration
            //                ConfigureServices(c, x);
            //            }).
            //            ConfigureLogging(l => l.AddConsole(o =>
            //            {
            //                //setup a console logger and disable colors since they don't have any colors in VS
            //                o.DisableColors = true;
            //            }))
            //            .Build();

            ////Save our service provider so we can use it later.
            //ServiceProvider = host.Services;
        }

        static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            services.AddSingleton<NavigationService>();
            services.AddTransient<ProjectDetailsViewModel>();
            services.AddTransient<CreateEnvironmentViewModel>();
            services.AddTransient<EditEnvironmentViewModel>();
            services.AddTransient<ProjectService>();
            services.AddTransient<EnvironmentService>();
            services.AddTransient<ActivedEnvironmentService>();
            services.AddTransient<EditProjectViewModel>();
            services.AddTransient<CreateProjectViewModel>();
            services.AddAutoMapper(configuration =>
            {
                configuration.CreateMap<EnvironmentDO, Environment>();
                configuration.CreateMap<Environment, EnvironmentDO>();
                configuration.CreateMap<ProjectDO, Project>();
                configuration.CreateMap<Project, ProjectDO>();
            }, typeof(Startup));
        }

        static string ExtractResource(string filename, string location)
        {
            var a = Assembly.GetExecutingAssembly();

            using (var resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream != null)
                {
                    var full = Path.Combine(location, filename);

                    using (var stream = File.Create(full))
                    {
                        resFilestream.CopyTo(stream);
                    }
                }
            }
            return Path.Combine(location, filename);
        }
    }
}
