using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Systems;
using Content.Server.Antag;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Werewolf.Systems;

public sealed class WerewolfCursedBiteSystem : SharedWerewolfCursedBiteSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;

    private EntityQuery<BloodstreamComponent> _bloodQuery;

    public override void Initialize()
    {
        base.Initialize();

        _bloodQuery = GetEntityQuery<BloodstreamComponent>();

    }
    protected override void TryModifyBleeding(EntityUid target, float amount)
    {
        base.TryModifyBleeding(target, amount);

        if (!_bloodQuery.TryComp(target, out var blood))
            return;

        _bloodstreamSystem.TryModifyBloodLevel(target, amount, blood);
        _bloodstreamSystem.TryModifyBleedAmount(target, blood.MaxBleedAmount, blood);
    }

    protected override void MakeAntagWerewolf(EntityUid target)
    {
        base.MakeAntagWerewolf(target);

        if (!TryComp<ActorComponent>(target, out var actor))
            return;

        _antagSelection.ForceMakeAntag<WerewolfComponent>(actor.PlayerSession, "Werewolf");
    }
}
