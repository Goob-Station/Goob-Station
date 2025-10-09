using Content.Goobstation.Shared.Wraith.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Medical;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Chat;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Nutrition;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed partial class CursedRotSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly VomitSystem _vomitSystem = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedRotComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CursedRotComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CursedRotComponent, IngestionAttemptEvent>(OnIngestAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CursedRotComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Cough Timer
            if (curTime >= comp.NextTickCough)
            {
                _chatSystem.TrySendInGameICMessage(uid, "coughs", InGameICChatType.Emote, false);

                // Schedule next tick
                comp.NextTickCough = curTime + comp.TimeTillCough;
            }

            // Puke Timer
            if (curTime >= comp.NextTickPuke)
            {
                //The code I copied from made you puke -1000, and yet -5 still makes you completely starve :shrugs:
                _vomitSystem.Vomit(uid, -5, -5);

                // Schedule next tick
                comp.NextTickPuke = curTime + comp.TimeTillPuke;
            }
        }
    }
    private void OnExamined(Entity<CursedRotComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
            //Tells the wraith that the target is cursed.
            args.PushMarkup($"[color=darkgreen]{Loc.GetString("wraith-cursed-rot", ("target", ent.Owner))}[/color]");
    }
    private void OnIngestAttempt(Entity<CursedRotComponent> ent, ref IngestionAttemptEvent args)
    {
        var uid = ent.Owner;

        if (args.Cancelled)
            return;

        // Prevent eating
        args.Cancel();

        _popup.PopupEntity("curse-rot-cant-eat", uid, uid);
    }
    private void OnMapInit(Entity<CursedRotComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTickPuke = _timing.CurTime + ent.Comp.TimeTillPuke;
        ent.Comp.NextTickCough = _timing.CurTime + ent.Comp.TimeTillCough;
    }
}
