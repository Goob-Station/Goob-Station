using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.TeslaBlast;

public abstract class SharedTeslaBlastSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CastingTeslaBlastComponent, TeslaBlastDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<CastingTeslaBlastComponent> ent, ref TeslaBlastDoAfterEvent args)
    {
        RemCompDeferred<CastingTeslaBlastComponent>(ent);

        if (args.Handled)
            return;

        args.Handled = true;

        var doAfter = args.DoAfter;

        if (_net.IsServer)
        {
            var sound = ent.Comp.Sound;
            if (sound != null)
                _audio.Stop(sound.Value.Owner, sound.Value.Comp);
            QueueDel(ent.Comp.Effect);
        }

        // This will fire when cancelled
        if (doAfter.CancelledTime == null)
        {
            if (!_net.IsServer)
                return;

            _actions.StartUseDelay(GetEntity(args.Action));
            _popup.PopupEntity(Loc.GetString("spell-fail-tesla-blast"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        var time = doAfter.CancelledTime.Value - doAfter.StartTime;

        if (time < TimeSpan.Zero)
            return;

        var power = args.Delay <= TimeSpan.Zero ? 1f : Math.Clamp((float) (time / args.Delay), 0f, 1f);

        ShootRandomLightnings(args.User,
            power,
            args.Range,
            args.BoltCount,
            args.ArcDepth,
            args.LightningPrototype,
            args.MinMaxDamage,
            args.MinMaxStunTime);
    }

    public void StartCharging(TeslaBlastEvent ev)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            ev.Performer,
            ev.Delay,
            new TeslaBlastDoAfterEvent(ev.Delay,
                ev.Range,
                ev.BoltCount,
                ev.ArcDepth,
                ev.MinMaxDamage,
                ev.MinMaxStunTime,
                ev.LightningPrototype,
                GetNetEntity(ev.Action)),
            ev.Performer)
        {
            MultiplyDelay = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        var casting = EnsureComp<CastingTeslaBlastComponent>(ev.Performer);
        casting.DoAfterId = id.Value.Index;

        if (_net.IsClient)
            return;

        var xform = Transform(ev.Performer);

        var effect = SpawnAttachedTo(ev.EffectPrototype, xform.Coordinates);
        _transform.SetParent(effect, Transform(effect), ev.Performer, xform);
        casting.Effect = effect;

        casting.Sound = _audio.PlayEntity(ev.Sound, Filter.Pvs(ev.Performer), ev.Performer, true);
    }

    public void CancelDoAfter(EntityUid uid, CastingTeslaBlastComponent casting)
    {
        if (_net.IsServer)
            _doAfter.Cancel(uid, casting.DoAfterId);
    }

    public virtual void ShootRandomLightnings(EntityUid performer,
        float power,
        float range,
        int boltCount,
        int arcDepth,
        string lightningPrototype,
        Vector2 minMaxDamage,
        Vector2 minMaxStunTime)
    {
    }

    public virtual void ShootLightning(EntityUid performer,
        EntityUid target,
        string lightningPrototype,
        float damage)
    {
    }
}

[Serializable, NetSerializable]
public sealed partial class TeslaBlastDoAfterEvent(
    TimeSpan delay,
    float range,
    int boltCount,
    int arcDepth,
    Vector2 damage,
    Vector2 stunTime,
    string lightningPrototype,
    NetEntity action) : DoAfterEvent
{
    public TimeSpan Delay = delay;

    public float Range = range;

    public int BoltCount = boltCount;

    public int ArcDepth = arcDepth;

    public Vector2 MinMaxDamage = damage;

    public Vector2 MinMaxStunTime = stunTime;

    public string LightningPrototype = lightningPrototype;

    public NetEntity Action = action;

    public TeslaBlastDoAfterEvent() : this(TimeSpan.FromSeconds(10),
        7f,
        1,
        5,
        new(15f, 50f),
        new(1f, 8f),
        "SuperchargedLightning",
        NetEntity.Invalid)
    {
    }

    public override DoAfterEvent Clone() => this;
}
