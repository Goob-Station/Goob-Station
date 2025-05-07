// SPDX-FileCopyrightText: 2020 BlueberryShortcake <rubetskoy234@mail.ru>
// SPDX-FileCopyrightText: 2020 DmitriyRubetskoy <75271456+DmitriyRubetskoy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 nuke <47336974+nuke-makes-games@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 StStevens <StStevens@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Fluids.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory; // Assmos - Extinguisher Nozzle
using Content.Shared.Whitelist; // Assmos - Extinguisher Nozzle

namespace Content.Server.Fluids.Components;

[RegisterComponent]
[Access(typeof(SpraySystem))]
public sealed partial class SprayComponent : Component
{
    public const string SolutionName = "spray";
    public const string TankSolutionName = "tank"; // Assmos - Extinguisher Nozzle

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public FixedPoint2 TransferAmount = 10;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SprayDistance = 3.5f;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SprayVelocity = 3.5f;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntProtoId SprayedPrototype = "Vapor";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int VaporAmount = 1;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float VaporSpread = 90f;

    /// <summary>
    /// How much the player is pushed back for each spray.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float PushbackAmount = 2f;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    [Access(typeof(SpraySystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
    public SoundSpecifier SpraySound { get; private set; } = default!;

    /// <remarks>
    /// Assmos - Extinguisher Nozzle
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public SlotFlags TargetSlot;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntityWhitelist? ProviderWhitelist;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ExternalContainer = false;
}
