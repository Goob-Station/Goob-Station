using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._Lavaland.Megafauna.Components;
using Content.Shared.Random.Helpers;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    private bool TryPickMegafaunaAttack(
        IReadOnlyList<MegafaunaAction> actionsData,
        string? previousAttack,
        [NotNullWhen(true)] out MegafaunaAction? attack)
    {
        attack = null;
        if (actionsData.Count == 0)
            return false;

        var resolvedWeights = actionsData.ToDictionary(action => action, action => action.Weight);
        attack = SharedRandomExtensions.Pick(resolvedWeights, _random.GetRandom());

        if (actionsData.Count > 1 && attack.Name == previousAttack)
        {
            // Pick once again if this attack was already picked before
            resolvedWeights.Remove(attack);
            attack = SharedRandomExtensions.Pick(resolvedWeights, _random.GetRandom());
        }

        return true;
    }

    private bool TryPickMegafaunaAttack(Entity<MegafaunaAiComponent, AggressiveMegafaunaAiComponent> ent, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        return TryPickMegafaunaAttack(ent.Comp2.ActionsData, ent.Comp1.PreviousAttack, out action);
    }

    private bool TryPickMegafaunaAttack(Entity<MegafaunaAiComponent, PhasesMegafaunaAiComponent> ent, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        action = null;
        if (!ent.Comp2.PhasedActionsData.TryGetValue(ent.Comp2.CurrentPhase, out var actionData))
            return false;

        return TryPickMegafaunaAttack(actionData, ent.Comp1.PreviousAttack, out action);
    }

    private void PickMissingAttacks(Entity<MegafaunaAiComponent, AggressiveMegafaunaAiComponent> ent)
    {
        if (ent.Comp1.BossAttackQueue.Count > ent.Comp1.AttacksBufferSize)
            return;

        for (int i = 0; i < ent.Comp1.AttacksBufferSize - ent.Comp1.BossAttackQueue.Count; i++)
        {
            if (!TryPickMegafaunaAttack(ent, out var picked))
                continue;

            ent.Comp1.BossAttackQueue.Enqueue(picked);
        }
    }

    private void PickMissingAttacks(Entity<MegafaunaAiComponent, PhasesMegafaunaAiComponent> ent)
    {
        if (ent.Comp1.BossAttackQueue.Count > ent.Comp1.AttacksBufferSize)
            return;

        for (int i = 0; i < ent.Comp1.AttacksBufferSize - ent.Comp1.BossAttackQueue.Count; i++)
        {
            if (!TryPickMegafaunaAttack(ent, out var picked))
                continue;

            ent.Comp1.BossAttackQueue.Enqueue(picked);
        }
    }
}
