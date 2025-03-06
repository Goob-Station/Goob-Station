using Content.Shared.Popups;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using System.Linq;
using Content.Shared.Damage;
using Robust.Shared.Audio.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Heretic;
using Robust.Shared.Utility;
using Content.Shared.Damage.Components;

namespace Content.Shared._Goobstation.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<HereticRitualRuneComponent, StepTriggeredOffEvent>(OnStepOffTriggered);
        SubscribeLocalEvent<HereticRitualRuneComponent, StepTriggeredOnEvent>(OnStepTriggered);
    }

    private void OnCompInit(Entity<WeakToHolyComponent> ent, ref ComponentInit args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == "Biological") {
            _damageableSystem.ChangeDamageContainer(ent, "BiologicalMetaphysical");
        }

    }


    // passive healing on runes for aviu - commented out until we refactor to shared
    private void OnStepTriggered(EntityUid uid, HereticRitualRuneComponent component, ref StepTriggeredOnEvent args)
    {

        if (!HasComp<WeakToHolyComponent>(args.Tripper))
            return;

        var _heretic = EnsureComp<PassiveDamageComponent>(args.Tripper);
        _heretic.AllowedStates = new List<MobState>() { MobState.Alive };
        _heretic.Damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Holy"), -10);
        DirtyEntity(uid);

    }

    private void OnStepOffTriggered(EntityUid uid, HereticRitualRuneComponent component, ref StepTriggeredOffEvent args)
    {
        RemComp<PassiveDamageComponent>(args.Tripper);
    }
}
