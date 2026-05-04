using Content.Goobstation.Common.Religion;
using Content.Server._DV.CosmicCult.Components;
using Content.Server.EUI;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Jittering;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Content.Shared.Tools.Systems;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult;

public sealed class DeconversionSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedToolSystem _tools = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCenserComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CosmicCenserTargetComponent, CleanseOnDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<CleanseCultComponent, ComponentInit>(OnCompInit);
    }

    private void OnCompInit(Entity<CleanseCultComponent> uid, ref ComponentInit args)
    {
        _jittering.DoJitter(uid.Owner, uid.Comp.CleanseDuration, true, 5, 20);
        uid.Comp.CleanseTime = _timing.CurTime + uid.Comp.CleanseDuration;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var deconCultTimer = EntityQueryEnumerator<CleanseCultComponent>();
        while (deconCultTimer.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.CleanseTime && !HasComp<CosmicBlankComponent>(uid))
            {
                RemComp<CleanseCultComponent>(uid);
                DeconvertCultist(uid);
            }
        }
    }

    private void OnAfterInteract(Entity<CosmicCenserComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not {} target || !HasComp<CosmicCenserTargetComponent>(target) || _mobState.IsDead(target))
            return;

        args.Handled = _tools.UseTool(
            args.Used,
            args.User,
            args.Target,
            ent.Comp.DeconversionTime,
            [ent.Comp.ToolRequired],
            new CleanseOnDoAfterEvent(),
            out _);
    }

    private void OnDoAfter(Entity<CosmicCenserTargetComponent> uid, ref CleanseOnDoAfterEvent args)
    {
        var target = args.Args.Target;
        if (args.Cancelled || args.Handled || target == null || _mobState.IsDead(target.Value))
            return;

        if (args.Args.Used is not {} used || !TryComp<CosmicCenserComponent>(used, out var censer))
            return;

        var targetPosition = Transform(target.Value).Coordinates;
        var userPosition = Transform(args.User).Coordinates;
        //TODO: This could be made more agnostic, but there's only one cult for now, and frankly, i'm so tired. This is easy to read and easy to modify code. Expand it at thine leisure.
        if (TryComp<CosmicCultComponent>(args.Target, out var comp) && comp.CosmicEmpowered)
        {
            Spawn(censer.MalignVFX, targetPosition);
            Spawn(censer.MalignVFX, userPosition);
            EnsureComp<CleanseCultComponent>(target.Value, out var cleanse);
            cleanse.CleanseDuration = TimeSpan.FromSeconds(1);
            _audio.PlayPvs(censer.MalignSound, targetPosition, AudioParams.Default.WithVolume(2f));
            _popup.PopupEntity(Loc.GetString("cleanse-deconvert-attempt-success-empowered", ("target", Identity.Entity(target.Value, EntityManager))), args.User, args.User);
        }
        else if (TryComp<CosmicCultComponent>(target, out var cultComponent) && !cultComponent.CosmicEmpowered)
        {
            Spawn(censer.CleanseVFX, targetPosition);
            EnsureComp<CleanseCultComponent>(target.Value, out var cleanse);
            cleanse.CleanseDuration = TimeSpan.FromSeconds(1);
            _audio.PlayPvs(censer.CleanseSound, targetPosition, AudioParams.Default.WithVolume(4f));
            _popup.PopupEntity(Loc.GetString("cleanse-deconvert-attempt-success", ("target", Identity.Entity(target.Value, EntityManager))), args.User, args.User);
        }
        else
        {
            Spawn(censer.ReboundVFX, userPosition);
            Spawn(censer.ReboundVFX, targetPosition);
            _audio.PlayPvs(censer.SizzleSound, targetPosition);
            _popup.PopupEntity(Loc.GetString("cleanse-deconvert-attempt-notcorrupted", ("target", Identity.Entity(target.Value, EntityManager))), args.User, args.User);
            _popup.PopupCoordinates(Loc.GetString("cleanse-deconvert-attempt-rebound"), targetPosition, PopupType.MediumCaution);
            _damageable.TryChangeDamage(args.User, censer.FailedDeconversionDamage, true);
            _damageable.TryChangeDamage(args.Target, censer.FailedDeconversionDamage, true);
        }
        args.Handled = true;
    }

    private void DeconvertCultist(EntityUid uid)
    {
        RemComp<CosmicCultComponent>(uid);
        if (TryComp<PolymorphedEntityComponent>(uid, out var polyComp) && polyComp.Parent.HasValue) // If the cultist is polymorphed, we revert the polymorph and deconvert the original entity too.
        {
            _polymorph.Revert((uid, polyComp));
            RemCompDeferred<CosmicCultComponent>(polyComp.Parent.Value);
        }
    }
}
