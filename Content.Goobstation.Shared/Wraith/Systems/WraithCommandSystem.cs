using System.Linq;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// This handles the command ability of Wraith.
/// Hurls a few nearby loose objects at the chosen target.
/// </summary>
public sealed class WraithCommandSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithCommandComponent, WraithCommandEvent>(OnCommand);
    }

    private void OnCommand(Entity<WraithCommandComponent> ent, ref WraithCommandEvent args)
    {
        var entities = _lookupSystem.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);

        var objectsSelected = 0;
        foreach (var entity in entities)
        {
            if (_whitelist.IsBlacklistPass(ent.Comp.Blacklist, entity))
                continue;

            if (objectsSelected > ent.Comp.MaxObjects)
                break;

            _throwingSystem.TryThrow(entity, Transform(args.Target).Coordinates, ent.Comp.ThrowSpeed, ent.Owner);
            objectsSelected++;
        }

        // args.Handled = true;
    }
}
