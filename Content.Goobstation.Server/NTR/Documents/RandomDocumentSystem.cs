using Content.Server.Paper;
using Content.Shared.Paper;    //ammount of times this whole system was re-done: 3
using Content.Shared.StoryGen; //ammount of hours wasted trying to understand papersystem: 29
using Robust.Shared.Random;    //skill issue.
using System.Linq;
using Content.Goobstation.Shared.NTR;
using Robust.Shared.GameStates;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
// todo: clean these usings
namespace Content.Goobstation.Server.NTR.Documents
{
    public sealed class RandomDocumentSystem : EntitySystem
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PaperSystem _paper = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<RandomDocumentComponent, MapInitEvent>(OnDocumentInit);
        }

        private void OnDocumentInit(EntityUid uid, RandomDocumentComponent component, MapInitEvent args)
        {
            var text = GenerateDocument(component.dtype);
            if (TryComp<PaperComponent>(uid, out var paperComp))
            {
                _paper.SetContent((uid, paperComp), text);
            }
        }

        private string GenerateDocument(RandomDocumentComponent.DocumentType type)
        {
            var template = type switch
            {
                RandomDocumentComponent.DocumentType.Security => "security-document-text",
                RandomDocumentComponent.DocumentType.Cargo => "cargo-document-text",
                RandomDocumentComponent.DocumentType.Medical => "medical-document-text",
                RandomDocumentComponent.DocumentType.Engineering => "engineering-document-text",
                RandomDocumentComponent.DocumentType.Science => "science-document-text",
                _ => "service-document-text" // if not specified, use service
            };

            var args = type switch
            {
                RandomDocumentComponent.DocumentType.Security => GetSecurityArgs(),
                RandomDocumentComponent.DocumentType.Cargo => GetCargoArgs(),
                RandomDocumentComponent.DocumentType.Medical => GetMedicalArgs(),
                RandomDocumentComponent.DocumentType.Engineering => GetEngineeringArgs(),
                RandomDocumentComponent.DocumentType.Science => GetScienceArgs(),
                _ => GetServiceArgs()
            };

            var result = _loc.GetString(template, args);
            return result;
        }

        private (string, object)[] GetBaseArgs(string prefix)
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString($"{prefix}-starting-text"))
            };
        }

        private (string, object)[] GetServiceArgs()
        {
            var args = new List<(string, object)>
            {
                ("start", _loc.GetString("service-starting-text")),
                ("text1", _loc.GetString($"funny-service1-{_random.Next(1, 16)}")),
                ("text2", _loc.GetString($"funny-service2-{_random.Next(1, 16)}")),
                ("text3", _loc.GetString($"funny-service3-{_random.Next(1, 16)}")),
                ("text4", _loc.GetString($"funny-service4-{_random.Next(1, 16)}"))
            };
            return args.ToArray();
        }

        private (string, object)[] GetSecurityArgs()
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString("security-starting-text")),
                ("text1", _loc.GetString($"funny-sec1-{_random.Next(1, 16)}")),
                ("text2", _loc.GetString($"funny-sec2-{_random.Next(1, 5)}")),
                ("text3", _loc.GetString($"funny-sec3-{_random.Next(1, 3)}")),
                ("text4", _loc.GetString($"funny-sec4-{_random.Next(1, 9)}"))
            };
        }

        private (string, object)[] GetCargoArgs()
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString("cargo-starting-text")),
                ("text1", _loc.GetString($"funny-cargo1-{_random.Next(1, 6)}")),
                ("text2", _loc.GetString($"funny-cargo2-{_random.Next(1, 9)}")),
                ("text3", _loc.GetString($"funny-cargo3-{_random.Next(1, 10)}")),
            };
        }

        private (string, object)[] GetMedicalArgs()
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString("medical-starting-text")),
                ("text1", _loc.GetString($"funny-med1-{_random.Next(1, 3)}")),
                ("text2", _loc.GetString($"funny-med2-{_random.Next(1, 9)}")),
                ("text3", _loc.GetString($"funny-med3-{_random.Next(1, 7)}")),
                ("text4", _loc.GetString($"funny-med4-{_random.Next(1, 11)}")),
            };
        }

        private (string, object)[] GetEngineeringArgs()
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString("engineering-starting-text")),
                ("text1", _loc.GetString($"funny-engi1-{_random.Next(1, 6)}")),
                ("text2", _loc.GetString($"funny-engi2-{_random.Next(1, 11)}")),
                ("text3", _loc.GetString($"funny-engi3-{_random.Next(1, 10)}")),
                ("text4", _loc.GetString($"funny-engi4-{_random.Next(1, 11)}")),
            };
        }
        private (string, object)[] GetScienceArgs()
        {
            return new (string, object)[]
            {
                ("start", _loc.GetString("science-starting-text")),
                ("text1", _loc.GetString($"funny-sci1-{_random.Next(1, 8)}")),
                ("text2", _loc.GetString($"funny-sci2-{_random.Next(1, 10)}")),
                ("text3", _loc.GetString($"funny-sci3-{_random.Next(1, 16)}")),
                ("text4", _loc.GetString($"funny-sci4-{_random.Next(1, 8)}")),
            };
        }
    }
}
