using Autofac;
using Autofac.Extensions.DependencyInjection;
using Projet.Application.Autofacs;
using Projet.Infrastructure.Autofacs;

namespace Projet.API.Extensions;

public static class ContainerExtensions
{
    // Les modules passent après builder.Services : en cas de double enregistrement, le leur gagne.
    public static IHostBuilder UseAutofacContainer(this IHostBuilder host) =>
        host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(container =>
            {
                container.RegisterModule<InfrastructureModule>();
                container.RegisterModule<ApplicationModule>();
            });
}
