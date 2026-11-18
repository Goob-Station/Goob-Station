using Content.Client.Alerts;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Slasher.Systems;

public sealed partial class SlasherSoulBonusAlertSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    [SubscribeLocalEvent]
    private void OnUpdateAlertSprite(Entity<SlasherSoulBonusAlertComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (!TryComp<SlasherSoulStealComponent>(args.ViewerEnt, out var soulSteal)
            || soulSteal.Alert != args.Alert)
            return;

        SetRow(ent, ent.Comp.DamageDigitKeys, soulSteal.TotalAppliedDamageBonus.GetTotal().Int());
        SetRow(ent, ent.Comp.ArmorDigitKeys, (int) soulSteal.ArmorPercent);
    }

    private void SetRow(EntityUid uid, List<string> keys, int amount)
    {
        amount = Math.Clamp(amount, 0, (int) Math.Pow(10, keys.Count) - 1);

        for (var i = 0; i < keys.Count; i++)
        {
            var digit = amount % 10;
            var visible = i == 0 || amount > 0;
            amount /= 10;

            if (!_sprite.LayerMapTryGet(uid, keys[i], out var layer, false))
                continue;

            _sprite.LayerSetVisible(uid, layer, visible);
            if (visible)
                _sprite.LayerSetRsiState(uid, layer, digit.ToString());
        }
    }
}
