using Cantina.Application.Interfaces;
using Cantina.Infrastructure.Persistence.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cantina.Infrastructure.Persistence.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IDishRepository, DishRepository>();
            services.AddScoped<IDrinkRepository, DrinkRepository>();
            return services;
        }
    }
}
