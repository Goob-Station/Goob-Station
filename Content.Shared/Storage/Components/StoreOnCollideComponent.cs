// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Storage.Components;

// Use where you want an entity to store other entities on collide
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(StoreOnCollideSystem))]
public sealed partial class StoreOnCollideComponent : Component
{
    /// <summary>
    ///     Entities that are allowed in the storage on collide
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    ///     Should this storage lock on collide, provided they have a lock component?
    /// </summary>
    [DataField]
    public bool LockOnCollide;

    /// <summary>
    ///     Should the behavior be disabled when the storage is first opened?
    /// </summary>
    [DataField]
    public bool DisableWhenFirstOpened;

    /// <summary>
    ///     If the behavior is disabled or not
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Disabled;

    /// <summary>
    ///     Goobstation
    ///     Don't store this entity, it is used for shooter if this is projectile
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? IgnoredEntity;

    /// <summary>
    ///     Goobstation
    ///     Should the behavior be disabled when entity (physics) sleeps?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DisableOnSleep;
}