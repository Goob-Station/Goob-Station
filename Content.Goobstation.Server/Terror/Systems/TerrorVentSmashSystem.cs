using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Terror.Systems
{
    public sealed class TerrorVentSmashSystem : EntitySystem
    {
        [Dependency] private readonly WeldableSystem _weldable = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TerrorVentSmashComponent, TerrorVentSmashEvent>(OnSmash);
        }

        private void OnSmash(EntityUid uid, TerrorVentSmashComponent component, TerrorVentSmashEvent args)
        {
            var target = args.Target;

            if (_weldable.IsWelded(target))
            {
                _weldable.SetWeldedState(target, false);
                _audio.PlayPvs(component.SmashSound, uid);
            }
        }

    }
}
