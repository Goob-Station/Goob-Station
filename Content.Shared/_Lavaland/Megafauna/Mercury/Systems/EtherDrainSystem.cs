using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Coordinates;
using Content.Shared.Damage.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public sealed partial class EtherDrainSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EtherDrainComponent, EtherDrainEvent>(OnDrain);
    }

    private void OnDrain(Entity<EtherDrainComponent> ent, ref EtherDrainEvent args)
    {
        var target = args.Target;
        var comp = ent.Comp;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        _stamina.TakeOvertimeStaminaDamage(target, comp.StaminaDrain);
        _popup.PopupClient(Loc.GetString("ort-ether-drain"), target, target, PopupType.MediumCaution);

        PredictedSpawnAtPosition(comp.Prototype, Transform(target).Coordinates);

        args.Handled = true;
    }
}
