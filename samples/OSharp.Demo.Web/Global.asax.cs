﻿using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Autofac;
using Autofac.Integration.Mvc;

using OSharp.Core;
using OSharp.Core.Data;
using OSharp.Core.Data.Entity;
using OSharp.Core.Data.Entity.Migrations;
using OSharp.Demo.Web.Dtos;
using OSharp.Demo.Web.Logging;
using OSharp.Demo.Web.Services.Impl;
using OSharp.Utility.Logging;


namespace OSharp.Demo.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RoutesRegister();
            DtoMappers.MapperRegister();
            AutofacMvcRegister();
            DatabaseInitialize();
            LoggingInitialize();
        }

        private static void RoutesRegister()
        {
            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "OSharp.Demo.Web.Controllers" });
        }

        private static void AutofacMvcRegister()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>));
            Type baseType = typeof(IDependency);
            Assembly[] assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(Assembly.Load).ToArray();
            assemblies = assemblies.Union(new[] { Assembly.GetExecutingAssembly() }).ToArray();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
                .AsImplementedInterfaces().InstancePerLifetimeScope();//InstancePerLifetimeScope 保证生命周期基于请求

            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void DatabaseInitialize()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            DatabaseInitializer.AddMapperAssembly(assembly);
            CreateDatabaseIfNotExistsWithSeed.SeedActions.Add(new IdentitySeedAction());

            DatabaseInitializer.Initialize();
        }

        private static void LoggingInitialize()
        {
            Log4NetLoggerAdapter adapter = new Log4NetLoggerAdapter();
            LogManager.AddLoggerAdapter(adapter);
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }
    }
}