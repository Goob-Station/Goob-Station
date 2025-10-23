using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;


namespace Content.Goobstation.Shared.GunSpinning
{

    public sealed class GunSpinningSystem : EntitySystem
    {
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedIdentitySystem _identity = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpinableGunComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
            SubscribeLocalEvent<SpinableGunComponent, GunSpinDoAfterEvent>(OnDoafterSpin);
        }
        private void OnGetVerbs(EntityUid uid, SpinableGunComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
                return;

            var weapon = Identity.Name(args.Using.Value, EntityManager);

            var doAfter =
             new DoAfterArgs(EntityManager, args.User, comp.SpinTime, new GunSpinDoAfterEvent(), uid)
             {
                 BreakOnMove = false,
                 BreakOnDamage = true,
                 NeedHand = true,
                 MultiplyDelay = false,
             };

            AlternativeVerb verb = new()
            {
                Text = Loc.GetString("gun-spin"),
                Message = Loc.GetString("gun-spin-message"),
                Act = () =>
                {

                    var user = args.User;

                    var selfMsgStart = Loc.GetString("gun-spin-start-self", ("weapon", Identity.Name(uid, EntityManager)));
                    var othersMsgStart = Loc.GetString("gun-spin-start-others", ("user", user), ("weapon", Identity.Name(uid, EntityManager)));

                    _popupSystem.PopupPredicted(selfMsgStart, othersMsgStart, user, user, PopupType.Small);
                    var audio = _audio.PlayPredicted(comp.SoundSpin, uid, args.User);
                    if (audio != null)
                        comp.SoundEntity = audio.Value.Entity;

                    _doAfter.TryStartDoAfter(doAfter);
                }
            };
            args.Verbs.Add(verb);
        }
        private void OnDoafterSpin(EntityUid uid, SpinableGunComponent comp, GunSpinDoAfterEvent args)
        {

            var user = args.User;

            if (args.Cancelled)
            {
                var selfMsgCancel = Loc.GetString("gun-spin-cancel-self", ("weapon", Identity.Name(uid, EntityManager)));
                var othersMsgCancel = Loc.GetString("gun-spin-cancel-other", ("user", user), ("weapon", Identity.Name(uid, EntityManager)));
                _audio.Stop(comp.SoundEntity);
                _popupSystem.PopupPredicted(selfMsgCancel, othersMsgCancel, user, user, PopupType.Small);
                return;
            }
            if (args.Handled || args.Cancelled)
                return;

            args.Handled = true;

            if (_random.Prob(comp.FailChance))
            {
                // TODO replace this with a chance to shoot yourself in the head when gun executions gets merged

                var selfMsgFail = Loc.GetString("gun-spin-fail-self", ("weapon", Identity.Name(uid, EntityManager)));
                var othersMsgFail = Loc.GetString("gun-spin-fail-others", ("user", user), ("weapon", Identity.Name(uid, EntityManager)));
                _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, user, user, PopupType.Small);
                _audio.PlayPredicted(comp.SoundFail, uid, args.User);
                return;
            }

            var selfMsgFinish = Loc.GetString("gun-spin-complete-self", ("weapon", Identity.Name(uid, EntityManager)));
            var othersMsgFinish = Loc.GetString("gun-spin-complete-others", ("user", user), ("weapon", Identity.Name(uid, EntityManager)));
            _popupSystem.PopupPredicted(selfMsgFinish, othersMsgFinish, user, user, PopupType.Small);

        }
    }
}