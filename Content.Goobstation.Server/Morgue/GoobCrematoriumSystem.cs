using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Morgue;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Server.Administration.Logs;
using Content.Server.Morgue;
using Content.Server.Morgue.Components;
using Content.Server.Popups;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Access.Systems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Tag;
using Moonyware.Miscellaneous.Systems;
using Robust.Server.Audio;
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
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    private static readonly ProtoId<TagPrototype> HighRiskItemTag = "HighRiskItem";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrematoriumComponent, GotEmaggedEvent>(OnEmagged);
    }

    public override bool IsAllowed(EntityUid uid, EntityUid user)
    {
        var component = Comp<CrematoriumComponent>(uid);

        if (!_accessReader.IsAllowed(user, uid))
        {
            _audio.PlayPvs(component.CremateDeniedSound, uid);
            // Why do we have multiple "No Access"/"Access Denied" fluent locs? Can't there be like a "no-access" string and thats it?
            // Why make a new one each time?
            #region Things that have a duplicate "no access" ftl:
            /*
             * news
             * docking console
             * cryostorage
             * cargo console
             * netconf
             * gateway
             * RestrictById in general
             * APC
             * air alarm
             * holopad
             * deployable turret
             * emerg shuttle
             * lock comp
             * vending machine
             * gascans
             *
             */
            #endregion
            _popup.PopupEntity(Loc.GetString("news-write-no-access-popup"), uid);
            return false;
        }

        return true;
    }

    public override bool CanCremate(EntityUid ent, EntityUid target, [NotNullWhen(false)] out string? reason)
    {
        reason = Loc.GetString("crematorium-cant-cremate");
        var comp = Comp<CrematoriumComponent>(ent);

        if (HasComp<CrematoriumImmuneComponent>(target))
            return false;

        if (!HasComp<EmaggedComponent>(ent) && !_mob.IsDead(target))
        {
            reason = Loc.GetString("crematorium-not-dead");
            return false;
        }

        if (comp.DemandStrip && TryComp<InventoryComponent>(target, out var inv) && HasItems(target, inv))
        {
            reason = Loc.GetString("crematorium-has-items");
            return false;
        }

        // The entity we're burning might not neccessarily be a mob, and we want to be checking for high risk items
        // Can this be meta'd to find high risk items in storage implants? Absolutely.
        // Dealing with a deleted high risk item is worse than dealing with a metagaming player
        if (_storage.FindFirstStoredByTag(target, HighRiskItemTag).Length != 0)
        {
            reason = Loc.GetString("crematorium-unknown-obstruction");
            return false;
        }

        return true;
    }

    public override void Execute(EntityUid uid, EntityUid target)
    {
        var comp = Comp<CrematoriumComponent>(uid);

        _damage.TryChangeDamage(target, comp.Damage, splitDamage: SplitDamageBehavior.None);
    }

    public override void LogPassedChecks(EntityUid user, EntityUid target)
    {
        var log = Loc.GetString("crematorium-passed-cremate-log",
        [
            ("user", ToPrettyString(user)),
            ("target", ToPrettyString(target)),
        ]);

                                                // Can I kill everyone involved?                                    Pretty please
        _adminLog.Add(LogType.Verb, LogImpact.High, $"{log}");
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
        if (args.Type != EmagType.Interaction)
            return;

        if (HasComp<EmaggedComponent>(ent.Owner))
            return;

        // It's an important thing innit
        _adminLog.Add(LogType.Emag, LogImpact.Extreme, $"{Loc.GetString("crematorium-emagged", ("user", ToPrettyString(args.UserUid)))}");

        args.Handled = true;
    }
}
