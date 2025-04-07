// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Examine;
using Content.Shared.Tools;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Construction.Steps
{
    [DataDefinition]
    public sealed partial class ToolConstructionGraphStep : ConstructionGraphStep
    {
        [DataField("tool", required:true, customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
        public string Tool { get; private set; } = string.Empty;

        [DataField("fuel")] public float Fuel { get; private set; } = 10;

        [DataField("examine")] public string ExamineOverride { get; private set; } = string.Empty;

        public override void DoExamine(ExaminedEvent examinedEvent)
        {
            if (!string.IsNullOrEmpty(ExamineOverride))
            {
                examinedEvent.PushMarkup(Loc.GetString(ExamineOverride));
                return;
            }

            if (string.IsNullOrEmpty(Tool) || !IoCManager.Resolve<IPrototypeManager>().TryIndex(Tool, out ToolQualityPrototype? quality))
                return;

            examinedEvent.PushMarkup(Loc.GetString("construction-use-tool-entity", ("toolName", Loc.GetString(quality.ToolName))));

        }

        public override ConstructionGuideEntry GenerateGuideEntry()
        {
            var quality = IoCManager.Resolve<IPrototypeManager>().Index<ToolQualityPrototype>(Tool);

            return new ConstructionGuideEntry()
            {
                Localization = "construction-presenter-tool-step",
                Arguments = new (string, object)[]{("tool", quality.ToolName)},
                Icon = quality.Icon,
            };
        }
    }
}