using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared.DoAfter;
using Content.Shared.Popups;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Empowered Enthrall.
/// Empowered enthrall is like the basic enthrall, however it can bypass Mindshields, has slightly longer range.
/// It also takes less time to enthrall someone.
/// </summary>
public sealed class ShadowlingEmpoweredEnthrallSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, EmpoweredEnthrallEvent>(OnEmpEnthrall);
        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, EmpoweredEnthrallDoAfterEvent>(OnEmpEnthrallDoAfter);

        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ShadowlingEmpoweredEnthrallComponent component, ComponentStartup args)
    {
        // We no longer need the previous Enthrall
        if (!TryComp<ShadowlingComponent>(uid, out var sling))
            return;
        _actions.RemoveAction(uid, sling.ActionEnthrallEntity);
        RemComp<ShadowlingEnthrallComponent>(uid);
    }

private void OnEmpEnthrall(EntityUid uid, ShadowlingEmpoweredEnthrallComponent component, EmpoweredEnthrallEvent args)
    {
        var target = args.Target;
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            component.EnthrallTime,
            new EmpoweredEnthrallDoAfterEvent(),
            uid,
            target)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        if (!_shadowling.CanEnthrall(uid, target))
            return;

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), uid, target, PopupType.SmallCaution);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEmpEnthrallDoAfter(EntityUid uid, ShadowlingEmpoweredEnthrallComponent component, EmpoweredEnthrallDoAfterEvent args)
    {
        _shadowling.DoEnthrall(uid, args);
    }
}
