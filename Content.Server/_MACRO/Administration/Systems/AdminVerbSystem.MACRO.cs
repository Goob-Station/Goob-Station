using Content.Server._MACRO.StrangeMoods;
using Content.Server._MACRO.StrangeMoods.Eui;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private StrangeMoodsSystem _moods = default!; // MACRO

    private void AddMACROVerbs(GetVerbsEvent<Verb> args)
    {
        if (TryComp<StrangeMoodsComponent>(args.Target, out var moods))
        {
            args.Verbs.Add(new Verb()
            {
                Text = Loc.GetString("strange-moods-ui-verb"),
                Category = VerbCategory.Admin,
                Act = () =>
                {
                    var ui = new StrangeMoodsEui(_moods, EntityManager, _random, _adminManager);
                    if (!_playerManager.TryGetSessionByEntity(args.User, out var session))
                        return;

                    _euiManager.OpenEui(ui, session);
                    ui.UpdateMoods((args.Target, moods));
                },
                Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Actions/actions_borg.rsi"), "state-laws"),
            });
        }
    }
    private void AddMACROTricks(GetVerbsEvent<Verb> args)
    {
        if (TryComp<StrangeMoodsComponent>(args.Target, out var moods))
        {
            if (moods.StrangeMood.Datasets.Count <= 0)
                return;

            Verb addRandomMood = new()
            {
                Text = "Add Random Mood",
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Rsi(new ResPath("Interface/Actions/actions_borg.rsi"), "state-laws"),
                Act = () =>
                {
                    _moods.TryAddRandomMood((args.Target, moods), _random.Pick(moods.StrangeMood.Datasets).Key);
                },
                Impact = LogImpact.High,
                Message = Loc.GetString("admin-trick-add-random-mood-description"),
                Priority = (int)TricksVerbPriorities.AddRandomMood,
            };
            args.Verbs.Add(addRandomMood);
        }
        else
        {
            Verb giveMoods = new()
            {
                Text = "Give Moods",
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Rsi(new ResPath("Interface/Actions/actions_borg.rsi"), "state-laws"),
                Act = () =>
                {
                    if (HasComp<StrangeMoodsComponent>(args.Target))
                        return;

                    var ui = new StrangeMoodsInitEui(_moods, EntityManager, _prototypeManager, _random, _adminManager, _playerManager, _euiManager, args.User);
                    if (!_playerManager.TryGetSessionByEntity(args.User, out var session))
                        return;

                    _euiManager.OpenEui(ui, session);
                    ui.SetTarget(args.Target);
                },
                Impact = LogImpact.High,
                Message = Loc.GetString("admin-trick-give-moods-description"),
                Priority = (int)TricksVerbPriorities.AddRandomMood,
            };
            args.Verbs.Add(giveMoods);
        }
    }
}