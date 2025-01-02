using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Body;

public sealed class CursedHeartSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    //[Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedHeartComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CursedHeartComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CursedHeartComponent, PumpHeartActionEvent>(OnPump);

        SubscribeLocalEvent<CursedHeartGrantComponent, UseInHandEvent>(OnUseInHand);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CursedHeartComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobState.IsDead(uid))
                continue;
            if (_timing.CurTime >= comp.LastPump + TimeSpan.FromSeconds(comp.MaxDelay))
            {
                Damage(uid);
                comp.LastPump = _timing.CurTime;
            }
        }
    }

    [ValidatePrototypeId<DamageGroupPrototype>]
    private const string BloodDamage = "Bloodloss";

    private void Damage(EntityUid uid)
    {
        //_bloodstream.TryModifyBloodLevel(uid, -50, spill: false);
        _damage.TryChangeDamage(uid, new DamageSpecifier(_proto.Index<DamageGroupPrototype>(BloodDamage), 30), true, false);
        _popup.PopupEntity(Loc.GetString("popup-cursed-heart-damage"), uid, uid, PopupType.MediumCaution);
    }

    private void OnMapInit(EntityUid uid, CursedHeartComponent comp, MapInitEvent args)
    {
        _actions.AddAction(uid, ref comp.PumpActionEntity, "ActionPumpCursedHeart");
    }

    private void OnShutdown(EntityUid uid, CursedHeartComponent comp, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, comp.PumpActionEntity);
    }

    private void OnPump(EntityUid uid, CursedHeartComponent comp, PumpHeartActionEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg"), uid);
        _damage.TryChangeDamage(uid, new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), -8), true, false);
        //_bloodstream.TryModifyBloodLevel(uid, 17);
        comp.LastPump = _timing.CurTime;
    }

    private void OnUseInHand(EntityUid uid, CursedHeartGrantComponent comp, UseInHandEvent args)
    {
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg"), args.User);
        var heart = EnsureComp<CursedHeartComponent>(args.User);
        heart.LastPump = _timing.CurTime;
        QueueDel(uid);
    }
}
