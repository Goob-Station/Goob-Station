using Content.Server.Paper;
using Content.Shared.Paper;
using Robust.Shared.Random;
using System.Text;
using Robust.Shared.Localization;
using System.Collections.Generic;
using System;
// mocho please we need content.shitcode asap ;-;
namespace Content.Goobstation.Server.NTR.Documents
{
    public sealed class SpamDocumentSystem : EntitySystem
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PaperSystem _paper = default!;

        private readonly List<(string department, int varCount)> _departments = new()
        {//maybe put this inside a comp or smth
            ("security", 4),
            ("cargo", 3),// cuz cargo has only 3 texts while every other department has 4
            ("medical", 4),
            ("engineering", 4),
            ("science", 4)
        };//oh and also, service sucks ass so i removed that, womp womp

        private readonly Dictionary<string, string> _departmentShortNames = new()
        {
            {"security", "sec"},
            {"cargo", "cargo"},
            {"medical", "med"},
            {"engineering", "engi"},
            {"science", "sci"}
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<SpamDocumentComponent, MapInitEvent>(OnDocumentInit);
        }

        private void OnDocumentInit(EntityUid uid, SpamDocumentComponent component, MapInitEvent args)
        {
            var text = GenerateSpamText(component.stype);
            if (TryComp<PaperComponent>(uid, out var paper))
            {
                _paper.SetContent((uid, paper), text);
            }
        }

        private string GenerateSpamText(SpamDocumentComponent.SpamType type)
        {
            return type switch
            {
                SpamDocumentComponent.SpamType.Obvious => GenerateObviousSpam(),
                SpamDocumentComponent.SpamType.Mimic => GenerateMimicSpam(),
                _ => GenerateObviousSpam()
            };
        }

        private string GenerateObviousSpam()
        {
            return _loc.GetString($"spam-obvious-text-{_random.Next(1, 11)}");
        }

        private string GenerateMimicSpam()
        {
            //random date, 2-60 days offset
            var dateOffset = _random.Next(2, 60);
            var curDate = DateTime.Now.AddYears(1000);
            var fakeDate = _random.Prob(0.5f)
                ? curDate.AddDays(dateOffset)   // positive days offset
                : curDate.AddDays(-dateOffset); // negative days offset
            var dateString = fakeDate.ToString("dd.MM.yyyy");
            // 10% chance of mimic document being just 100% legit but with an offset date to mess with NTRs
            if (_random.Prob(0.1f))
            {
                var (department, varCount) = _random.Pick(_departments);
                var args = GetDepartmentArgs(department, dateString);
                return _loc.GetString($"{department}-document-text", args.ToArray());
            }

            if (_random.Prob(0.2f)) // 20% chance
            {
                // bingle doc
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
            else
            {   //not bingle doc
                var (department, varCount) = _random.Pick(_departments);
                // generating starting text with an invalid date
                var startingArgs = new List<(string, object)> { ("date", dateString) };
                var startingText = _loc.GetString($"{department}-starting-text", startingArgs.ToArray());
                //spam part
                var legitText = GenerateLegitDepartmentText(department, varCount);
                var fullArgs = new List<(string, object)>
                {
                    ("start", startingText),
                    ("content", legitText),
                };

                return _loc.GetString("spam-mimic-template", fullArgs.ToArray());
            }
        }

        private string GenerateLegitDepartmentText(string department, int varCount)
        {
            var result = new StringBuilder();
            var shortName = _departmentShortNames[department];

            for (int i = 1; i <= varCount; i++)
            {
                var textKey = $"funny-{shortName}{i}-{_random.Next(1, 3)}";
                if (_loc.HasString(textKey))
                {
                    result.AppendLine(_loc.GetString(textKey));
                }
            }
            return result.ToString();
        }
        // department args
        // todo: integrate RandomDocumentSystem into this instead of just copy-pasting code...
        private List<(string, object)> GetDepartmentArgs(string department, string date)
        {
            return department switch
            {
                "security" => new List<(string, object)>
                {
                    ("start", _loc.GetString("security-starting-text", ("date", date))),
                    ("text1", _loc.GetString($"funny-sec1-{_random.Next(1, 16)}")),
                    ("text2", _loc.GetString($"funny-sec2-{_random.Next(1, 5)}")),
                    ("text3", _loc.GetString($"funny-sec3-{_random.Next(1, 3)}")),
                    ("text4", _loc.GetString($"funny-sec4-{_random.Next(1, 9)}"))
                },
                "cargo" => new List<(string, object)>
                {
                    ("start", _loc.GetString("cargo-starting-text", ("date", date))),
                    ("text1", _loc.GetString($"funny-cargo1-{_random.Next(1, 6)}")),
                    ("text2", _loc.GetString($"funny-cargo2-{_random.Next(1, 9)}")),
                    ("text3", _loc.GetString($"funny-cargo3-{_random.Next(1, 10)}")),
                },
                "medical" => new List<(string, object)>
                {
                    ("start", _loc.GetString("medical-starting-text", ("date", date))),
                    ("text1", _loc.GetString($"funny-med1-{_random.Next(1, 3)}")),
                    ("text2", _loc.GetString($"funny-med2-{_random.Next(1, 9)}")),
                    ("text3", _loc.GetString($"funny-med3-{_random.Next(1, 7)}")),
                    ("text4", _loc.GetString($"funny-med4-{_random.Next(1, 11)}")),
                },
                "engineering" => new List<(string, object)>
                {
                    ("start", _loc.GetString("engineering-starting-text", ("date", date))),
                    ("text1", _loc.GetString($"funny-engi1-{_random.Next(1, 6)}")),
                    ("text2", _loc.GetString($"funny-engi2-{_random.Next(1, 11)}")),
                    ("text3", _loc.GetString($"funny-engi3-{_random.Next(1, 10)}")),
                    ("text4", _loc.GetString($"funny-engi4-{_random.Next(1, 11)}")),
                },
                _ => new List<(string, object)> // sci by default cuz service sucks ass
                {
                    ("start", _loc.GetString("science-starting-text", ("date", date))),
                    ("text1", _loc.GetString($"funny-sci1-{_random.Next(1, 8)}")),
                    ("text2", _loc.GetString($"funny-sci2-{_random.Next(1, 10)}")),
                    ("text3", _loc.GetString($"funny-sci3-{_random.Next(1, 16)}")),
                    ("text4", _loc.GetString($"funny-sci4-{_random.Next(1, 8)}")),
                }
            };
        }
    }
}
