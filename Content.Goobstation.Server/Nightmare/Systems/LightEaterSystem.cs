using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Light.Components;
using Content.Server.PowerCell;
using Content.Shared._EE.Nightmare;
using Content.Shared._EE.Nightmare.Components;
using Content.Shared.Inventory;
using Content.Shared.Light.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Weapons.Melee.Events;


namespace Content.Server._EE.Nightmare.Systems;


/// <summary>
/// This handles the Light Eater system.
/// Light Eater is an armblade that ashes any light that it attacks.
/// </summary>
public sealed class LightEaterSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightEaterUserComponent, ToggleLightEaterEvent>(OnToggleLightEater);

        SubscribeLocalEvent<LightEaterComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnToggleLightEater(EntityUid uid, LightEaterUserComponent component, ToggleLightEaterEvent args)
    {
        component.Activated = !component.Activated;
        if (!component.Activated)
        {
            var lightEater = Spawn(component.LightEaterProto, Transform(uid).Coordinates);
            component.LightEaterEntity = lightEater;
            if (!_handsSystem.TryPickupAnyHand(uid, lightEater, true))
            {
                QueueDel(component.LightEaterEntity);
            }
        }
        else
        {
            if (component.LightEaterEntity != null)
                QueueDel(component.LightEaterEntity);
        }
    }

    private void OnMeleeHit(EntityUid uid, LightEaterComponent component, MeleeHitEvent args)
    {
        if (!args.IsHit
            || !args.HitEntities.Any())
            return;

        foreach (var target in args.HitEntities)
        {
            if (HasComp<PoweredLightComponent>(target))
            {
                Spawn("Ash", Transform(target).Coordinates);
                QueueDel(target);
                continue;
            }

            if (TryComp<InventoryComponent>(target, out var inv))
            {
                foreach (var container in inv.Containers)
                {
                    foreach (var containerItem in container.ContainedEntities)
                    {
                        if (HasComp<HandheldLightComponent>(containerItem))
                        {
                            // not checking for point lights cuz of pda lights
                            Spawn("Ash", Transform(target).Coordinates);
                            QueueDel(containerItem);
                        }
                    }
                }
            }

            if (HasComp<BorgChassisComponent>(target))
            {
                if (!_powerCellSystem.TryGetBatteryFromSlot(target, out var battery))
                    continue;

                _powerCellSystem.SetDrawEnabled(target, false);
            }

            // could add more interactions in the future here
        }
    }
}
