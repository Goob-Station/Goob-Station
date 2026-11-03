using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlimeRegrowth;

public sealed partial class SlimeRegrowthSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private HungerSystem _hunger = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeRegrowthComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlimeRegrowthComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlimeRegrowthComponent, SlimeRegrowLimbActionEvent>(OnRegrowAction);
        SubscribeLocalEvent<SlimeRegrowthComponent, SlimeRegrowLimbDoAfterEvent>(OnRegrowDoAfter);
    }

    private void OnMapInit(Entity<SlimeRegrowthComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnShutdown(Entity<SlimeRegrowthComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnRegrowAction(Entity<SlimeRegrowthComponent> ent, ref SlimeRegrowLimbActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanRegrow(ent, out var failure))
        {
            _popup.PopupClient(Loc.GetString(failure), ent, ent);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.Delay, new SlimeRegrowLimbDoAfterEvent(), ent, target: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupPredicted(Loc.GetString("slime-regrowth-start-self"),
            Loc.GetString("slime-regrowth-start-others", ("user", ent)),
            ent,
            ent);
        args.Handled = true;
    }

    private void OnRegrowDoAfter(Entity<SlimeRegrowthComponent> ent, ref SlimeRegrowLimbDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        if (!CanRegrow(ent, out var failure))
        {
            _popup.PopupEntity(Loc.GetString(failure), ent, ent);
            return;
        }

        var grown = new HashSet<EntityUid>();
        while (TryFindMissingPart(ent, grown.Count == 0 ? null : grown, out var missing))
        {
            var part = Spawn(missing.Proto, new EntityCoordinates(missing.Parent, Vector2.Zero));
            if (!_body.TryCreatePartSlotAndAttach(missing.Parent, missing.Slot, part, missing.Type, missing.Symmetry))
            {
                Del(part);
                break;
            }

            grown.Add(part);
        }

        if (grown.Count == 0)
            return;

        if (TryComp<HungerComponent>(ent, out var hunger))
            _hunger.ModifyHunger(ent, -ent.Comp.HungerCost, hunger);

        _bloodstream.TryModifyBloodLevel(ent.Owner, -ent.Comp.BloodCost);
        _body.RestoreMissingOrgans(ent.Owner);

        if (ent.Comp.Damage is { } damage)
        {
            foreach (var part in grown)
            {
                _damageable.TryChangeDamage(ent.Owner,
                    damage,
                    ignoreResistances: true,
                    interruptsDoAfters: false,
                    targetPart: _body.GetTargetBodyPart(part),
                    splitDamage: SplitDamageBehavior.None,
                    canMiss: false);
            }
        }

        _audio.PlayPvs(ent.Comp.Sound, ent);
        _popup.PopupEntity(Loc.GetString("slime-regrowth-finish-others", ("user", ent)), ent, Filter.PvsExcept(ent), true);
        _popup.PopupEntity(Loc.GetString("slime-regrowth-finish-self"), ent, ent);
    }

    private bool CanRegrow(Entity<SlimeRegrowthComponent> ent, [NotNullWhen(false)] out string? failure)
    {
        failure = null;

        if (!TryFindMissingPart(ent, null, out _))
            failure = "slime-regrowth-nothing-missing";
        else if (TryComp<HungerComponent>(ent, out var hunger) &&
                 _hunger.GetHunger(hunger) - ent.Comp.HungerCost < ent.Comp.MinHunger)
            failure = "slime-regrowth-too-hungry";
        else if (TryComp<BloodstreamComponent>(ent, out var blood) &&
                 _bloodstream.GetBloodLevel((ent, blood)) < ent.Comp.MinBloodLevel)
            failure = "slime-regrowth-too-little-blood";

        return failure == null;
    }

    private bool TryFindMissingPart(Entity<SlimeRegrowthComponent> ent, HashSet<EntityUid>? parents, out MissingPart missing)
    {
        missing = default;

        if (!TryComp<BodyComponent>(ent, out var body) ||
            body.Prototype is not { } protoId ||
            !_prototype.TryIndex(protoId, out var prototype) ||
            !_body.TryGetRootPart(ent, out var root, body))
            return false;

        var visited = new HashSet<string> { prototype.Root };
        var frontier = new Queue<(string Slot, EntityUid Part)>();
        frontier.Enqueue((prototype.Root, root.Value.Owner));

        while (frontier.TryDequeue(out var current))
        {
            foreach (var connection in prototype.Slots[current.Slot].Connections)
            {
                if (!visited.Add(connection))
                    continue;

                if (_container.TryGetContainer(current.Part, SharedBodySystem.GetPartSlotContainerId(connection), out var container) &&
                    container.ContainedEntities.Count > 0)
                {
                    frontier.Enqueue((connection, container.ContainedEntities[0]));
                    continue;
                }

                if (parents != null && !parents.Contains(current.Part))
                    continue;

                if (prototype.Slots[connection].Part is not { } partProto ||
                    !_prototype.Index(partProto).TryGetComponent<BodyPartComponent>(out var part, Factory) ||
                    !ent.Comp.Parts.Contains(part.PartType))
                    continue;

                missing = new MissingPart(current.Part, connection, partProto, part.PartType, part.Symmetry);
                return true;
            }
        }

        return false;
    }

    private readonly record struct MissingPart(
        EntityUid Parent,
        string Slot,
        EntProtoId Proto,
        BodyPartType Type,
        BodyPartSymmetry Symmetry);
}
