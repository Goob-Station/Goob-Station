using System.Linq;
using Content.Server.DoAfter;
using Content.Server.Emp;
using Content.Server.GameTicking;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.DoAfter;
using Content.Shared.WhiteDream.BloodCult.Runes;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.WhiteDream.BloodCult.Runes.Apocalypse;

public sealed class CultRuneApocalypseSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuneApocalypseComponent, TryInvokeCultRuneEvent>(OnApocalypseRuneInvoked);
        SubscribeLocalEvent<CultRuneApocalypseComponent, ApocalypseRuneDoAfter>(OnDoAfter);
    }

    private void OnApocalypseRuneInvoked(Entity<CultRuneApocalypseComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        if (ent.Comp.Used)
        {
            args.Cancel();
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, args.User, ent.Comp.InvokeTime, new ApocalypseRuneDoAfter(), ent)
        {
            BreakOnUserMove = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<CultRuneApocalypseComponent> ent, ref ApocalypseRuneDoAfter args)
    {
        if (args.Cancelled || EntityManager.EntityQuery<BloodCultRuleComponent>().FirstOrDefault() is not { } cultRule)
            return;

        ent.Comp.Used = true;
        _appearance.SetData(ent, ApocalypseRuneVisuals.Used, true);

        _emp.EmpPulse(_transform.GetMapCoordinates(ent),
            ent.Comp.EmpRange,
            ent.Comp.EmpEnergyConsumption,
            ent.Comp.EmpDuration);

        foreach (var guaranteedEvent in ent.Comp.GuaranteedEvents)
        {
            _gameTicker.StartGameRule(guaranteedEvent);
        }

        var requiredCultistsThreshold = MathF.Floor(_playerManager.PlayerCount * ent.Comp.CultistsThreshold);
        var totalCultists = cultRule.Cultists.Count + cultRule.Constructs.Count;
        if (totalCultists >= requiredCultistsThreshold)
            return;

        var (randomEvent, repeatTimes) = _random.Pick(ent.Comp.PossibleEvents);
        for (var i = 0; i < repeatTimes; i++)
        {
            _gameTicker.StartGameRule(randomEvent);
        }
    }
}
