// SPDX-FileCopyrightText: 2020 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Hugal31 <hugo.laloge@gmail.com>
// SPDX-FileCopyrightText: 2020 R. Neuser <rneuser@iastate.edu>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 GraniteSidewalk <32942106+GraniteSidewalk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Flash.Components;

/// <summary>
/// Allows this entity to flash someone by using it or melee attacking with it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedFlashSystem))]
public sealed partial class FlashComponent : Component
{
    /// <summary>
    /// Flash the area around the entity when used in hand?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FlashOnUse = true;

    /// <summary>
    /// Flash the target when melee attacking them?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FlashOnMelee = true;

    /// <summary>
    /// Time the Flash will be visually flashing after use.
    /// For the actual interaction delay use UseDelayComponent.
    /// These two times should be the same.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan FlashingTime = TimeSpan.FromSeconds(4);

    /// <summary>
    /// For how long the target will lose vision when melee attacked with the flash.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MeleeDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// For how long the target will lose vision when used in hand.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan AoeFlashDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How long a target is stunned when a melee flash is used.
    /// If null, melee flashes will not stun at all.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? MeleeStunDuration = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// Range of the flash when using it.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 7f;

    /// <summary>
    /// Movement speed multiplier for slowing down the target while they are flashed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SlowTo = 0.5f;

    /// <summary>
    /// The sound to play when flashing.
    /// </summary>

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/flash.ogg")
    {
        Params = AudioParams.Default.WithVolume(1f).WithMaxDistance(3f)
    };

    /// <summary>
    /// The probability of sucessfully flashing someone.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Probability = 1f;
}
