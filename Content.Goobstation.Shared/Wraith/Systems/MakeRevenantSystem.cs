using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class MakeRevenentSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly WraithPossessedSystem _wraithPossessed = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MakeRevenantComponent, MakeRevenantEvent>(OnMakeRevenant);
    }

    //TO DO: Add action for wraith to leave body.
    //TO DO: Add way for wraith to return to wraith body if killed while inside body.
    public void OnMakeRevenant(Entity<MakeRevenantComponent> ent, ref MakeRevenantEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        //TO DO: Heal the body. This shit don't fucking work.
        var rej = new RejuvenateEvent();
        RaiseLocalEvent(args.Target, rej);

        var possessed = EnsureComp<WraithPossessedComponent>(args.Target);
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
