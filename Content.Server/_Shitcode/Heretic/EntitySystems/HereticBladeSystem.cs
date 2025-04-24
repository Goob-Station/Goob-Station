using System.Linq;
using System.Text;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Rotting;
using Content.Server.Body.Systems;
using Content.Server.Heretic.Components;
using Content.Server.Teleportation;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Teleportation;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticBladeSystem : SharedHereticBladeSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;

    protected override void ApplyAshBladeEffect(EntityUid target)
    {
        base.ApplyAshBladeEffect(target);

        _flammable.AdjustFireStacks(target, 2.5f, ignite: true);
    }

    protected override void ApplyFleshBladeEffect(EntityUid target)
    {
        base.ApplyFleshBladeEffect(target);

        _blood.TryModifyBleedAmount(target, 1.5f);
    }

    protected override void ApplyVoidBladeEffect(EntityUid target)
    {
        base.ApplyVoidBladeEffect(target);

        if (TryComp<TemperatureComponent>(target, out var temp))
            _temp.ForceChangeTemperature(target, temp.CurrentTemperature - 5f, temp);
    }

    protected override void RandomTeleport(EntityUid user, EntityUid blade, RandomTeleportComponent comp)
    {
        base.RandomTeleport(user, blade, comp);


        _teleport.RandomTeleport(user, comp);
        QueueDel(blade);
    }
}
