using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Shitcode.Heretic.EntitySystems;

public sealed class SacramentsSystem : SharedSacramentsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacramentsOfPowerComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<SacramentsOfPowerComponent, AppearanceComponent>();
        while (query.MoveNext(out var uid, out var comp, out var appearance))
        {
            if (comp.State == SacramentsState.Closing || time < comp.StateUpdateAt)
                continue;

            switch (comp.State)
            {
                case SacramentsState.Opening:
                    _appearance.SetData(uid, SacramentsKey.Key, SacramentsState.Open, appearance);
                    comp.StateUpdateAt = _timing.CurTime + comp.EffectTime;
                    _audio.PlayEntity(comp.Sound, Filter.Pvs(uid), uid, true);
                    comp.State = SacramentsState.Open;
                    Dirty(uid, comp);
                    break;
                case SacramentsState.Open:
                    _appearance.SetData(uid, SacramentsKey.Key, SacramentsState.Closing, appearance);
                    comp.StateUpdateAt = _timing.CurTime + comp.DeactivationTime;
                    comp.State = SacramentsState.Closing;
                    Dirty(uid, comp);
                    break;
            }
        }
    }

    private void OnMapInit(Entity<SacramentsOfPowerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.StateUpdateAt = _timing.CurTime + ent.Comp.ActivationTime;
        var appearance = EnsureComp<AppearanceComponent>(ent);
        _appearance.SetData(ent, SacramentsKey.Key, SacramentsState.Opening, appearance);
        _audio.PlayPvs(ent.Comp.ActivationSound, ent);
    }

    protected override void Pulse(EntityUid ent)
    {
        base.Pulse(ent);

        var ev = new SacramentsPulseEvent(GetNetEntity(ent));
        RaiseNetworkEvent(ev, Filter.Pvs(ent));
    }
}
