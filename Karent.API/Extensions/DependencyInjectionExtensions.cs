using Karent.DataAccess.Interfaces;
using Karent.DataAccess.NativeQuery;
using Karent.DataAccess.ORM;

namespace Karent.API.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services)
        {
            services.AddScoped<IDACar, DACarOrm>();
            services.AddScoped<IDARental, DARentalOrm>();
            services.AddScoped<IDARentalReturn, DARentalReturnOrm>();
            services.AddScoped<IDAUser, DAUserOrm>();

            services.AddScoped<IDACar, DACarNativeQuery>();
            services.AddScoped<IDARental, DARentalNativeQuery>();
            services.AddScoped<IDARentalReturn, DARentalReturnNativeQuery>();
            services.AddScoped<IDAUser, DAUserNativeQuery>();

            return services;
        }
    }
}
