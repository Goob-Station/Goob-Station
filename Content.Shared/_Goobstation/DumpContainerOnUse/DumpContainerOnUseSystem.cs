using System.Linq;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.DumpContainerOnUse;

/// <summary>
/// This handles...
/// </summary>
public sealed class DumpContainerOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DumpContainerOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<DumpContainerOnUseComponent> ent, ref UseInHandEvent args)
    {
        if(!_net.IsServer)
            return;

        if (args.Handled)
            return;

        args.Handled = true;

        _audioSystem.PlayPvs(ent.Comp.Sound, ent);

        var container = _containerSystem.GetContainer(ent, ent.Comp.ContainerId);

        if(container.ContainedEntities.Count == 0)
            return;

        // Make a copy of the entities before iterating
        var entitiesToRemove = container.ContainedEntities.ToList();

        foreach (var entity in entitiesToRemove)
        {
            _containerSystem.Remove(entity, container, force: true);
        }

        if(ent.Comp.DeleteAfterUse)
            QueueDel(ent);
    }
}
