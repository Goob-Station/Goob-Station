using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Interaction.GunBlock;

/// <summary>
/// Cancels gun firing attempts
/// </summary>
public sealed class SlasherGunBlockedSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // Server-side cooldown to avoid popup spam while holding fire
    private readonly Dictionary<EntityUid, TimeSpan> _lastPopup = new();
    private static readonly TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunBlockedComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<GunBlockedComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.User != ent.Owner)
            return;

        args.Cancel();

        if (_net.IsClient)
            return;

        var now = _timing.CurTime;
        if (_lastPopup.TryGetValue(ent.Owner, out var last) && now < last + PopupCooldown)
            return;

        _lastPopup[ent.Owner] = now;
        _popup.PopupEntity(Loc.GetString(ent.Comp.PopupText), ent.Owner, ent.Owner, PopupType.MediumCaution);
    }
}
