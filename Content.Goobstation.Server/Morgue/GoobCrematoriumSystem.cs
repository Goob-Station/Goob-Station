using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Morgue;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Morgue;
using Content.Server.Morgue.Components;
using Content.Shared.Access.Components;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Tag;
using Moonyware.Miscellaneous.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Morgue;

/// <summary>
/// An extension to <see cref="CrematoriumSystem"/> without intruding into it too much
/// </summary>
public sealed class GoobCrematoriumSystem : CommonGoobCrematoriumSystem
{
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly StorageLookupSystem _storage = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly InventorySystem _inv = default!;

    private static readonly ProtoId<TagPrototype> HighRiskItemTag = "HighRiskItem";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrematoriumComponent, GotEmaggedEvent>(OnEmagged);
    }

    public override bool CanCremate(EntityUid ent, EntityUid target, [NotNullWhen(false)] out string? reason)
    {
        reason = Loc.GetString("crematorium-cant-cremate");

        if (HasComp<CrematoriumImmuneComponent>(target))
            return false;

        if (!HasComp<EmaggedComponent>(ent))
        {
            if (!_mob.IsDead(target))
                return false;

            if (TryComp<MindContainerComponent>(target, out var mindCont) && mindCont.Mind != null)
                return false;
        }

        if (HasComp<MobStateComponent>(target) && HasItems(target))
        {
            reason = Loc.GetString("crematorium-has-items");
            return false;
        }

        // The entity we're burning might no neccessarily be a mob, and we're checking for high risk items
        // Can this be meta'd to find high risk items in storage implants? Absolutely.
        // Dealing with a deleted high risk item is worse than dealing with a metagaming player
        if (_storage.FindFirstStoredByTag(target, HighRiskItemTag).Length != 0)
        {
            reason = Loc.GetString("crematorium-has-items");
            return false;
        }

        return true;
    }

    public override void TryDeleteItems(EntityUid ent, EntityUid crematorium)
    {
        // Todo inv checks, this should blow up if there are any high risk items
        throw new NotImplementedException();
    }

    private bool HasItems(EntityUid ent, InventoryComponent? inv = null)
    {
        if (!Resolve(ent, ref inv))
            return false;

        var slotenum = _inv.GetSlotEnumerator((ent, inv), flags: SlotFlags.All);
        while (slotenum.MoveNext(out var cont))
        {
            if (cont.ContainedEntities.Count != 0)
                return true;
        }

        return false;
    }

    private void OnEmagged(Entity<CrematoriumComponent> ent, ref GotEmaggedEvent args)
    {
        // It's an important thing innit
        _adminLog.Add(LogType.Emag, LogImpact.Extreme, $"{Loc.GetString("crematorium-emagged", ("user", ToPrettyString(args.UserUid)))}");

        // TODO handle item deletion
    }
}
