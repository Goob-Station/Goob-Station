using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

[RegisterComponent, NetworkedComponent]
public sealed partial class DecaySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private static readonly ProtoId<StatusEffectPrototype> WraithDecay = "WraithDecay";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DecayComponent, DecayEvent>(OnDecay);
    }

    public void OnDecay(Entity<DecayComponent> ent, ref DecayEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        if (HasComp<HumanoidAppearanceComponent>(target))
        {
            _statusEffects.TryAddStatusEffect<DecayComponent>(target, WraithDecay, TimeSpan.FromSeconds(10), true);
            _popup.PopupEntity(Loc.GetString("wraith-decay"), target);
            return;
        }

        args.Handled = true;
    }
}
