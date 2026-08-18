// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared._Hood.Phone;

/// <summary>
/// Marks a reusable Hood smartphone and owns its removable SIM slot.
/// The phone itself never owns a telephone number; that identity belongs to the inserted SIM.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedPhoneSystem))]
public sealed partial class PhoneComponent : Component
{
    public const string SimSlotId = "hood-phone-sim";

    [DataField]
    public ItemSlot SimSlot = new();
}
