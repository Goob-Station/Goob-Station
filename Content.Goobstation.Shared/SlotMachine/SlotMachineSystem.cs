using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;


namespace Content.Goobstation.Shared.SlotMachine
{
    public sealed class SlotMachineSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

        public override void Initialize()
        {
            base.Initialize();


            SubscribeLocalEvent<SlotMachineComponent, InteractHandEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, SlotMachineComponent component, InteractHandEvent args)
        {
            _audio.PlayPredicted(component.SpinSound, uid, args.User);
        }
    }
}