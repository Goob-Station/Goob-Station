// SPDX-FileCopyrightText: 2020 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2020 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 FL-OZ <58238103+FL-OZ@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Matz05 <Matz05@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Memory <58238103+FL-OZ@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 PrPleGoo <felix.leeuwen@gmail.com>
// SPDX-FileCopyrightText: 2020 RemberBL <timmermanrembrandt@gmail.com>
// SPDX-FileCopyrightText: 2020 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Light.EntitySystems;
using Content.Shared.DeviceLinking;
using Content.Shared.Light.Components;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Light.Components
{
    /// <summary>
    ///     Component that represents a wall light. It has a light bulb that can be replaced when broken.
    /// </summary>
    [RegisterComponent, Access(typeof(PoweredLightSystem))]
    public sealed partial class PoweredLightComponent : Component
    {
        [DataField("burnHandSound")]
        public SoundSpecifier BurnHandSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

        [DataField("turnOnSound")]
        public SoundSpecifier TurnOnSound = new SoundPathSpecifier("/Audio/Machines/light_tube_on.ogg");

        [DataField("hasLampOnSpawn", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? HasLampOnSpawn = null;

        [DataField("bulb")]
        public LightBulbType BulbType;

        [DataField("on")]
        public bool On = true;

        [DataField("ignoreGhostsBoo")]
        public bool IgnoreGhostsBoo;

        [DataField("ghostBlinkingTime")]
        public TimeSpan GhostBlinkingTime = TimeSpan.FromSeconds(10);

        [DataField("ghostBlinkingCooldown")]
        public TimeSpan GhostBlinkingCooldown = TimeSpan.FromSeconds(60);

        [ViewVariables]
        public ContainerSlot LightBulbContainer = default!;
        [ViewVariables]
        public bool CurrentLit;
        [ViewVariables]
        public bool IsBlinking;
        [ViewVariables]
        public TimeSpan LastThunk;
        [ViewVariables]
        public TimeSpan? LastGhostBlink;

        [DataField("onPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
        public string OnPort = "On";

        [DataField("offPort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
        public string OffPort = "Off";

        [DataField("togglePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
        public string TogglePort = "Toggle";

        /// <summary>
        /// How long it takes to eject a bulb from this
        /// </summary>
        [DataField("ejectBulbDelay")]
        public float EjectBulbDelay = 2;

        /// <summary>
        /// Shock damage done to a mob that hits the light with an unarmed attack
        /// </summary>
        [DataField("unarmedHitShock")]
        public int UnarmedHitShock = 20;

        /// <summary>
        /// Stun duration applied to a mob that hits the light with an unarmed attack
        /// </summary>
        [DataField("unarmedHitStun")]
        public TimeSpan UnarmedHitStun = TimeSpan.FromSeconds(5);
    }
}