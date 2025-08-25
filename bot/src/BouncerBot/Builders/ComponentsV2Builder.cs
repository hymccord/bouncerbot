using NetCord;
using NetCord.Rest;

namespace BouncerBot.Builders;
internal class ComponentsBuilderV2 : IStaticComponentContainer
{
    public List<IComponentBuilder> Components { get; } = [];

    public IEnumerable<IComponentProperties> Build()
    {
        return Components.Select(c => c.Build());
    }
}

internal interface IComponentContainer
{
    List<IComponentBuilder> Components { get; }

    IComponentContainer AddComponent(IComponentBuilder component)
    {
        Components.Add(component);
        return this;
    }

    IComponentContainer AddComponents(params ReadOnlySpan<IComponentBuilder> components)
    {
        Components.AddRange(components);
        return this;
    }

    IComponentContainer WithComponents(params ReadOnlySpan<IComponentBuilder> components)
    {
        Components.Clear();
        Components.AddRange(components);

        return this;
    }
}

internal interface IStaticComponentContainer : IComponentContainer
{ }

internal interface IComponentBuilder
{
    IComponentProperties Build();
}

internal class ContainerBuilder : IComponentBuilder, IStaticComponentContainer
{
    public List<IComponentBuilder> Components { get; } = [];

    public IComponentProperties Build()
    {
        return new ComponentContainerProperties();
    }

    public void Build(MessageOptions message)
    {
        message.Components = [
            Build()
        ];
        message.Flags |= MessageFlags.IsComponentsV2;
    }
}

internal static class BuilderExtensions
{
    public static T WithContainer<T>(this T builder, IEnumerable<IComponentBuilder> components)
        where T : class, IStaticComponentContainer
    {
        //builder.WithContainer(new ContainerBuilder().WithComponents(components));

        return builder;
    }
    public static T WithContainer<T>(this T builder, ContainerBuilder containerBuilder)
        where T : class, IStaticComponentContainer
    {
        builder.AddComponent(containerBuilder);
        return builder;
    }

    public static T WithContainer<T>(this T builder, Action<ContainerBuilder> configure)
        where T : class, IStaticComponentContainer
    {
        var containerBuilder = new ContainerBuilder();
        configure(containerBuilder);

        return builder.WithContainer(containerBuilder);
    }
}
