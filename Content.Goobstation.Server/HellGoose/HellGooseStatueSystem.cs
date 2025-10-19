using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.HellGoose.Systems;
using Content.Goobstation.Shared.Emoting;
using Content.Server.Pointing.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.HellGoose;


public sealed class HellGooseStatueSystem : HellGooseStatueSharedSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    protected override void GibTheGoose(EntityUid uid, EntityUid statue)
    {
        base.GibTheGoose(uid, statue);
        _bodySystem.GibBody(uid, splatModifier: 40);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Misc/odetojoy.ogg"), statue);
    }

}