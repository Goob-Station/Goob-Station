using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Examine;
using static Content.Shared.Examine.ExamineSystemShared;
using Robust.Shared.Timing;
using Content.Shared.Actions.Events;
using Robust.Server.Audio;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicRegenerationPowerSystem : EntitySystem
    {
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ExamineSystemShared _examine = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, PsionicRegenerationPowerActionEvent>(OnPowerUsed);

            SubscribeLocalEvent<PsionicRegenerationPowerComponent, DispelledEvent>(OnDispelled);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, PsionicRegenerationDoAfterEvent>(OnDoAfter);
        }


        private void OnPowerUsed(EntityUid uid, PsionicRegenerationPowerComponent component, PsionicRegenerationPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, "psionic regeneration", true))
                return;

            var ev = new PsionicRegenerationDoAfterEvent(_gameTiming.CurTime);
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid);

            _doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId);

            component.DoAfter = doAfterId;

            _popupSystem.PopupEntity(Loc.GetString("psionic-regeneration-begin", ("entity", uid)),
                uid,
                // TODO: Use LoS-based Filter when one is available.
                Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !_examine.InRangeUnOccluded(uid, entity, ExamineRange, null)),
                true,
                PopupType.Medium);

            _audioSystem.PlayPvs(component.SoundUse, uid, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
            _psionics.LogPowerUsed(uid, "psionic regeneration");
            args.Handled = true;
        }

        private void OnDispelled(EntityUid uid, PsionicRegenerationPowerComponent component, DispelledEvent args)
        {
            if (component.DoAfter == null)
                return;

            _doAfterSystem.Cancel(component.DoAfter);
            component.DoAfter = null;

            args.Handled = true;
        }

        private void OnDoAfter(EntityUid uid, PsionicRegenerationPowerComponent component, PsionicRegenerationDoAfterEvent args)
        {
            component.DoAfter = null;

            if (!TryComp<BloodstreamComponent>(uid, out var stream))
                return;

            // DoAfter has no way to run a callback during the process to give
            // small doses of the reagent, so we wait until either the action
            // is cancelled (by being dispelled) or complete to give the
            // appropriate dose. A timestamp delta is used to accomplish this.
            var percentageComplete = Math.Min(1f, (_gameTiming.CurTime - args.StartedAt).TotalSeconds / component.UseDelay);

            var solution = new Solution();
            solution.AddReagent("PsionicRegenerationEssence", FixedPoint2.New(component.EssenceAmount * percentageComplete));
            _bloodstreamSystem.TryAddToChemicals(uid, solution, stream);
        }
    }
}

