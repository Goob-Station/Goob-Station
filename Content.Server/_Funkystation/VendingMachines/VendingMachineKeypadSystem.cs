using Content.Shared._Funkystation.VendingMachines;
using Content.Shared.VendingMachines;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Funkystation.VendingMachines;

public sealed partial class VendingMachineKeypadSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VendingMachineComponent, VendingMachineKeypadAudioMessage>(OnKeypadAudio);
    }

    private void OnKeypadAudio(EntityUid uid, VendingMachineComponent component, VendingMachineKeypadAudioMessage args)
    {
        var soundPath = args.SoundType switch
        {
            VendingMachineKeypadSound.Beep => "/Audio/Machines/Nuke/general_beep.ogg",
            VendingMachineKeypadSound.Success => "/Audio/Machines/vending_jingle.ogg",
            VendingMachineKeypadSound.Error => "/Audio/Machines/buzz-two.ogg",
            VendingMachineKeypadSound.Timeout => "/Audio/Machines/button.ogg",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundPath))
            return;

        var volume = args.SoundType == VendingMachineKeypadSound.Timeout ? -6f : -4f;
        var audioParams = new AudioParams().WithVolume(volume).WithPitchScale(Math.Clamp(args.Pitch, 0.25f, 4f));

        _audio.PlayPredicted(new SoundPathSpecifier(soundPath), uid, args.Actor, audioParams);
    }
}
