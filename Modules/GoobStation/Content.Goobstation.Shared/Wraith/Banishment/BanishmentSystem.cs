using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Banishment;

/// <summary>
/// Revives once dead to give you a second chance, with side-effects (if needed).
/// </summary>
public sealed class BanishmentSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BanishmentComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<BanishmentComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMapInit(Entity<BanishmentComponent> ent, ref MapInitEvent args) =>
        ent.Comp.MaxLives = ent.Comp.Lives;

    private void OnMobStateChanged(Entity<BanishmentComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != ent.Comp.MobStateTrigger)
            return;

        if (ent.Comp.Lives <= 0)
        {
            var doneEv = new BanishmentDoneEvent();
            RaiseLocalEvent(ent, ref doneEv);

            return;
        }

        if (_netManager.IsClient)
            return;

        var rej = new RejuvenateEvent();
        RaiseLocalEvent(ent, rej);

        if (ent.Comp.Popup is {} popup)
            _popupSystem.PopupEntity(Loc.GetString(popup), ent.Owner, ent.Owner, PopupType.MediumCaution);

        var banishEv = new BanishmentEvent(ent.Comp.Lives);
        RaiseLocalEvent(ent, ref banishEv);

        // Reduce 1 life
        ent.Comp.Lives = Math.Max(ent.Comp.Lives - 1, 0);
        Dirty(ent);

        _admin.Add(LogType.Respawn, LogImpact.High,
            $"{ToPrettyString(ent.Owner)} got revived by Banishment, and now has {ent.Comp.Lives} lives left.");
    }
}
