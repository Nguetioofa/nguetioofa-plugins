using Autofac;

namespace Projet.Infrastructure.Autofacs;

/// <summary>Services de l'Infrastructure, un par un ; un sujet qui grossit prend son module (<c>PersistenceModule</c>…).</summary>
public sealed class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
    }
}
