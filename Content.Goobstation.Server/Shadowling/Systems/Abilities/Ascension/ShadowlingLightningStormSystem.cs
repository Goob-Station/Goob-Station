using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Lightning;
using Content.Shared._EE.Shadowling;
using Content.Shared.DoAfter;
using Content.Shared.Electrocution;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Lightning Storm ability.
/// Lightning Storm creates a lightning ball that electrocutes everyone near a specific radius
/// </summary>
public sealed class ShadowlingLightningStormSystem : EntitySystem
{
    [Dependency] private readonly LightningSystem _lightningSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingLightningStormComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEvent>(OnLightningStorm);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEventDoAfterEvent>(OnLightningStormDoAfter);
    }

    private void OnStartup(EntityUid uid, ShadowlingLightningStormComponent component, ComponentStartup args)
    {
        // So they don't get hit by their own lightning
        EnsureComp<InsulatedComponent>(uid);
    }

    private void OnLightningStorm(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEvent args)
    {
        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHH
        var doAfter = new DoAfterArgs(
            EntityManager,
            args.Performer,
            component.TimeBeforeActivation,
            new LightningStormEventDoAfterEvent(),
            uid)
        {
            BreakOnDamage = true,
            CancelDuplicate = true,
        };
        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private void OnLightningStormDoAfter(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEventDoAfterEvent args)
    {
        if (!TryComp<ShadowlingComponent>(uid, out var sling))
            return;

        if (args.Cancelled)
        {
            _actions.SetCooldown(sling.ActionLightningStormEntity, TimeSpan.FromSeconds(2));
            return;
        }

        _lightningSystem.ShootRandomLightnings(uid, component.Range, component.BoltCount, component.LightningProto);

        _actions.StartUseDelay(sling.ActionLightningStormEntity);
    }
}
