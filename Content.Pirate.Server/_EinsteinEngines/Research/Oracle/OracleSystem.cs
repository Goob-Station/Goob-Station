using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Botany;
using Content.Pirate.Server._EinsteinEngines.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Chat;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Psionics.Passives;
using Content.Shared.Random.Helpers;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Research.Oracle;

public sealed class OracleSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TelepathicChatSystem _tChat = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PuddleSystem _puddles = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SolutionContainerSystem _solutions = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<OracleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.NextDemandTime)
            {
                // Might be null if this is the first tick. In that case this will simply initialize it.
                var last = (EntityPrototype?) comp.DesiredPrototype;
                if (NextItem((uid, comp)))
                    comp.LastDesiredPrototype = last;
            }

            if (_timing.CurTime >= comp.NextBarkTime)
            {
                comp.NextBarkTime = _timing.CurTime + comp.BarkDelay;

                var message = Loc.GetString(_random.Pick(comp.DemandMessages), ("item", comp.DesiredPrototype.Name)).ToUpper();
                _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, false);
            }
        }

        query.Dispose();
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OracleComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<OracleComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractHand(Entity<OracleComponent> oracle, ref InteractHandEvent args)
    {
        if (!HasComp<PsionicComponent>(args.User) || HasComp<PsionicInsulationComponent>(args.User)
            || !TryComp<ActorComponent>(args.User, out var actor))
            return;

        SendTelepathicInfo(oracle, actor.PlayerSession.Channel,
            Loc.GetString("oracle-current-item", ("item", oracle.Comp.DesiredPrototype.Name)), HasComp<PsychognomistComponent>(args.User));

        if (oracle.Comp.LastDesiredPrototype != null)
            SendTelepathicInfo(oracle, actor.PlayerSession.Channel,
                Loc.GetString("oracle-previous-item", ("item", oracle.Comp.LastDesiredPrototype.Name)), HasComp<PsychognomistComponent>(args.User));
    }

    private void OnInteractUsing(Entity<OracleComponent> oracle, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<MobStateComponent>(args.Used) || !TryComp<MetaDataComponent>(args.Used, out var meta) || meta.EntityPrototype == null)
            return;

        var requestValid = IsCorrectItem(meta.EntityPrototype, oracle.Comp.DesiredPrototype);
        var updateRequest = true;

        if (oracle.Comp.LastDesiredPrototype != null &&
            IsCorrectItem(meta.EntityPrototype, oracle.Comp.LastDesiredPrototype))
        {
            updateRequest = false;
            requestValid = true;
            oracle.Comp.LastDesiredPrototype = null;
        }

        if (!requestValid)
        {
            if (!HasComp<RefillableSolutionComponent>(args.Used) &&
                _timing.CurTime >= oracle.Comp.NextRejectTime)
            {
                oracle.Comp.NextRejectTime = _timing.CurTime + oracle.Comp.RejectDelay;
                _chat.TrySendInGameICMessage(oracle, _random.Pick(oracle.Comp.RejectMessages), InGameICChatType.Speak, true);
            }

            return;
        }

        DispenseRewards(oracle, Transform(args.User).Coordinates);
        QueueDel(args.Used);
        args.Handled = true;

        if (updateRequest)
            NextItem(oracle);
    }

    private void SendTelepathicInfo(Entity<OracleComponent> oracle, INetChannel client, string message, bool psychognomist = false)
    {
        if (!psychognomist)
        {
            var messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
                ("telepathicChannelName", Loc.GetString("chat-manager-telepathic-channel-name")),
                ("message", message));

            _chatMan.ChatMessageToOne(ChatChannel.Telepathic,
                message, messageWrap, oracle, false, client, Color.PaleVioletRed);
        }
        else
        {
            var descriptor = _tChat.SourceToDescriptor(oracle);
            var psychogMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-psychognomy",
                ("source", descriptor.ToUpper()), ("message", message));

            _chatMan.ChatMessageToOne(ChatChannel.Telepathic, message, psychogMessageWrap, oracle, false, client, Color.PaleVioletRed);
        }
    }

    private bool IsCorrectItem(EntityPrototype given, EntityPrototype target)
    {
        // Nyano, what is this shit?
        // Why are we comparing by name instead of prototype id?
        // Why is this ever necessary?
        // What were you trying to accomplish?!
        if (given.Name == target.Name)
            return true;

        return false;
    }

    private void DispenseRewards(Entity<OracleComponent> oracle, EntityCoordinates throwTarget)
    {
        foreach (var rewardRandom in oracle.Comp.RewardEntities)
        {
            // Spawn each reward next to oracle and throw towards the target
            var rewardProto = _protoMan.Index(rewardRandom).Pick(_random);
            var reward = EntityManager.SpawnNextToOrDrop(rewardProto, oracle);
            _throwing.TryThrow(reward, throwTarget, recoil: false);
        }

        DispenseLiquidReward(oracle);
    }

    private void DispenseLiquidReward(Entity<OracleComponent> oracle)
    {
        if (!_solutions.TryGetSolution(oracle.Owner, OracleComponent.SolutionName, out var fountainSol))
            return;

        // Why is this hardcoded?
        var amount = MathF.Round(20 + _random.Next(1, 30) + (float) _glimmer.GlimmerOutput / 10f);
        var temporarySol = new Solution();
        var reagent = _protoMan.Index(oracle.Comp.RewardReagents).Pick(_random);

        if (_random.Prob(oracle.Comp.AbnormalReagentChance))
        {
            var allReagents = _protoMan.EnumeratePrototypes<ReagentPrototype>()
                .Where(x => !x.Abstract)
                .Select(x => x.ID).ToList();

            reagent = _random.Pick(allReagents);
        }

        temporarySol.AddReagent(reagent, amount);
        _solutions.TryMixAndOverflow(fountainSol.Value, temporarySol, fountainSol.Value.Comp.Solution.MaxVolume, out var overflowing);

        if (overflowing != null && overflowing.Volume > 0)
            _puddles.TrySpillAt(oracle, overflowing, out var _);
    }

    private bool NextItem(Entity<OracleComponent> oracle)
    {
        oracle.Comp.NextBarkTime = oracle.Comp.NextRejectTime = TimeSpan.Zero;
        oracle.Comp.NextDemandTime = _timing.CurTime + oracle.Comp.DemandDelay;

        var protoId = GetDesiredItem(oracle);
        if (protoId != null && _protoMan.TryIndex<EntityPrototype>(protoId, out var proto))
        {
            oracle.Comp.DesiredPrototype = proto;
            return true;
        }

        return false;
    }

    // TODO: find a way to not just use string literals here (weighted random doesn't support enums)
    private string? GetDesiredItem(Entity<OracleComponent> oracle)
    {
        var demand = _protoMan.Index(oracle.Comp.DemandTypes).Pick(_random);

        string? proto;
        if (demand == "tech" && GetRandomTechProto(oracle, out proto))
            return proto;

        // This is also a fallback for when there's no research server to form an oracle tech request.
        if (demand is "plant" or "tech" && GetRandomPlantProto(oracle, out proto))
            return proto;

        return null;
    }

    private bool GetRandomTechProto(Entity<OracleComponent> oracle, [NotNullWhen(true)] out string? proto)
    {
        // Try to find the most advanced server.
        var database = _research.GetServerIds()
            .Select(x => _research.TryGetServerById(x, out var serverUid, out _) ? serverUid : null)
            .Where(x => x != null && Transform(x.Value).GridUid == Transform(oracle).GridUid)
            .Select(x =>
            {
                TryComp<TechnologyDatabaseComponent>(x!.Value, out var comp);
                return new Entity<TechnologyDatabaseComponent?>(x.Value, comp);
            })
            .Where(x => x.Comp != null)
            .OrderByDescending(x =>
                _research.GetDisciplineTiers(x.Comp!).Select(pair => pair.Value).Max())
            .FirstOrDefault(EntityUid.Invalid);

        if (database.Owner == EntityUid.Invalid)
        {
            Log.Warning($"Cannot find an applicable server on grid {Transform(oracle).GridUid} to form an oracle request.");
            proto = null;
            return false;
        }

        // Select a technology that's either already unlocked, or can be unlocked from current research
        var techs = _protoMan.EnumeratePrototypes<TechnologyPrototype>()
            .Where(x => !x.Hidden)
            .Where(x =>
                _research.IsTechnologyUnlocked(database.Owner, x, database.Comp)
                || _research.IsTechnologyAvailable(database.Comp!, x))
            .SelectMany(x => x.RecipeUnlocks)
            .Select(x => _protoMan.Index(x).Result)
            .Where(x => IsDemandValid(oracle, x))
            .ToList();

        // Unlikely.
        if (techs.Count == 0)
        {
            proto = null;
            return false;
        }

        proto = _random.Pick(techs);

        if (proto == null)
            return false;

        return true;
    }

    private bool GetRandomPlantProto(Entity<OracleComponent> oracle, [NotNullWhen(true)] out string? proto)
    {
        var allPlants = _protoMan.EnumeratePrototypes<SeedPrototype>()
            .Select(x => x.ProductPrototypes.FirstOrDefault())
            .Where(x => IsDemandValid(oracle, x))
            .ToList();

        if (allPlants.Count == 0)
        {
            proto = null;
            return false;
        }

        proto = _random.Pick(allPlants)!;
        return true;
    }

    private bool IsDemandValid(Entity<OracleComponent> oracle, EntProtoId? id)
    {
        if (id == null || oracle.Comp.BlacklistedDemands.Contains(id.Value))
            return false;

        return _protoMan.TryIndex(id, out var proto) && proto.Components.ContainsKey("Item");
    }
}
