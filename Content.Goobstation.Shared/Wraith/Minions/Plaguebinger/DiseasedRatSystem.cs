using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Minions.Plaguebringer;

/// <summary>
/// This handles the system for the diseased rat evolving into its bigger stages.
/// </summary>
public abstract class SharedDiseasedRatSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseasedRatComponent, AteFilthEvent>(OnEatFilth);
    }

    private void OnEatFilth(Entity<DiseasedRatComponent> ent, ref AteFilthEvent args)
    {
        if (!CanEvolve(ent, args.CurrentFilthConsumed, out var formToEvolve))
            return;

        Evolve(ent.Owner, formToEvolve.Value);
    }

    #region Helpers

    private bool CanEvolve(
        Entity<DiseasedRatComponent> ent,
        int filthConsumed,
        [NotNullWhen(true)] out ProtoId<DiseasedRatFormUnlockPrototype>? unlockedForm)
    {
        foreach (var form in ent.Comp.DiseasedRatForms)
        {
            if (!_proto.TryIndex(form, out var formIndex)
                || filthConsumed < formIndex.FilthRequired)
                continue;

            ent.Comp.DiseasedRatForms.Remove(formIndex);
            Dirty(ent);

            unlockedForm = formIndex;
            return true;
        }

        unlockedForm = null;
        return false;
    }
    #endregion

    #region Server
    protected virtual void Evolve(EntityUid uid, ProtoId<DiseasedRatFormUnlockPrototype> newProto) {}

    #endregion
}
