using Content.Server._EinsteinEngines.Language.Components;
using Content.Shared._EinsteinEngines.Language.Components;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._EinsteinEngines.Language.Systems;

public sealed class RandomizeLanguageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomLanguageComponent, MapInitEvent>(OnLanguageStartup);
        SubscribeLocalEvent<RandomLanguageComponent, ComponentShutdown>(OnLanguageShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<RandomLanguageComponent, LanguageSpeakerComponent>();
        while (query.MoveNext(out var uid, out var random, out var speaker))
        {
            if (!random.Enabled)
                continue;

            if (curTime < random.Until)
                continue;

            if (!_language.RandomizeEntityLanguage(uid))
                continue;

            random.Until = random.Interval + curTime;
        }
    }

    public void OnLanguageStartup(Entity<RandomLanguageComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<LanguageSpeakerComponent>(ent.Owner, out var speaker))
            return;

        //Don't remove the ToList() because it need the shallow copy
        ent.Comp.OriginalUnderstood = speaker.UnderstoodLanguages.ToList();
        ent.Comp.OriginalSpoken = speaker.SpokenLanguages.ToList();
    }

    public void OnLanguageShutdown(Entity<RandomLanguageComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<LanguageSpeakerComponent>(ent.Owner, out var speaker))
            return;

        //Don't remove the ToList() because it need the shallow copy
        speaker.SpokenLanguages = ent.Comp.OriginalSpoken.ToList();
        speaker.UnderstoodLanguages = ent.Comp.OriginalUnderstood.ToList();

        _language.UpdateEntityLanguages(ent.Owner);
    }
}
