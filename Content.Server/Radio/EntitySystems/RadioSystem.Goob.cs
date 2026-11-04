using Content.Goobstation.Common.Radio;
using Content.Shared._EinsteinEngines.Language;

namespace Content.Server.Radio.EntitySystems;

public sealed partial class RadioSystem
{
    private bool LanguageRadioOverride(EntityUid speaker, ref LanguagePrototype language)
    {
        var ev = new LanguageRadioAttemptEvent(language.ID);
        RaiseLocalEvent(speaker, ref ev);
        if (!ev.Allowed)
            return false;

        if (ev.TranslateTo != null && _prototype.TryIndex<LanguagePrototype>(ev.TranslateTo, out var translated))
            language = translated;

        return true;
    }
}
