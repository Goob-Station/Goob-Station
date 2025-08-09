// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.NTR.Documents; //amount of times this whole system was re-done: 3
using Content.Shared.Paper;
using Robust.Shared.Prototypes; //amount of hours wasted trying to understand papersystem: 29
using Robust.Shared.Random;                     //skill issue.

// todo: clean these usings
namespace Content.Goobstation.Server.NTR.Documents
{
    public sealed class RandomDocumentSystem : EntitySystem
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PaperSystem _paper = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<RandomDocumentComponent, MapInitEvent>(OnDocumentInit);
        }

        private void OnDocumentInit(EntityUid uid, RandomDocumentComponent component, MapInitEvent args)
        {
            var text = GenerateDocument(component.DocumentType);
            if (TryComp<PaperComponent>(uid, out var paperComp))
                _paper.SetContent((uid, paperComp), text);
        }

        private string GenerateDocument(ProtoId<DocumentTypePrototype> docType)
        {
            if (string.IsNullOrEmpty(docType.Id) // i hate this
                || !_proto.TryIndex(docType, out var docProto))
                return string.Empty;

            var curDate = DateTime.Now.AddYears(1000);
            var dateString = curDate.ToString("dd.MM.yyyy");

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
