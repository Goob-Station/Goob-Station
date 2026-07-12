using Content.Shared._Harmony.Conspirators.Components;
using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._Harmony.Conspirators.EntitySystems;

public abstract class SharedConspiratorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConspiratorComponent, ComponentGetStateAttemptEvent>(OnConspiratorAttemptGetState);
    }

    private void OnConspiratorAttemptGetState(
        Entity<ConspiratorComponent> entity,
        ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    private bool CanGetState(ICommonSession? player)
    {
        if (player?.AttachedEntity is not { } uid)
            return true;

        return HasComp<ConspiratorComponent>(uid) || HasComp<ShowAntagIconsComponent>(uid);
    }
}
