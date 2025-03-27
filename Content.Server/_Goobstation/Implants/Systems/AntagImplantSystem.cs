using Content.Shared.Implants;
using Content.Server._Goobstation.Implants.Components;

namespace Content.Server._Goobstation.Implants.Systems;

public sealed class AntagImplantBaseSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();
        IoCManager.InjectDependencies(this);
    }

    public void OnImplanted(EntityUid uid, AntagImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted.HasValue && !string.IsNullOrEmpty(comp.SelectedAntagPrototype))
        {
            EnsureComp(uid, comp.SelectedAntagPrototype);
        }
    }

    private void EnsureComp(EntityUid uid, string componentName)
    {
        var componentType = _componentFactory.GetRegistration(componentName)?.Type;
        if (componentType != null)
        {
            if (!EntityManager.HasComponent(uid, componentType))
            {
                EntityManager.AddComponent(uid, _componentFactory.GetComponent(componentType));
            }
        }
        else
        {
            Logger.Error($"Could not find component type {componentName}");
        }
    }
}
