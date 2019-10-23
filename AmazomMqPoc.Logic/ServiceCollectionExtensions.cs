using AmazomMqPoc.Logic.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AmazomMqPoc.Logic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IMessageBroker, MessageBroker>();
            services.AddScoped<IRepository, Repository>();

            return services;
        }
    }
}
