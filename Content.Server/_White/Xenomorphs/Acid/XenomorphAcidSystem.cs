using Content.Shared._White.Xenomorphs.Acid;
using Content.Shared._White.Xenomorphs.Acid.Components;
using Content.Server.Actions;
using Content.Shared.Damage;

namespace Content.Server._White.Xenomorphs.Acid;

public sealed class XenomorphAcidSystem : SharedXenomorphAcidSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphAcidComponent, MapInitEvent>(OnXenomorphAcidMapInit);
        SubscribeLocalEvent<XenomorphAcidComponent, ComponentShutdown>(OnXenomorphAcidShutdown);
    }

    private void OnXenomorphAcidMapInit(EntityUid uid, XenomorphAcidComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.AcidAction, component.AcidActionId);

    private void OnXenomorphAcidShutdown(EntityUid uid, XenomorphAcidComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.AcidAction);

    public override void Update(float frameTime)
    {
        var time = Timing.CurTime;

        var acidCorrodingQuery = EntityQueryEnumerator<AcidCorrodingComponent>();
        while (acidCorrodingQuery.MoveNext(out var uid, out var acidCorrodingComponent))
        {
            if (time > acidCorrodingComponent.NextDamageAt)
            {
                _damageable.TryChangeDamage(uid, acidCorrodingComponent.DamagePerSecond);
                acidCorrodingComponent.NextDamageAt = time + TimeSpan.FromSeconds(1);
            }

            if (time <= acidCorrodingComponent.AcidExpiresAt)
                continue;

            QueueDel(acidCorrodingComponent.Acid);
            RemCompDeferred<AcidCorrodingComponent>(uid);
        }
    }
}
