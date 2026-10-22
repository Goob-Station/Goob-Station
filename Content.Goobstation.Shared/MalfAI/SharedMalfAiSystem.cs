using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;

namespace Content.Goobstation.Shared.MalfAI;

public abstract partial class SharedMalfAiSystem : EntitySystem
{
    [Dependency] private SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiComponent, BoundUserInterfaceMessageAttempt>(OnUiMessage);
        SubscribeLocalEvent<MalfAiComponent, AccessibleOverrideEvent>(OnAccessibleOverride);
    }

    private void OnUiMessage(Entity<MalfAiComponent> ent, ref BoundUserInterfaceMessageAttempt args)
    {
        if (args.UiKey.Equals(StoreUiKey.Key)
            && (!HasComp<StationAiHeldComponent>(args.Actor)
                || !_mind.TryGetMind(args.Actor, out var mind, out _)
                || mind != ent.Owner))
            args.Cancel();
    }

    private void OnAccessibleOverride(Entity<MalfAiComponent> ent, ref AccessibleOverrideEvent args)
    {
        if (args.Target != ent.Owner
            || !HasComp<StationAiHeldComponent>(args.User)
            || !_mind.TryGetMind(args.User, out var mind, out _)
            || mind != ent.Owner)
            return;

        args.Handled = true;
        args.Accessible = true;
    }
}
