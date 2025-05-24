using Content.Server._DV.Objectives.Events;
using Content.Server.Antag;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.Silicons;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.Radio;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicFragmentationSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private ProtoId<RadioChannelPrototype> _cultRadio = "CosmicRadio";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AILawUpdatedEvent>(OnLawInserted);

        SubscribeLocalEvent<BorgChassisComponent, MalignFragmentationEvent>(OnFragmentBorg);
        SubscribeLocalEvent<SiliconLawUpdaterComponent, MalignFragmentationEvent>(OnFragmentAi);

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentation>(OnCosmicFragmentation);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentationDoAfter>(OnCosmicFragmentationDoAfter);
    }

    private void UnEmpower(Entity<CosmicCultComponent> ent)
    {
        var comp = ent.Comp;
        comp.CosmicEmpowered = false; // empowerment spent! Now we set all the values back to their default.
        comp.CosmicSiphonQuantity = CosmicCultComponent.DefaultCosmicSiphonQuantity;
        comp.CosmicGlareRange = CosmicCultComponent.DefaultCosmicGlareRange;
        comp.CosmicGlareDuration = CosmicCultComponent.DefaultCosmicGlareDuration;
        comp.CosmicGlareStun = CosmicCultComponent.DefaultCosmicGlareStun;
        comp.CosmicImpositionDuration = CosmicCultComponent.DefaultCosmicImpositionDuration;
        comp.CosmicBlankDuration = CosmicCultComponent.DefaultCosmicBlankDuration;
        comp.CosmicBlankDelay = CosmicCultComponent.DefaultCosmicBlankDelay;
    }

    private void OnCosmicFragmentation(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentation args)
    {
        if (args.Handled || HasComp<ActiveNPCComponent>(args.Target) || _mobStateSystem.IsIncapacitated(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), ent, ent);
            return;
        }

        var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.CosmicSiphonDelay, new EventCosmicFragmentationDoAfter(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = false,
            BreakOnHandChange = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        };
        args.Handled = true;
        _doAfter.TryStartDoAfter(doargs);
        _cult.MalignEcho(ent);
        UnEmpower(ent);
    }

    private void OnCosmicFragmentationDoAfter(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentationDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        var evt = new MalignFragmentationEvent(ent, target);
        RaiseLocalEvent(target, ref evt);
    }

    private void OnFragmentBorg(Entity<BorgChassisComponent> ent, ref MalignFragmentationEvent args)
    {
        if (_polymorph.PolymorphEntity(args.Target, "CosmicFragmentationWisp") is not { } polyVictim)
            return;

        var chantry = Spawn("CosmicBorgChantry", Transform(polyVictim).Coordinates);
        EnsureComp<CosmicChantryComponent>(chantry, out var chantryComponent);
        chantryComponent.PolyVictim = polyVictim;
        chantryComponent.Victim = args.Target;

        var mins = chantryComponent.EventTime.Minutes;
        var secs = chantryComponent.EventTime.Seconds;
        _antag.SendBriefing(polyVictim, Loc.GetString("cosmiccult-silicon-chantry-briefing", ("minutesandseconds", $"{mins} minutes and {secs} seconds")), Color.FromHex("#4cabb3"), null);
    }

    private void OnFragmentAi(Entity<SiliconLawUpdaterComponent> ent, ref MalignFragmentationEvent args)
    {
        var lawboard = Spawn("CosmicCultLawBoard", Transform(args.Target).Coordinates);
        _container.TryGetContainer(args.Target, "circuit_holder", out var container);
        if (container == null)
            return;
        _container.EmptyContainer(container, true);
        _container.Insert(lawboard, container, Transform(args.Target), true);
    }

    private void OnLawInserted(AILawUpdatedEvent args)
    {
        if (!TryComp<IntrinsicRadioTransmitterComponent>(args.Target, out var radio) || !TryComp<ActiveRadioComponent>(args.Target, out var transmitter))
            return;
        if (args.Lawset.Id == "CosmicCultLaws")
        {
            radio.Channels.Add(_cultRadio);
            transmitter.Channels.Add(_cultRadio);
            _antag.SendBriefing(args.Target, Loc.GetString("cosmiccult-silicon-subverted-briefing"), Color.FromHex("#4cabb3"), null);
        }
        else
        {
            radio.Channels.Remove(_cultRadio);
            transmitter.Channels.Remove(_cultRadio);
        }
    }
}

[ByRefEvent]
public record struct MalignFragmentationEvent(Entity<CosmicCultComponent> User, EntityUid Target);
