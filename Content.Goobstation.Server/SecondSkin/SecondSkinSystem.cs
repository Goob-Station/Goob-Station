using Content.Goobstation.Common.SecondSkin;
using Content.Goobstation.Shared.SecondSkin;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Server.SecondSkin;

public sealed class SecondSkinSystem : SharedSecondSkinSystem
{
    [Dependency] private readonly DamageableSystem _dmg = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var siliconQuery = GetEntityQuery<SiliconComponent>();
        var userQuery = GetEntityQuery<SecondSkinUserComponent>();
        var godmodeQuery = GetEntityQuery<GodmodeComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();

        var query = EntityQueryEnumerator<SecondSkinComponent>();
        while (query.MoveNext(out var uid, out var skin))
        {
            if (skin.User == null)
                continue;

            skin.Accumulator += frameTime;

            if (skin.Accumulator < skin.UpdateTime)
                continue;

            skin.Accumulator -= skin.UpdateTime;

            if (!userQuery.HasComp(skin.User.Value) || !mobStateQuery.TryComp(skin.User.Value, out var state) ||
                state.CurrentState != MobState.Alive)
            {
                DisableSecondSkin((uid, skin), skin.User.Value, false);
                continue;
            }

            if (godmodeQuery.HasComp(skin.User.Value))
                continue;

            if (!skin.DamageToSilicons.Empty && siliconQuery.HasComp(skin.User.Value))
            {
                _dmg.TryChangeDamage(skin.User.Value,
                    skin.DamageToSilicons,
                    true,
                    false,
                    targetPart: skin.Parts,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll,
                    canMiss: false);
            }
        }
    }
}
