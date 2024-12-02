using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Heretic.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Teleportation;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Text;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticBladeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly HereticCombatMarkSystem _combatMark = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticBladeComponent, UseInHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticBladeComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HereticBladeComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public void ApplySpecialEffect(EntityUid performer, EntityUid target)
    {
        if (!TryComp<HereticComponent>(performer, out var hereticComp))
            return;

        switch (hereticComp.CurrentPath)
        {
            case "Ash":
                _flammable.AdjustFireStacks(target, 2.5f, ignite: true);
                break;

            case "Blade":
                // check event handler
                break;

            case "Flesh":
                // ultra bleed
                _blood.TryModifyBleedAmount(target, 1.5f);
                break;

            case "Lock":
                // todo: do something that has weeping and avulsion in it
                if (_random.Next(0, 10) >= 8)
                    _blood.TryModifyBleedAmount(target, 10f);
                break;

            case "Void":
                if (TryComp<TemperatureComponent>(target, out var temp))
                    _temp.ForceChangeTemperature(target, temp.CurrentTemperature - 5f, temp);
                break;

            default:
                return;
        }
    }

    private void OnInteract(Entity<HereticBladeComponent> ent, ref UseInHandEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        // void path exclusive
        if (heretic.CurrentPath == "Void" && heretic.PathStage >= 7)
        {
            var look = _lookupSystem.GetEntitiesInRange<HereticCombatMarkComponent>(Transform(ent).Coordinates, 20f);
            if (look.Count > 0)
            {
                var targetCoords = Transform(look.ToList()[0]).Coordinates;
                _xform.SetCoordinates(args.User, targetCoords);
            }
        }
        else
        {
            if (!TryComp<RandomTeleportComponent>(ent, out var rtp))
                return;

            _teleport.RandomTeleport(args.User, rtp);
            QueueDel(ent);
        }

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/tesla_consume.ogg"), args.User);

        args.Handled = true;
    }

    private void OnExamine(Entity<HereticBladeComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<HereticComponent>(args.Examiner, out var heretic))
            return;

        var isUpgradedVoid = heretic.CurrentPath == "Void" && heretic.PathStage >= 7;

        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("heretic-blade-examine"));
        if (isUpgradedVoid) sb.AppendLine(Loc.GetString("heretic-blade-void-examine"));

        args.PushMarkup(sb.ToString());
    }

    private void OnMeleeHit(Entity<HereticBladeComponent> ent, ref MeleeHitEvent args)
    {
        if (string.IsNullOrWhiteSpace(ent.Comp.Path))
            return;

        if (ent.Comp.Path == "Flesh" && HasComp<GhoulComponent>(args.User))
            args.BonusDamage += args.BaseDamage; // "ghouls can use bloody blades effectively... so real..."

        if (!TryComp<HereticComponent>(args.User, out var hereticComp))
            return;

        foreach (var hit in args.HitEntities)
        {
            // does not work on other heretics
            if (HasComp<HereticComponent>(hit))
                continue;

            if (TryComp<HereticCombatMarkComponent>(hit, out var mark))
            {
                _combatMark.ApplyMarkEffect(hit, ent.Comp.Path, args.User);
                RemComp(hit, mark);
            }

            if (hereticComp.PathStage >= 7)
                ApplySpecialEffect(args.User, hit);
        }

        // blade path exclusive.
        if (HasComp<SilverMaelstromComponent>(args.User))
        {
            args.BonusDamage += args.BaseDamage; // double it.
            if (TryComp<DamageableComponent>(args.User, out var dmg))
            {
                // -5 to all damage types
                // if infused do -10. why? gaming.
                var bonusHeal = HasComp<MansusInfusedComponent>(ent) ? 10f : 5f;

                var orig = dmg.Damage.DamageDict;
                foreach (var k in orig.Keys)
                    orig[k] = MathF.Max((float) orig[k] - bonusHeal, 0f);

                _damage.SetDamage(args.User, dmg, new() { DamageDict = orig });
            }
        }
    }
}
