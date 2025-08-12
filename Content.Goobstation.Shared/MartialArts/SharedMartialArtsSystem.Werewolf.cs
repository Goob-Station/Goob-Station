using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    /// <inheritdoc/>
    public void InitializeWerewolf()
    {
        // Component lifetime related
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentStartup>(OnWerewolfMovesStartup);
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentShutdown>(OnWerewolfMovesShutdown);

        // Combos
        SubscribeLocalEvent<CanPerformComboComponent, OpenVeinPerformedEvent>(OnOpenVeinPerformed);
    }

    #region Generic Events
    private void OnWerewolfMovesStartup(Entity<GrantWerewolfMovesComponent> ent, ref ComponentStartup args)
    {
        if (!_netManager.IsServer)
            return;

        TryGrantMartialArt(ent.Owner, ent.Comp);
    }

    private void OnWerewolfMovesShutdown(Entity<GrantWerewolfMovesComponent> ent, ref ComponentShutdown args)
    {
        var user = ent.Owner;
        if (!HasComp<MartialArtsKnowledgeComponent>(user))
            return;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }
    #endregion

    #region Martial Arts

    private void OnOpenVeinPerformed(Entity<CanPerformComboComponent> ent, ref OpenVeinPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed)
            return;

        DoDamage(ent.Owner, target, proto.DamageType, proto.ExtraDamage, out _);

        if (_netManager.IsServer)
        {
            TryModifyBleeding(target, args.BleedAmount);
        }

        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    #endregion

    #region Server-Side

    protected virtual void TryModifyBleeding(EntityUid target, float amount) {}

    #endregion
}
