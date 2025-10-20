using Content.Shared._DV.AACTablet;
using Content.Shared._EinsteinEngines.Language.Components;

namespace Content.Client._DV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<AACTabletLanguagesRefreshedEvent>(OnLanguagesRefreshed);
    }
    private void OnLanguagesRefreshed(AACTabletLanguagesRefreshedEvent ev, EntitySessionEventArgs args)
    {
        var tablet = GetEntity(ev.Tablet);
        if (!TryComp<LanguageSpeakerComponent>(tablet, out var speaker))
            return;

        speaker.SpokenLanguages = ev.SpokenLanguages;
    }
}
