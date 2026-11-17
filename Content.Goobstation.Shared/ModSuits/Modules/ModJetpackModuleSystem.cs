using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModJetpackModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedJetpackSystem _jetpack = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ModJetpackModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out var uid, out _, out var module))
        {
            if (!module.Active || HasComp<ActiveJetpackComponent>(uid))
                continue;

            if (_modSuit.TryGetModuleSuit(uid, out var suit))
                _modSuit.SetModuleActive(suit, (uid, module), false);
        }
    }

    [SubscribeLocalEvent]
    private void OnActivateAttempt(Entity<ModJetpackModuleComponent> ent, ref ModModuleActivateAttemptEvent args)
    {
        if (args.User is not { } user)
            return;

        if (Transform(user).GridUid is { } grid && HasComp<GravityComponent>(grid))
        {
            args.Cancelled = true;
            args.Reason = "jetpack-no-station";
        }
    }

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModJetpackModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (args.Wearer is { } wearer && TryComp<JetpackComponent>(ent, out var jetpack))
            _jetpack.SetEnabled(ent, jetpack, true, wearer);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModJetpackModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (TryComp<JetpackComponent>(ent, out var jetpack) && jetpack.JetpackUser is { } user)
            _jetpack.SetEnabled(ent, jetpack, false, user);
    }
}
