// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Traitor;
using Content.Server.Store.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Implants;
using Content.Shared.Mind;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Traitor.Uplink;

// goobstation - heavily edited. fuck newstore
// do not touch unless you want to shoot yourself in the leg
public sealed class UplinkSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermalImplant = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly GoobCommonUplinkSystem _goobUplink = default!;

    public static readonly ProtoId<CurrencyPrototype> TelecrystalCurrencyPrototype = "Telecrystal";
    private static readonly EntProtoId FallbackUplinkImplant = "UplinkImplant";
    private static readonly ProtoId<ListingPrototype> FallbackUplinkCatalog = "UplinkUplinkImplanter";

    /// <summary>
    /// Adds an uplink to the target based on their preference
    /// Falls back to implant if the preferred target entity is not found
    /// </summary>
    public bool TryAddUplink(
        EntityUid user,
        FixedPoint2 balance,
        ProtoId<UplinkPreferencePrototype> preferenceId,
        out EntityUid? uplinkTarget,
        out SetupUplinkEvent? setupEvent)
    {
        var preference = _proto.Index(preferenceId);
        uplinkTarget = null;
        setupEvent = null;

        if (preference.SearchComponents != null)
            uplinkTarget = _goobUplink.FindUplinkTarget(user, preference.SearchComponents);

        if (uplinkTarget == null)
            return ImplantUplink(user, balance);

        EnsureComp<UplinkComponent>(uplinkTarget.Value);
        SetUplink(user, uplinkTarget.Value, balance);

        var ev = new SetupUplinkEvent { User = user };
        RaiseLocalEvent(uplinkTarget.Value, ref ev);
        setupEvent = ev;

        return true;
    }

    /// <summary>
    /// Legacy method for backwards compatibility.
    /// Adds an uplink to the target, auto-detecting location (prefers PDA).
    /// </summary>
    public bool AddUplinkAutoDetect(EntityUid user, FixedPoint2 balance, EntityUid? uplinkEntity = null)
    {
        uplinkEntity ??= _goobUplink.FindUplinkTarget(user, new[] { "Pda", "Pen" });

        if (uplinkEntity == null)
            return ImplantUplink(user, balance);

        EnsureComp<UplinkComponent>(uplinkEntity.Value);
        SetUplink(user, uplinkEntity.Value, balance);

        return true;
    }

    /// <summary>
    /// Configure TC for the uplink
    /// </summary>
    private void SetUplink(EntityUid user, EntityUid uplink, FixedPoint2 balance)
    {
        if (!_mind.TryGetMind(user, out var mind, out _))
            return;

        var store = EnsureComp<StoreComponent>(uplink);

        store.AccountOwner = mind;

        store.Balance.Clear();
        var bal = new Dictionary<string, FixedPoint2> { { TelecrystalCurrencyPrototype, balance } };
        _store.TryAddCurrency(bal, uplink, store);
    }

    /// <summary>
    /// Implant an uplink as a fallback measure if the traitor had no PDA
    /// </summary>
    private bool ImplantUplink(EntityUid user, FixedPoint2 balance)
    {
        if (!_proto.TryIndex<ListingPrototype>(FallbackUplinkCatalog, out var catalog))
            return false;

        if (!catalog.Cost.TryGetValue(TelecrystalCurrencyPrototype, out var cost))
            return false;

        if (balance < cost) // Can't use Math functions on FixedPoint2
            balance = 0;
        else
            balance = balance - cost;

        var implant = _subdermalImplant.AddImplant(user, FallbackUplinkImplant);

        if (!HasComp<StoreComponent>(implant))
            return false;

        SetUplink(user, implant.Value, balance);
        return true;
    }
}
