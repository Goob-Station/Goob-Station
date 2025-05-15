using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MantisBlades;

public interface IMantisBladeUserComponent
{
    string BladeProto { get; set; }

    EntityUid? BladeUid { get; set; }

    SoundSpecifier? ExtendSound { get; set; }

    SoundSpecifier? RetractSound { get; set; }
}
