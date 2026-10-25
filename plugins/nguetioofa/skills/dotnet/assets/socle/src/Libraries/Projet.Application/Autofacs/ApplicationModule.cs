using Autofac;
using FluentValidation;

namespace Projet.Application.Autofacs;

public sealed class ApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Services un par un ; validateurs par balayage, car un validateur oublié laisserait passer une requête en silence.
        builder.RegisterAssemblyTypes(ThisAssembly)
               .AsClosedTypesOf(typeof(IValidator<>))
               .InstancePerLifetimeScope();
    }
}
