// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Goobstation.Shared.NTR.Documents;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.NTR.Documents
{
    public sealed class SpamDocumentSystem : EntitySystem
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PaperSystem _paper = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SpamDocumentComponent, MapInitEvent>(OnDocumentInit);
        }

        private void OnDocumentInit(EntityUid uid, SpamDocumentComponent component, MapInitEvent args)
        {
            var text = GenerateSpamText(component.SpamType);
            if (TryComp<PaperComponent>(uid, out var paper))
            {
                _paper.SetContent((uid, paper), text);
            }
        }
        private string GenerateSpamText(ProtoId<SpamTypePrototype> spamType)
        {
            if (!_proto.TryIndex(spamType, out var spamProto))
                return GenerateObviousSpam();

            return spamProto.ID switch
            {
                "ObviousSpam" => GenerateObviousSpam(),
                "MimicSpam" => GenerateMimicSpam(spamProto),
                _ => GenerateObviousSpam()
            };
        }

        private string GenerateObviousSpam()
        {
            return _loc.GetString($"spam-obvious-text-{_random.Next(1, 11)}");
        }

        private string GenerateMimicSpam(SpamTypePrototype spamProto)
        {
            // Random date offset (2-60 days)
            var dateOffset = _random.Next(2, 60);
            var curDate = DateTime.Now.AddYears(1000);
            var fakeDate = _random.Prob(0.5f) ? curDate.AddDays(dateOffset) : curDate.AddDays(-dateOffset);
            var dateString = fakeDate.ToString("dd.MM.yyyy");

            // 10% chance of perfect mimic
            if (_random.Prob(0.1f))
            {
                var docProto = _proto.Index<DocumentTypePrototype>(_random.Pick(spamProto.LegitDocuments));
                return GenerateDocumentContent(docProto, dateString);
            }

            // 20% chance of bingle doc
            if (_random.Prob(0.2f))
            {
                var randomNum = _random.Next(1000, 9999);
                var args = new List<(string, object)>
                {
                    ("scamPart", _loc.GetString($"spam-mimic-part-{_random.Next(1, 5)}")),
                    ("scamPart2", _loc.GetString($"spam-mimic-part2-{_random.Next(1, 4)}")),
                    ("randomNum", randomNum),
                    ("date", dateString)
                };
                return _loc.GetString("spam-mimic-bingle-text", args.ToArray());
            }

            // Regular mimic
            var targetDoc = _proto.Index<DocumentTypePrototype>(_random.Pick(spamProto.MimicDocuments));
            var content = GenerateDocumentContent(targetDoc, dateString);
            return _loc.GetString("spam-mimic-template", ("content", content));
        }

        private string GenerateDocumentContent(DocumentTypePrototype docProto, string dateString)
        {
            var args = new List<(string, object)>
            {
                ("start", _loc.GetString(docProto.StartingText, ("date", dateString)))
            };

            for (var i = 0; i < docProto.TextKeys.Length; i++)
            {
                var key = docProto.TextKeys[i];
                var count = docProto.TextCounts[i];
                var value = _loc.GetString($"{key}-{_random.Next(1, count + 1)}");
                args.Add(($"text{i + 1}", value));
            }

            return _loc.GetString(docProto.Template, args.ToArray());
        }
    }
}
