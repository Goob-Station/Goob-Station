using Content.Server.Chat.Systems;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Shitmed.DelayedDeath;

public partial class DelayedDeathSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DelayedDeathComponent, TargetBeforeDefibrillatorZapsEvent>(OnDefibZap);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        using var query = EntityQueryEnumerator<DelayedDeathComponent, MobStateComponent>();
        while (query.MoveNext(out var ent, out var comp, out var mob))
        {
            comp.DeathTimer += frameTime;

            if (comp.DeathTimer >= comp.DeathTime && !_mobState.IsDead(ent, mob))
            {
                // go crit then dead so deathgasp can happen
                _mobState.ChangeMobState(ent, MobState.Critical, mob);
                _mobState.ChangeMobState(ent, MobState.Dead, mob);
            }
        }
    }

    private void OnDefibZap(Entity<DelayedDeathComponent> ent, ref TargetBeforeDefibrillatorZapsEvent args)
    {
        // can't defib someone without a heart or brain pal
        args.Cancel();

        _chat.TrySendInGameICMessage(args.Defib, Loc.GetString("defibrillator-missing-organs"),
            InGameICChatType.Speak, true);
    }
}
