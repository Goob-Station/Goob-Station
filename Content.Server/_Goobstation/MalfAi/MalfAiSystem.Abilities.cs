using Content.Server.Explosion.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.Power.Components;
using Content.Server.Objectives.Components;
using Content.Server.Store.Systems;
using Content.Shared.Emag.Components;
using Content.Shared.MalfAi;
using Content.Shared.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Store.Components;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.MalfAi;

public sealed partial class MalfAiSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<MalfAiComponent, OpenModuleMenuEvent>(OnOpenModuleMenu);
        SubscribeLocalEvent<MalfAiComponent, ProgramOverrideEvent>(OnProgramOverride);
        SubscribeLocalEvent<MalfAiComponent, MachineOverloadEvent>(OnMachineOverload);
        SubscribeLocalEvent<MalfAiComponent, RCDDestructionEvent>(OnRCDDestruction);
    }
    private void OnOpenModuleMenu(EntityUid uid, MalfAiComponent comp, ref OpenModuleMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }
    private void OnProgramOverride(EntityUid uid, MalfAiComponent comp, ref ProgramOverrideEvent args)
    {
        var target = args.Target;

        if (HasComp<EmaggedComponent>(target))
            return;

        var addedControlPower = 0f;

        if (TryComp<ApcPowerProviderComponent>(target, out var targetComp))
        {
        addedControlPower += 2;
        EnsureComp<EmaggedComponent>(target);
        }

        if (TryComp<StoreComponent>(uid, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "ControlPower", 15 } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }
    }

#region destructive
    private void OnMachineOverload(EntityUid uid, MalfAiComponent comp, ref MachineOverloadEvent args)
    {
        var target = args.Target;
        var timespan = TimeSpan.FromSeconds(3f);
        _audio.PlayPvs(comp.AlarmSound, target);
        Timer.Spawn(timespan, () =>
        {
            _explosionSystem.QueueExplosion(
                (EntityUid) target,
                typeId: "Default",
                totalIntensity: 50,
                slope: 5,
                maxTileIntensity: 7);
        });
    }
    private void OnRCDDestruction(EntityUid uid, MalfAiComponent comp, ref RCDDestructionEvent args)
    {
        var RCDs = new List<Entity<RCDComponent>>();
        var query = EntityQueryEnumerator<RCDComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out var rcd, out var xform))
        {
            RCDs.Add((rcdUid, rcd));
        }

        var toExplode = RCDs.Count;
        if (toExplode == 0)
            return;

        for (var i = 0; i < toExplode; i++)
        {
            _explosionSystem.QueueExplosion(
                (RCDs[i]),
                typeId: "Default",
                totalIntensity: 50,
                slope: 9,
                maxTileIntensity: 10);
            QueueDel(RCDs[i]);
        }


    }
#endregion
}
