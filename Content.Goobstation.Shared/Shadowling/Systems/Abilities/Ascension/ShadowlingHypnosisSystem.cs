// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles Hypnosis.
/// Instant Thrall from afar!
/// </summary>
public sealed class ShadowlingHypnosisSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingHypnosisComponent, HypnosisEvent>(OnHypnosis);
        SubscribeLocalEvent<ShadowlingHypnosisComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingHypnosisComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingHypnosisComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingHypnosisComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnHypnosis(EntityUid uid, ShadowlingHypnosisComponent component, HypnosisEvent args)
    {
        var target = args.Target;
        if (args.Handled
            || !HasComp<HumanoidAppearanceComponent>(target)
            || HasComp<ThrallComponent>(target)
            || HasComp<ShadowlingComponent>(target))
            return;

        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
        }

        var comps = _proto.Index(component.HypnosisComponents);
        EntityManager.AddComponents(target, comps);
        args.Handled = true;
    }
}
