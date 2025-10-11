using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Mind;
using Content.Shared.Rejuvenate;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed class MakeRevenentSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly WraithPossessedSystem _wraithPossessed = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MakeRevenantComponent, MakeRevenantEvent>(OnMakeRevenant);
    }

    //TO DO: Add action for wraith to leave body.
    //TO DO: Add way for wraith to return to wraith body if killed while inside body.
    private void OnMakeRevenant(Entity<MakeRevenantComponent> ent, ref MakeRevenantEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        if (!HasComp<WraithAbsorbableComponent>(args.Target)
            || !TryComp<PerishableComponent>(args.Target, out var perishComp)
            || perishComp.Stage != 1) // should have been an enum... anyways: 1 means its a fresh corpse
            return;

        if (_netManager.IsClient)
            return;

        var rej = new RejuvenateEvent();
        RaiseLocalEvent(args.Target, rej);

        var possessed = EnsureComp<WraithPossessedComponent>(args.Target);
        possessed.RevenantDamageOvertime = ent.Comp.PassiveRevenantDamage;
        Dirty(args.Target, possessed);

        _audio.PlayPredicted(ent.Comp.PossessSound, args.Target, args.Target);
        _wraithPossessed.StartPossession((args.Target, possessed), ent.Owner, mindId, true);

        // THE REV TODO LIST:
        // YAML:
        //  - WP regeneration
        //  - stun immunity
        //  - ignore stamina penalties
        // Mechanics:
        //  - lose their normal set of powers (honestly block doing this on magic antags)
        //  - constantly lose health (done)
        //  - cannot be healed (done)
        //  - death = black smoke (done)
        //  - you can identify one easily
        // Actions:
        //  - Push (done)
        //  - Mass Command (done)
        //  - Shockwave (done)
        //  - Touch of Evil (done)
        //  - Crush (done)
        // Polish:
        // - add popups
        // - add sounds
        // - actually playtest all abilities lmao (i havent even launched the game at all)

        args.Handled = true;
    }
}
