using Content.Server.Administration.Logs;
using Content.Server.Cargo.Components;
using Content.Server.Stack;
using Content.Shared._EinsteinEngines.Silicon.BlindHealing;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stacks;

namespace Content.Server._EinsteinEngines.Silicon.BlindHealing;

public sealed class BlindHealingSystem : SharedBlindHealingSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly BlindableSystem _blindableSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlindHealingComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<BlindHealingComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<BlindHealingComponent, HealingDoAfterEvent>(OnHealingFinished);
    }

     private void OnHealingFinished(EntityUid uid, BlindHealingComponent component, HealingDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null
            || !TryComp<BlindableComponent>(args.Target, out var blindComp)
            || blindComp is { EyeDamage: 0 })
            return;

        if (TryComp<StackComponent>(uid, out var stackComponent)
            && TryComp<StackPriceComponent>(uid, out var stackPrice))
            _stackSystem.SetCount(uid, (int) (_stackSystem.GetCount(uid, stackComponent) - stackPrice.Price), stackComponent);

        _blindableSystem.AdjustEyeDamage((args.Target.Value, blindComp), -blindComp.EyeDamage);

        _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target}'s vision");

        var str = Loc.GetString("comp-repairable-repair",
            ("target", uid),
            ("tool", args.Used!));
        _popup.PopupEntity(str, uid, args.User);

    }
    
    private bool TryHealBlindness(EntityUid uid, EntityUid user, EntityUid target, float delay)
    {
        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new HealingDoAfterEvent(), uid, target: target, used: uid)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    private void OnInteract(EntityUid uid, BlindHealingComponent component, ref AfterInteractEvent args)
    {

        if (args.Handled
            || !TryComp<DamageableComponent>(args.User, out var damageable)
            || damageable.DamageContainerID != null && !component.DamageContainers.Contains(damageable.DamageContainerID)
            || !TryComp<BlindableComponent>(args.User, out var blindcomp)
            || blindcomp.EyeDamage == 0
            || args.User == args.Target && !component.AllowSelfHeal)
            return;

        TryHealBlindness(uid, args.User, args.User,
            args.User == args.Target
                ? component.DoAfterDelay * component.SelfHealPenalty
                : component.DoAfterDelay);
    }

    private void OnUse(EntityUid uid, BlindHealingComponent component, ref UseInHandEvent args)
    {
        if (args.Handled
            || !TryComp<DamageableComponent>(args.User, out var damageable)
            || damageable.DamageContainerID != null && !component.DamageContainers.Contains(damageable.DamageContainerID)
            || !TryComp<BlindableComponent>(args.User, out var blindcomp)
            || blindcomp.EyeDamage == 0
            || !component.AllowSelfHeal)
            return;

        TryHealBlindness(uid, args.User, args.User,
            component.DoAfterDelay * component.SelfHealPenalty);
    }
}