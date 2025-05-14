using Robust.Shared.Audio;

namespace Content.Goobstation.Server.MantisBlades;

public interface IMantisBladeUserComponent
{
    string BladeProto { get; set; }

    EntityUid? BladeUid { get; set; }

    SoundSpecifier? ExtendSound { get; set; }

    SoundSpecifier? RetractSound { get; set; }

    bool DisabledByEmp { get; set; }
}
