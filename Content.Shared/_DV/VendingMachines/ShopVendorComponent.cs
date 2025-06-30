// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.VendingMachines;

/// <summary>
/// A vending machine that sells items for a currency controlled by events.
/// Does not need restocking.
/// Another component must handle <see cref="ShopVendorBalanceEvent"/> and <see cref="ShopVendorPurchaseEvent"/> to work.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedShopVendorSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ShopVendorComponent : Component
{
    /// <summary>
    /// The inventory prototype to sell.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ShopInventoryPrototype> Pack;

    [DataField, AutoNetworkedField]
    public bool Broken;

    [DataField, AutoNetworkedField]
    public bool Denying;

    /// <summary>
    /// Item being ejected, or null if it isn't.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? Ejecting;

    /// <summary>
    /// How long to wait before flashing denied again.
    /// </summary>
    [DataField]
    public TimeSpan DenyDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long to wait before another item can be bought
    /// </summary>
    [DataField]
    public TimeSpan EjectDelay = TimeSpan.FromSeconds(1.2);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextDeny;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextEject;

    [DataField]
    public SoundSpecifier PurchaseSound = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg")
    {
        Params = new AudioParams
        {
            Volume = -4f,
            Variation = 0.15f
        }
    };

    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg")
    {
        Params = new AudioParams
        {
            Volume = -2f
        }
    };

    #region Visuals

    [DataField]
    public bool LoopDenyAnimation = true;

    [DataField]
    public string? OffState;

    [DataField]
    public string? ScreenState;

    [DataField]
    public string? NormalState;

    [DataField]
    public string? DenyState;

    [DataField]
    public string? EjectState;

    [DataField]
    public string? BrokenState;

    #endregion
}