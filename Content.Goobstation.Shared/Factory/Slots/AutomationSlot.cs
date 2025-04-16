// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory.Filters;
using Content.Shared.DeviceLinking;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// An abstraction over some way to insert/take an item from a machine.
/// For these methods user is the machine that is doing the automation not a player.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class AutomationSlot
{
    [Dependency] public readonly IEntityManager EntMan = default!;
    private EntityWhitelistSystem? __whitelist;
    // Dependency not working my beloved
    private EntityWhitelistSystem _whitelist
    {
        get
        {
            __whitelist ??= EntMan.System<EntityWhitelistSystem>();
            return __whitelist;
        }
    }

    /// <summary>
    /// The input port for this slot, or null if can only be used as an output.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SinkPortPrototype>? Input;

    /// <summary>
    /// The output port for this slot, or null if can only be used as an input.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SourcePortPrototype>? Output;

    /// <summary>
    /// Whitelist that can be used in YML regardless of slot type.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Blacklist that can be used in YML regardless of slot type.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Try to insert an item into the slot, returning true if it was removed from its previous container.
    /// Inheritors must override this and use <c>if (!base.Insert(uid, item)) return false;</c>
    /// </summary>
    public virtual bool Insert(EntityUid uid, EntityUid item)
    {
        return CanInsert(uid, item);
    }

    /// <summary>
    /// Check if an item can be inserted into the slot, returning true if it can.
    /// Inheritors must override this and use <c>if (!base.CanInsert(uid, item)) return false;</c>
    /// </summary>
    public virtual bool CanInsert(EntityUid uid, EntityUid item)
    {
        return _whitelist.CheckBoth(item, whitelist: Whitelist, blacklist: Blacklist);
    }

    /// <summary>
    /// Get an item that can be taken from this slot, which has to match a given filter.
    /// If there are multiple items, which one returned is arbitrary and should not be relied upon.
    /// This should be "pure" and not actually modify anything.
    /// </summary>
    public abstract EntityUid? GetItem(EntityUid uid, AutomationFilter? filter);
}
