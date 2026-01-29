using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/centrifuge.dm

public sealed class NuclearCentrifugeSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    private readonly float _threshold = 1f;
    private float _accumulator = 0f;
    private readonly int _stackSize = 30;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearCentrifugeComponent, InteractUsingEvent>(OnInteract);
        SubscribeLocalEvent<NuclearCentrifugeComponent, PowerChangedEvent>(OnPowerChange);
    }

    public override void Update(float frameTime)
    {
        _accumulator += frameTime;
        if (_accumulator > _threshold)
        {
            AccUpdate();
            _accumulator = 0;
        }
    }

    public void AccUpdate()
    {
        var query = EntityQueryEnumerator<NuclearCentrifugeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if(!comp.Processing)
                continue;
            
            if(comp.FuelToExtract>0)
            {
                var delta = Math.Min(comp.FuelToExtract, 0.5f);
                comp.ExtractedFuel += delta;
                comp.FuelToExtract -= delta;
            }
            else
            {
                if(comp.ExtractedFuel > 1)
                {
                    // If this while loop causes problems, blame whoever put 1.78e308 plutonium in the centrifuge
                    while (comp.ExtractedFuel > 1) 
                    {
                        var plutoniumStack = Spawn("IngotPlutonium1", Transform(uid).Coordinates);
                        _stackSystem.SetCount(plutoniumStack, Math.Clamp((int)Math.Floor(comp.ExtractedFuel), 1, _stackSize));
                        comp.ExtractedFuel -= _stackSystem.GetCount(plutoniumStack);
                        _stackSystem.TryMergeToContacts(plutoniumStack);
                    }
                    _audio.PlayPvs(comp.SoundSucceed, uid);
                }
                else
                {
                    _audio.PlayPvs(comp.SoundFail, uid, AudioParams.Default.WithVolume(-2));
                }

                _audio.Stop(comp.AudioProcess);

                comp.Processing = false;
                _appearance.SetData(uid, NuclearCentrifugeVisuals.Processing, false);
            }
        }
    }

    private void OnInteract(EntityUid uid, NuclearCentrifugeComponent comp, ref InteractUsingEvent args)
    {
        if (!this.IsPowered(uid, _entityManager))
            return;

        if (!_entityManager.TryGetComponent<ReactorPartComponent>(args.Used, out var ReactorPart) || !ReactorPart.HasRodType(ReactorPartComponent.RodTypes.FuelRod))
        {
            _popupSystem.PopupEntity(Loc.GetString("nuclear-centrifuge-wrong-item", ("item", args.Used)), args.User, args.User);
            return;
        }

        if (ReactorPart.Properties == null || ReactorPart.Properties.FissileIsotopes < 0.1)
        {
            _popupSystem.PopupEntity(Loc.GetString("nuclear-centrifuge-unfit-item", ("item", args.Used)), args.User, args.User);
            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("nuclear-centrifuge-insert-item", ("user", args.User), ("machine", uid), ("item", args.Used)), uid);
        _audio.PlayPvs(comp.SoundLoad, uid);

        if(!_audio.IsPlaying(comp.AudioProcess))
            comp.AudioProcess = _audio.PlayPvs(comp.SoundProcess, uid, AudioParams.Default.WithLoop(true).WithVolume(-2))?.Entity;

        comp.FuelToExtract += ReactorPart.Properties.FissileIsotopes;
        comp.Processing = true;
        _entityManager.DeleteEntity(args.Used);

        _appearance.SetData(uid, NuclearCentrifugeVisuals.Processing, true);

        args.Handled = true;
    }

    private void OnPowerChange(EntityUid uid, NuclearCentrifugeComponent comp, ref PowerChangedEvent args)
    {
        if(!args.Powered && comp.Processing)
        {
            if(_audio.IsPlaying(comp.AudioProcess))
                _audio.Stop(comp.AudioProcess);
            comp.Processing = false;
        }

        if(args.Powered && comp.FuelToExtract > 0)
        {
            comp.AudioProcess = _audio.PlayPvs(comp.SoundProcess, uid, AudioParams.Default.WithLoop(true).WithVolume(-2))?.Entity;
            comp.Processing = true;
        }

        _appearance.SetData(uid, NuclearCentrifugeVisuals.Processing, comp.Processing);
    }
}