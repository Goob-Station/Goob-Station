using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Systems;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Magic.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class MakeRevenentSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MakeRevenantComponent, MakeRevenantEvent>(OnMakeRevenant);
    }

    //TO DO: Add action for wraith to leave body.
    //TO DO: Add way for wraith to return to wraith body if killed while inside body.
    public void OnMakeRevenant(Entity<MakeRevenantComponent> ent, ref MakeRevenantEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-not-humanoid"), uid, uid);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-alive"), uid, uid);
            return;
        }

        if (_rotting.IsRotten(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-rotting"), uid, uid);
            return;
        }

        //TO DO: Heal the body. This shit don't fucking work.
        _mobState.ChangeMobState(target, MobState.Alive);

        // Before moving the wraith’s mind in, clear any existing mind.
        if (_mind.TryGetMind(target, out var targetMindId, out var targetMind))
        {
            _mind.UnVisit(targetMindId); //Detach mind from body
        }

        if (_mind.TryGetMind(uid, out var perMindId, out var perMind))
        {
            // Transfer into corpse
            _mind.TransferTo(perMindId, target);
            _audio.PlayPredicted(comp.PossessSound, target, target);

            Timer.Spawn(TimeSpan.FromSeconds(15), () =>
            {
                if (Deleted(uid) || Deleted(target))
                    return;

                // Transfer back into wraith body
                _mind.TransferTo(perMindId, uid);
                _audio.PlayPredicted(comp.PossessEndSound, uid, uid);

                //TO DO: Return the corpse to being dead instead of gibbing.
                _bodySystem.GibBody(target);
            });
        }


        args.Handled = true;
    }
}
