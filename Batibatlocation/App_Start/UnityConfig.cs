using Batibatlocation.Data;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace Batibatlocation
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<ApplicationDbContext>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}