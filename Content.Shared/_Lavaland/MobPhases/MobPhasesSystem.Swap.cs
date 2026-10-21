using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared._Lavaland.MobPhases;

public abstract partial class MobPhasesSystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private ISerializationManager _serialization = default!;

    /// <summary>
    /// Undoes the old phases component changes and applies the next phases.
    /// </summary>
    protected void SwapComponents(
        EntityUid target,
        ComponentRegistry stash,
        List<ProtoId<MobPhaseComponentsPrototype>>? oldAdd,
        List<ProtoId<MobPhaseComponentsPrototype>>? oldRemove,
        List<ProtoId<MobPhaseComponentsPrototype>>? nextAdd,
        List<ProtoId<MobPhaseComponentsPrototype>>? nextRemove)
    {
        if (TerminatingOrDeleted(target))
            return;

        // Undo the old phase.
        foreach (var (name, _) in Components(oldAdd))
        {
            RemComp(target, _factory.GetRegistration(name).Type);
            Restore(name);
        }

        foreach (var (name, _) in Components(oldRemove))
            Restore(name);

        // Apply the next phase.
        foreach (var (name, _) in Components(nextRemove))
        {
            Stash(name);
            RemComp(target, _factory.GetRegistration(name).Type);
        }

        foreach (var (name, entry) in Components(nextAdd))
        {
            Stash(name);
            EntityManager.AddComponents(target, new ComponentRegistry { [name] = entry });
        }

        void Restore(string name)
        {
            if (stash.Remove(name, out var original))
                EntityManager.AddComponents(target, new ComponentRegistry { [name] = original });
        }

        void Stash(string name)
        {
            var registration = _factory.GetRegistration(name);
            if (stash.ContainsKey(name) || !TryComp(target, registration.Type, out var existing))
                return;

            var copy = (object?) _factory.GetComponent(registration);
            _serialization.CopyTo(existing, ref copy, notNullableOverride: true);
            stash[name] = new EntityPrototype.ComponentRegistryEntry((IComponent) copy!);
        }
    }

    private ComponentRegistry Components(List<ProtoId<MobPhaseComponentsPrototype>>? ids)
    {
        var components = new ComponentRegistry();
        if (ids == null)
            return components;

        foreach (var id in ids)
        {
            if (!_proto.TryIndex(id, out var proto))
                continue;

            foreach (var (name, entry) in proto.Components)
            {
                if (_net.IsClient && _factory.GetRegistration(name).NetID != null)
                    continue;

                components[name] = entry;
            }
        }

        return components;
    }
}
