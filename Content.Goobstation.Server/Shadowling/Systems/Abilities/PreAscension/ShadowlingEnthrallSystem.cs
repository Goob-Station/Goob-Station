using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Enthrall Abilities
/// </summary>
public sealed class ShadowlingEnthrallSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallEvent>(OnEnthrall);
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfter);
    }

    private void OnEnthrall(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallEvent args)
    {
        var target = args.Target;
        var time = comp.EnthrallTime;

        if (TryComp<EnthrallResistanceComponent>(target, out var enthrallRes))
            time += enthrallRes.ExtraTime;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            time,
            new EnthrallDoAfterEvent(),
            uid,
            target)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        if (!_shadowling.CanEnthrall(uid, target))
            return;
        // Basic Enthrall -> Can't melt Mindshields
        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
        }

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), uid, target, PopupType.SmallCaution);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEnthrallDoAfter(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallDoAfterEvent args)
    {
        _shadowling.DoEnthrall(uid, args);
    }
}
