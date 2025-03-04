using System.Runtime.InteropServices;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentInit>(OnCompInit);
    }

    private void OnCompInit(Entity<WeakToHolyComponent> ent, ref ComponentInit args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == "Biological") {
        _damageableSystem.ChangeDamageContainer(ent, "BiologicalMetaphysical");
        }
    }
}
