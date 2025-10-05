using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Goobstation.Shared.Morph;
using Content.Shared.Alert;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Devour;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Polymorph.Components;
using Content.Shared.Polymorph.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Server.Morph;

public sealed partial class MorphSystem : SharedMorphSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedChameleonProjectorSystem _chamleon = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobstate= default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MorphComponent, DevourDoAfterEvent>(OnMorphDevour);
        SubscribeLocalEvent<MorphComponent, MorphReplicateEvent>(OnMorphReplicate);
        SubscribeLocalEvent<MorphComponent, ReplicateDoAfterEvent>(OnMorphReplicateDoAfter);
        SubscribeLocalEvent<MorphComponent, MapInitEvent>(OnMapInit);

        //SubscribeLocalEvent<ChameleonProjectorComponent, MorphEvent>(TryMorph);
        SubscribeLocalEvent<ChameleonDisguisedComponent, UnMorphEvent>(TryUnMorph);

        SubscribeLocalEvent<MorphComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<MorphDisguiseComponent, ExaminedEvent>(AddMorphExamine);
       // SubscribeLocalEvent<MorphComponent, AttemptMeleeEvent>(OnAtack);
        SubscribeLocalEvent<MorphComponent, DamageChangedEvent>(OnTakeDamage);
        SubscribeLocalEvent<MorphComponent, MobStateChangedEvent>(OnDeath);
    }

    private void OnMapInit(EntityUid uid, MorphComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, component.MorphReplicate);
        _action.AddAction(uid, component.Morph);
        _action.AddAction(uid, component.UnMorph);

        _alerts.ShowAlert(uid, component.BiomassAlert);

    }

    # region Actions

    private void ChangeBiomassAmount(FixedPoint2 amount, EntityUid uid, MorphComponent? component = null)
    {
        if (component != null)
        {
            component.Biomass = FixedPoint2.Min(component.Biomass+amount, component.MaxBiomass);
            Dirty(uid, component);
            _alerts.ShowAlert(uid, component.BiomassAlert);
        }
    }

    private void OnMorphDevour(EntityUid uid , MorphComponent component, DevourDoAfterEvent arg)
    {
        if (arg.Handled || arg.Cancelled)
            return;

        if (!TryComp<PhysicsComponent>(arg.Target, out var physics))
            _popupSystem.PopupEntity(Loc.GetString("morph-no-biomass-target"), uid, arg.User, PopupType.Medium);

        else if (_whitelist.IsWhitelistPass(component.BiomassWhitelist, arg.Target.Value)
                 && _whitelist.IsBlacklistFail(component.BiomassBlacklist, arg.Target.Value))
            ChangeBiomassAmount(physics.Mass , uid, component);

        else
            _popupSystem.PopupEntity(Loc.GetString("morph-no-biomass-target"), uid, arg.User, PopupType.Medium);

        if (TryComp<MobStateComponent>(arg.Target, out var mob) && mob.CurrentState == MobState.Critical)
           _mobstate.ChangeMobState(arg.Target.Value, MobState.Dead); // kill the food upon devour if not dead

    }

    private void OnMorphReplicate(EntityUid uid , MorphComponent component, MorphReplicateEvent arg)
    {
        if (component.Biomass <= component.ReplicateCost)
        {
            _popupSystem.PopupEntity(Loc.GetString("morph-no-biomass"), uid, arg.Performer, PopupType.Medium);

            return;
        }
        var doafterArgs = new DoAfterArgs(EntityManager, arg.Performer, component.ReplicationDelay, new ReplicateDoAfterEvent(), uid, used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.5f,
        };


        _doAfterSystem.TryStartDoAfter(doafterArgs);

        _popupSystem.PopupEntity(Loc.GetString("morph-reproduce-start"), uid, arg.Performer, PopupType.Medium);

        arg.Handled = true;
    }

    private void OnMorphReplicateDoAfter(EntityUid uid , MorphComponent component, ReplicateDoAfterEvent arg)
    {
        if (arg.Handled || arg.Cancelled)
            return;

        var userCoords = Transform(arg.User);
        var morphSpawnCoords = userCoords.Coordinates;


        arg.Handled = true;

        ChangeBiomassAmount(-(component.ReplicateCost), uid, component);

        Spawn(component.MorphPrototype, morphSpawnCoords);
        _audio.PlayPvs(component.ReplicateSound, uid, null);
        MorphComponent.Children += 1;
    }
    # endregion

    # region Morph Disguise

    private void TryMorph(Entity<ChameleonProjectorComponent> ent, ref MorphEvent arg)
    {
        _chamleon.TryDisguise(ent, arg.Performer, arg.Target);
    }

    private void TryUnMorph(Entity<ChameleonDisguisedComponent> ent, ref UnMorphEvent arg)
    {
        _chamleon.TryReveal(ent!);
    }

    private void AddMorphExamine(EntityUid uid, MorphDisguiseComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
            args.PushMarkup(Loc.GetString(component.ExamineMessage), component.Priority);
    }

    private void OnTakeDamage(EntityUid uid, MorphComponent morph,  DamageChangedEvent args)
    {
        if (!HasComp<ChameleonDisguisedComponent>(uid))
            return;// you are not Disguised

        if (args.DamageDelta is null)
            return; // damage is moody
        if (!args.DamageIncreased)
            return; // you are being healed

        if (args.DamageDelta.GetTotal() < morph.DamageThreshold)
            return; // if damage is over threshold, unmorph

        if (TryComp<ChameleonDisguisedComponent>(uid, out var comp))
            _chamleon.TryReveal((uid,comp));

    }

    /*private void OnAtack(EntityUid uid, MorphComponent component, ref AttemptMeleeEvent args)
    {
        //abort attack if morphed
        if (HasComp<ChameleonDisguisedComponent>(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("morph-attack-failure"),uid,uid);
            args.Cancelled = true;
        }
    }*/

    private void OnDeath(Entity<MorphComponent> ent,ref  MobStateChangedEvent args)
    {
        //remove disguise in case morph dies while in disguise
        if (args.NewMobState is MobState.Dead && TryComp<ChameleonDisguisedComponent>(ent.Owner, out var comp))
            _chamleon.TryReveal((ent.Owner,comp));

    }

    private void OnTransformSpeakerName(Entity<MorphComponent> ent, ref TransformSpeakerNameEvent arg)
    {
        if (!TryComp<ChameleonDisguisedComponent>(ent.Owner, out var comp))
            return; //not disguised

        arg.VoiceName = MetaData(comp.Disguise).EntityName;
        arg.Sender = comp.Disguise;

    }

    # endregion

}



