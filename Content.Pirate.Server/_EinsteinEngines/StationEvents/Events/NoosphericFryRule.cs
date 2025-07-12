using Content.Shared.GameTicking.Components;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Server.Inventory;
using Content.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Psionics.Glimmer;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Maps;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Robust.Server.Player;

namespace Content.Goobstation.Server.StationEvents.Events;

/// <summary>
/// Fries tinfoil hats and cages
/// </summary>
internal sealed class NoosphericFryRule : StationEventSystem<NoosphericFryRuleComponent>
{
    [Dependency] private readonly SharedMapSystem _sharedMapSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;

    protected override void Started(EntityUid uid, NoosphericFryRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<(EntityUid wearer, TinfoilHatComponent worn)> psionicList = new();

        var query = EntityManager.EntityQueryEnumerator<PsionicInsulationComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out _))
        {
            if (!_mobStateSystem.IsAlive(psion))
                continue;

            if (!_inventorySystem.TryGetSlotEntity(psion, "head", out var headItem))
                continue;

            if (!TryComp<TinfoilHatComponent>(headItem, out var tinfoil))
                continue;

            psionicList.Add((psion, tinfoil));
        }

        foreach (var pair in psionicList)
        {
            if (pair.worn.DestroyOnFry)
            {
                QueueDel(pair.worn.Owner);
                Spawn("Ash", Transform(pair.wearer).Coordinates);
                _popupSystem.PopupEntity(Loc.GetString("psionic-burns-up", ("item", pair.worn.Owner)), pair.wearer, PopupType.MediumCaution);
                _audioSystem.PlayPvs("/Audio/Effects/lightburn.ogg", pair.worn.Owner);
            } else
            {
                _popupSystem.PopupEntity(Loc.GetString("psionic-burn-resist", ("item", pair.worn.Owner)), pair.wearer, PopupType.SmallCaution);
                _audioSystem.PlayPvs("/Audio/Effects/lightburn.ogg", pair.worn.Owner);
            }

            // Minor burn damage to the wearer
            var damage = new DamageSpecifier();
            damage.DamageDict.Add("Heat", 5);
            _damageableSystem.TryChangeDamage(pair.wearer, damage);
        }
    }
}

// Placeholder component - you'll need to implement this based on your tinfoil hat system
public sealed partial class TinfoilHatComponent : Component
{
    [DataField("destroyOnFry")]
    public bool DestroyOnFry = true;
}
