using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Actions;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;


namespace Content.Goobstation.Server.SlaughterDemon;


/// <summary>
/// AHHHH BLOOD BLOOD ME LOVE BLOOD. LOVELY RED YES
/// </summary>
public sealed class BloodCrawlSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SlaughterDemonSystem _slaughter = default!;

    private EntityQuery<ActionsComponent> _actionQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _actionQuery = GetEntityQuery<ActionsComponent>();

        SubscribeLocalEvent<BloodCrawlComponent, BloodCrawlEvent>(OnBloodCrawl);

        SubscribeLocalEvent<BloodCrawlComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BloodCrawlComponent component, ComponentStartup args)
    {
        if (!_actionQuery.TryGetComponent(uid, out var actions))
            return;

        _actions.AddAction(uid, ref component.ActionEntity, component.ActionId, component: actions);
        component.OriginalEntity = uid;
    }

    private void OnBloodCrawl(EntityUid uid, BloodCrawlComponent component, BloodCrawlEvent args)
    {
        if (!_slaughter.IsStandingOnBlood(uid))
        {
            _actions.SetCooldown(component.ActionEntity, component.ActionCooldown);
            return;
        }

        component.IsCrawling = !component.IsCrawling;

        if (!component.IsCrawling && HasComp<PolymorphedEntityComponent>(uid))
        {
            _polymorph.Revert(uid);
            return;
        }

        var ev = new BloodCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        _polymorph.PolymorphEntity(uid, component.Jaunt);
        _actions.StartUseDelay(component.ActionEntity);
    }
}


