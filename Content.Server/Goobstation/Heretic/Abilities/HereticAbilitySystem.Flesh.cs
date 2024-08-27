using Content.Shared.Heretic;
using Robust.Shared.Audio;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeFlesh()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticFleshAscend>(OnFleshAscendPolymorph);
    }

    private void OnFleshAscendPolymorph(Entity<HereticComponent> ent, ref EventHereticFleshAscend args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var urist = _poly.PolymorphEntity(ent, "EldritchHorror");
        if (urist == null)
            return;

        _aud.PlayPvs(new SoundPathSpecifier("/Audio/Animals/space_dragon_roar.ogg"), (EntityUid) urist, AudioParams.Default.AddVolume(2f));

        args.Handled = true;
    }
}
