using Content.Shared._Harmony.Conspirators.Components;
using Content.Shared._Harmony.Conspirators.EntitySystems;
using Content.Shared.Antag;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Harmony.Conspirators.EntitySystems;

public sealed class ConspiratorSystem : SharedConspiratorSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConspiratorComponent, GetStatusIconsEvent>(OnConspiratorGetIcons);
    }

    private void OnConspiratorGetIcons(Entity<ConspiratorComponent> entity, ref GetStatusIconsEvent args)
    {
        if (_playerManager.LocalSession?.AttachedEntity is { } playerEntity)
        {
            if (!HasComp<ShowAntagIconsComponent>(playerEntity) &&
                !HasComp<ConspiratorComponent>(playerEntity))
                return;
        }

        if (_prototypeManager.TryIndex(entity.Comp.ConspiratorIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
