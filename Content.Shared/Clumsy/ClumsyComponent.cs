// SPDX-FileCopyrightText: 2024 beck <163376292+widgetbeck@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Clumsy;

/// <summary>
/// Makes the entity clumsy, randomly failing some interactions and hurting themselves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClumsyComponent : Component
{

    // Standard options. Try to fit these in if you can!

    /// <summary>
    ///     Sound to play when clumsy interactions fail.
    /// </summary>
    [DataField]
    public SoundSpecifier ClumsySound = new SoundPathSpecifier("/Audio/Items/bikehorn.ogg");

    /// <summary>
    ///     Default chance to fail a clumsy interaction.
    ///     If a system needs to use something else, add a new variable in the component, do not modify this percentage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ClumsyDefaultCheck = 0.5f;

    /// <summary>
    ///     Default stun time.
    ///     If a system needs to use something else, add a new variable in the component, do not modify this number.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ClumsyDefaultStunTime = TimeSpan.FromSeconds(2.5);

    // Specific options

    /// <summary>
    ///     Sound to play after hitting your head on a table. Ouch!
    /// </summary>
    [DataField]
    public SoundCollectionSpecifier TableBonkSound = new SoundCollectionSpecifier("TrayHit");

    /// <summary>
    ///     Stun time after failing to shoot a gun.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan GunShootFailStunTime = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Damage taken after failing to shoot a gun.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier? GunShootFailDamage = new() {DamageDict = new Dictionary<string, FixedPoint2> {{"Brute", 12}}}; // Goob edit - add a default value (evil)

    /// <summary>
    ///     Damage taken after failing to catch an item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier? CatchingFailDamage;

    /// <summary>
    ///     Noise to play after failing to shoot a gun. Boom!
    /// </summary>
    [DataField]
    public SoundSpecifier GunShootFailSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/bang.ogg");

    /// <summary>
    ///      Whether or not to apply Clumsy to hyposprays.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyHypo = true;

    /// <summary>
    ///      Whether or not to apply Clumsy to defibs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyDefib = true;

    /// <summary>
    ///      Whether or not to apply Clumsy to guns.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyGuns = true;

    /// <summary>
    ///      Whether or not to apply Clumsy to catching items.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyCatching = true;

    /// <summary>
    ///      Whether or not to apply Clumsy to vaulting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyVaulting = true;

    /// <summary>
    ///      Lets you define a new "failed" message for each event.
    /// </summary>
    [DataField]
    public LocId HypoFailedMessage = "clumsy-hypospray-fail-message";

    [DataField]
    public LocId GunFailedMessage = "clumsy-gun-fail-message";

    [DataField]
    public LocId CatchingFailedMessageSelf = "clumsy-catch-fail-message-user";

    [DataField]
    public LocId CatchingFailedMessageOthers = "clumsy-catch-fail-message-others";

    [DataField]
    public LocId VaulingFailedMessageSelf = "clumsy-vaulting-fail-message-user";

    [DataField]
    public LocId VaulingFailedMessageOthers = "clumsy-vaulting-fail-message-others";

    [DataField]
    public LocId VaulingFailedMessageForced = "clumsy-vaulting-fail-forced-message";
}
