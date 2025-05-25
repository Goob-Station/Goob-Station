// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Timothy Teakettle <59849408+timothyteakettle@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Shared.Construction.Steps
{
    [DataDefinition]
    public sealed partial class TemperatureConstructionGraphStep : ConstructionGraphStep
    {
        [DataField("minTemperature")]
        public float? MinTemperature;
        [DataField("maxTemperature")]
        public float? MaxTemperature;

        public override void DoExamine(ExaminedEvent examinedEvent)
        {
            float guideTemperature = MinTemperature.HasValue ? MinTemperature.Value : (MaxTemperature.HasValue ? MaxTemperature.Value : 0);
            examinedEvent.PushMarkup(Loc.GetString("construction-temperature-default", ("temperature", guideTemperature)));
        }

        public override ConstructionGuideEntry GenerateGuideEntry()
        {
            float guideTemperature = MinTemperature.HasValue ? MinTemperature.Value : (MaxTemperature.HasValue ? MaxTemperature.Value : 0);

            return new ConstructionGuideEntry()
            {
                Localization = "construction-presenter-temperature-step",
                Arguments = new (string, object)[] { ("temperature", guideTemperature) }
            };
        }
    }
}