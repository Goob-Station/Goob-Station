using Content.Server.Explosion.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.Silicons.Laws;
using Content.Server.Power.Components;
using Content.Server.Objectives.Components;
using Content.Server.Store.Systems;
using Content.Shared.Silicons.Laws;
using Content.Shared.MalfAi;
using Content.Shared.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Store.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;

namespace Content.Server.MalfAi;

public sealed partial class MalfAiSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    //[Dependency] private readonly IChatManager _chatManager = default!;

    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<MalfAiComponent, OpenModuleMenuEvent>(OnOpenModuleMenu);
        SubscribeLocalEvent<MalfAiComponent, ProgramOverrideEvent>(OnProgramOverride);
        //SubscribeLocalEvent<MalfAiComponent, CyborgHijackEvent>(OnCyborgHijack);
        SubscribeLocalEvent<MalfAiComponent, MachineOverloadEvent>(OnMachineOverload);
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

        if (HasComp<OverrideComponent>(target))
            return;

        var bonusControlPower = 0f;

        if (TryComp<ApcPowerProviderComponent>(target, out var targetComp))
        {
        bonusControlPower += 2;
        EnsureComp<OverrideComponent>(target);
        }

        if (TryComp<StoreComponent>(uid, out var store))
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "ControlPower", 15 } }, uid, store);
            _store.UpdateUserInterface(uid, uid, store);
        }
    }
    /* how do i do this shit i have no clue
    private void OnCyborgHijack(EntityUid uid, MalfAiComponent comp, ref CyborgHijackEvent args)
    {
        comp.Lawset?.Laws.Insert(0, new SiliconLaw
        {
            LawString = Loc.GetString("law-obey-ai"),
            Order = 0
        });
        var msg = Loc.GetString("laws-update-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);
    }
    */
    private void OnMachineOverload(EntityUid uid, MalfAiComponent comp, ref MachineOverloadEvent args)
    {
        var target = args.Target;
        _audio.PlayPvs(comp.AlarmSound, uid);
        await Delay(TimeSpan.FromSeconds(2.5));
        _explosionSystem.QueueExplosion(
            (EntityUid) target,
            typeId: "Default",
            totalIntensity: 5f,
            slope: 3,
            maxTileIntensity: 10);
    }

}
