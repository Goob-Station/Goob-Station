// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire <lattice@saphi.re>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration.Logs;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Content.Shared.Whitelist;
using Content.Goobstation.Common.Effects; // goob edit
using Content.Goobstation.Common.Emag.Prototypes; // goob edit
using Robust.Shared.Prototypes; // Shitmed - Starlight Abductors

namespace Content.Shared.Emag.Systems;

/// How to add an emag interaction:
/// 1. Go to the system for the component you want the interaction with
/// 2. Subscribe to the GotEmaggedEvent
/// 3. Have some check for if this actually needs to be emagged or is already emagged (to stop charge waste)
/// 4. Past the check, add all the effects you desire and HANDLE THE EVENT ARGUMENT so a charge is spent
/// 5. Optionally, set Repeatable on the event to true if you don't want the emagged component to be added
public sealed class EmagSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!; // DeltaV - Add a whitelist/blacklist to the Emag
    [Dependency] private readonly SparksSystem _sparks = default!; // goob edit - sparks everywhere
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // goob edit - EmagType prototypes

    public readonly Dictionary<string, ProtoId<EmagTypePrototype>> EmagType = new();
    private readonly ProtoId<EmagTypePrototype> _emagAll = "All"; // Goob edit

    public override void Initialize()
    {
        base.Initialize();
        var query = _prototypeManager.EnumeratePrototypes<EmagTypePrototype>();
        foreach (var emagType in query)
        {
            EmagType[emagType.ID] = emagType;
        }
        SubscribeLocalEvent<EmagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<EmaggedComponent, OnAccessOverriderAccessUpdatedEvent>(OnAccessOverriderAccessUpdated);
    }

    private void OnAccessOverriderAccessUpdated(Entity<EmaggedComponent> entity, ref OnAccessOverriderAccessUpdatedEvent args)
    {
        if (entity.Comp.EmagType.Id != "Access") // goob edit start
            return;

        if (!EmagType.TryGetValue("Access", out var emag))
            return; 

        entity.Comp.EmagType = emag; // goob edit end
        Dirty(entity);
    }
    private void OnAfterInteract(EntityUid uid, EmagComponent comp, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        args.Handled = TryEmagEffect((uid, comp), args.User, target);
    }

    /// <summary>
    /// Does the emag effect on a specified entity with a specified EmagType. The optional field customEmagType can be used to override the emag type defined in the component.
    /// </summary>
    public bool TryEmagEffect(Entity<EmagComponent?> ent, EntityUid user, EntityUid target, ProtoId<EmagTypePrototype>? customEmagType = null) // goob edit
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (_tag.HasTag(target, ent.Comp.EmagImmuneTag))
            return false;

        Entity<LimitedChargesComponent?> chargesEnt = ent.Owner;
        if (_sharedCharges.IsEmpty(chargesEnt))
        {
            _popup.PopupClient(Loc.GetString("emag-no-charges"), user, user);
            return false;
        }

        // Shitmed - Starlight Abductors: Check if the target has a whitelist, and check if it passes
        if (_whitelist.IsWhitelistFail(ent.Comp.ValidTargets, target))
        {
            _popup.PopupClient(Loc.GetString("emag-attempt-failed", ("tool", ent)), user, user);
            return false;
        }
        // Shitmed end

        // Use custom emag type if provided, otherwise use the component's type
        var emagTypeToUse = customEmagType ?? ent.Comp.EmagType; // goob edit

        var emaggedEvent = new GotEmaggedEvent(user, emagTypeToUse, EmagUid: ent); // goob edit
        RaiseLocalEvent(target, ref emaggedEvent);
        if (!emaggedEvent.Handled)
            return false;

        _popup.PopupPredicted(Loc.GetString(ent.Comp.SuccessText, ("target", Identity.Entity(target, EntityManager))), user, user, PopupType.Medium); // Goobstation - Success text de-hardcoded

        _audio.PlayPredicted(ent.Comp.EmagSound, ent, ent);
        _sparks.DoSparks(Transform(target).Coordinates); // goob edit - sparks everywhere

        _adminLogger.Add(LogType.Emag, LogImpact.High, $"{ToPrettyString(user):player} emagged {ToPrettyString(target):target} with flag(s): {emagTypeToUse}"); // goob edit - log emag type

        if (emaggedEvent.Handled)
            _sharedCharges.TryUseCharge(chargesEnt);

        if (!emaggedEvent.Repeatable)
        {
            EnsureComp<EmaggedComponent>(target, out var emaggedComp);

            emaggedComp.EmagType = emagTypeToUse;
            Dirty(target, emaggedComp);
        }

        return emaggedEvent.Handled;
    }
    /// Gigantic goob edit - EmagType checking functions
    /// <summary>
    /// Checks whether an entity has the EmaggedComponent with a set ID.
    /// </summary>
    /// <param name="target">The target entity to check for the ID.</param>
    /// <param name="protoId">The of the protoID to check for.</param>
    /// <returns>True if entity has EmaggedComponent and the provided ID. False if the entity lacks EmaggedComponent or provided ID.</returns>
    public bool CheckProtoId(EntityUid target, string protoId)
    {
        if (!TryComp<EmaggedComponent>(target, out var comp))
            return false;

        return CompareProtoId(comp.EmagType, protoId);
    }

    /// <summary>
    /// Compares a protoId to the target.
    /// </summary>
    /// <param name="target">The target protoId to check.</param>
    /// <param name="protoId">The string of protoId to check for within the target.</param>
    /// <returns>True if target contains protoId, Otherwise false.</returns>
    public bool CompareProtoId(ProtoId<EmagTypePrototype> target, string protoId)
    {
        if (!EmagType.ContainsKey(protoId))
            return false;

        if (target == _emagAll)
            return true;

        return target == protoId;
    }
    // Gigantic goob edit end
}

/// <summary>
/// Shows a popup to emag user (client side only!) and adds <see cref="EmaggedComponent"/> to the entity when handled
/// </summary>
/// <param name="UserUid">Emag user</param>
/// <param name="Type">The emag type to use</param>
/// <param name="Handled">Did the emagging succeed? Causes a user-only popup to show on client side</param>
/// <param name="Repeatable">Can the entity be emagged more than once? Prevents adding of <see cref="EmaggedComponent"/></param>
/// <param name="EmagUid">Uid of emag entity, Goobstation</param>
/// <remarks>Needs to be handled in shared/client, not just the server, to actually show the emagging popup</remarks>
[ByRefEvent]
public record struct GotEmaggedEvent(EntityUid UserUid, ProtoId<EmagTypePrototype> Type, bool Handled = false, bool Repeatable = false, EntityUid? EmagUid = null); // Goob edit
