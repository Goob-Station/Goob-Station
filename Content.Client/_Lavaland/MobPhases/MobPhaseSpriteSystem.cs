using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.MobPhases;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Utility;

namespace Content.Client._Lavaland.MobPhases;

public sealed class MobPhaseSpriteSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MobPhaseSpriteComponent, MobPhaseChangedEvent>(OnPhaseChanged);
        SubscribeNetworkEvent<BossFirstDamageEvent>(OnBossFirstDamage);

    }
    private void OnBossFirstDamage(BossFirstDamageEvent ev, EntitySessionEventArgs args)
    {
        var boss = GetEntity(ev.Boss);

        if (!TryComp(boss, out MobPhaseSpriteComponent? spriteComp))
            return;

        if (!TryComp(boss, out SpriteComponent? sprite))
            return;

        // First combat sprite is always phase 2. Hardcoded don't sue me. Fix it if you want.
        const int firstCombatPhase = 2;

        if (!spriteComp.Phases.TryGetValue(firstCombatPhase, out var data))
            return;

        if (!data.ChangeSprite)
            return;

        if (!string.IsNullOrWhiteSpace(data.Rsi))
        {
            var rsiPath = new ResPath(data.Rsi);
            var rsi = _resourceCache.GetResource<RSIResource>(rsiPath).RSI;
            sprite.LayerSetRSI(data.Layer, rsi);
        }

        if (!string.IsNullOrWhiteSpace(data.State))
        {
            sprite.LayerSetState(data.Layer, data.State);
        }

        _spriteSystem.ForceUpdate(boss);
    }

    private void OnPhaseChanged(Entity<MobPhaseSpriteComponent> ent, ref MobPhaseChangedEvent args)
    {
        if (!ent.Comp.Phases.TryGetValue(args.NewPhase, out var data))
            return;

        if (!data.ChangeSprite)
            return;

        if (!TryComp(ent.Owner, out SpriteComponent? sprite))
            return;

        sprite.LayerSetState(data.Layer, data.State);

        if (!string.IsNullOrWhiteSpace(data.Rsi))
        {
            var rsiPath = new ResPath(data.Rsi);
            var rsi = _resourceCache.GetResource<RSIResource>(rsiPath).RSI;

            sprite.LayerSetRSI(data.Layer, rsi);

            _spriteSystem.QueueUpdateIsInert(new(ent.Owner, sprite));
        }

        if (!string.IsNullOrWhiteSpace(data.State))
        {
            sprite.LayerSetState(data.Layer, data.State);
        }

        _spriteSystem.ForceUpdate(ent.Owner);
    }
}
