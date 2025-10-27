using Content.Server._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.UserInterface;
using Robust.Shared.Utility;

namespace Content.Server._DV.CosmicCult;

public sealed partial class CosmicCultSystem : SharedCosmicCultSystem
{
    /// <summary>
    ///     Used to calculate when the finale song should start playing
    /// </summary>
    public void SubscribeFinale()
    {
        SubscribeLocalEvent<CosmicFinaleComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<CosmicFinaleComponent, StartFinaleDoAfterEvent>(OnFinaleStartDoAfter);
        SubscribeLocalEvent<CosmicFinaleComponent, CancelFinaleDoAfterEvent>(OnFinaleCancelDoAfter);
    }

    private void OnInteract(Entity<CosmicFinaleComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(args.User))
            return; // humanoids only!

        if (!EntityIsCultist(args.User) && !args.Handled && ent.Comp.FinaleActive)
        {
            ent.Comp.Occupied = true;
            var doargs = new DoAfterArgs(EntityManager,
                args.User,
                ent.Comp.InteractionTime,
                new CancelFinaleDoAfterEvent(), ent, ent)
            {
                DistanceThreshold = 1f, Hidden = false, BreakOnHandChange = true, BreakOnDamage = true, BreakOnMove = true
            };
            _popup.PopupEntity(Loc.GetString("cosmiccult-finale-cancel-begin"), args.User, args.User);
            _doAfter.TryStartDoAfter(doargs);
            args.Handled = true;
        }
        else if (EntityIsCultist(args.User) && !args.Handled && !ent.Comp.FinaleActive && ent.Comp.CurrentState != FinaleState.Unavailable)
        {
            ent.Comp.Occupied = true;
            var doargs = new DoAfterArgs(EntityManager,
                args.User,
                ent.Comp.InteractionTime,
                new StartFinaleDoAfterEvent(),
                ent,
                ent)
            {
                DistanceThreshold = 1f, Hidden = false, BreakOnHandChange = true, BreakOnDamage = true, BreakOnMove = true
            };
            _popup.PopupEntity(Loc.GetString("cosmiccult-finale-beckon-begin"), args.User, args.User);
            _doAfter.TryStartDoAfter(doargs);
            args.Handled = true;
        }
    }

    private void OnFinaleStartDoAfter(Entity<CosmicFinaleComponent> uid, ref StartFinaleDoAfterEvent args)
    {
        if (args.Args.Target == null || args.Cancelled || args.Handled)
        {
            uid.Comp.Occupied = false;
            return;
        }
        _popup.PopupEntity(Loc.GetString("cosmiccult-finale-beckon-success"), args.Args.User, args.Args.User);
        StartFinale(uid);
    }

    private void StartFinale(Entity<CosmicFinaleComponent> uid)
    {
        var comp = uid.Comp;
        var indicatedLocation = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((uid, Transform(uid))));

        if (!TryComp<MonumentComponent>(uid, out var monument)
            || !TryComp<CosmicCorruptingComponent>(uid, out var corruptingComp))
            return;

        if (uid.Comp.CurrentState == FinaleState.ReadyBuffer)
        {
            _corrupting.SetCorruptionTime((uid, corruptingComp), TimeSpan.FromSeconds(3));
            _appearance.SetData(uid, MonumentVisuals.FinaleReached, 2);
            comp.BufferTimer = _timing.CurTime + comp.BufferRemainingTime;
            comp.SelectedSong = comp.BufferMusic;
            _sound.DispatchStationEventMusic(uid, comp.SelectedSong, StationEventMusicType.CosmicCult);

            _chatSystem.DispatchStationAnnouncement(uid,
            Loc.GetString("cosmiccult-finale-location", ("location", indicatedLocation)),
            null, false, null,
            Color.FromHex("#cae8e8"));

            uid.Comp.CurrentState = FinaleState.ActiveBuffer;
        }
        else
        {
            _corrupting.SetCorruptionTime((uid, corruptingComp), TimeSpan.FromSeconds(1));
            _appearance.SetData(uid, MonumentVisuals.FinaleReached, 3);
            comp.FinaleTimer = _timing.CurTime + comp.FinaleRemainingTime;
            comp.SelectedSong = comp.FinaleMusic;
            _sound.DispatchStationEventMusic(uid, comp.SelectedSong, StationEventMusicType.CosmicCult);
            _chatSystem.DispatchStationAnnouncement(uid,
            Loc.GetString("cosmiccult-finale-location", ("location", indicatedLocation)),
            null, false, null,
            Color.FromHex("#cae8e8"));

            uid.Comp.CurrentState = FinaleState.ActiveFinale;
        }

        var stationUid = _station.GetStationInMap(Transform(uid).MapID);

        if (stationUid != null)
            _alert.SetLevel(stationUid.Value, "octarine", true, true, true, true);

        if (TryComp<ActivatableUIComponent>(uid, out var uiComp))
            uiComp.Key = MonumentKey.Key; // wow! This is the laziest way to enable a UI ever!

        _monument.Enable((uid, monument));
        comp.FinaleActive = true;

        Dirty(uid, monument);
        _ui.SetUiState(uid.Owner, MonumentKey.Key, new MonumentBuiState(monument));
    }

    private void OnFinaleCancelDoAfter(Entity<CosmicFinaleComponent> uid, ref CancelFinaleDoAfterEvent args)
    {
        var comp = uid.Comp;
        if (args.Args.Target is not {} target || args.Cancelled || args.Handled)
        {
            uid.Comp.Occupied = false;
            return;
        }

        var stationUid = _station.GetOwningStation(uid);

        if (stationUid != null)
            _alert.SetLevel(stationUid.Value, "green", true, true, true);

        _sound.PlayGlobalOnStation(uid, _audio.ResolveSound(comp.CancelEventSound));
        _sound.StopStationEventMusic(uid, StationEventMusicType.CosmicCult);

        if (uid.Comp.CurrentState == FinaleState.ActiveBuffer)
        {
            uid.Comp.CurrentState = FinaleState.ReadyBuffer;
            comp.BufferRemainingTime = comp.BufferTimer - _timing.CurTime + TimeSpan.FromSeconds(15);
        }
        else if (uid.Comp.CurrentState == FinaleState.ActiveFinale)
        {
            uid.Comp.CurrentState = FinaleState.ReadyFinale;
        }

        if (TryComp<CosmicCorruptingComponent>(uid, out var corruptingComp))
            _corrupting.SetCorruptionTime((uid, corruptingComp), TimeSpan.FromSeconds(6));

        if (TryComp<ActivatableUIComponent>(uid, out var uiComp))
        {
            _ui.CloseUi(uid.Owner, MonumentKey.Key);

            uiComp.Key = null; //kazne called this the laziest way to disable a UI ever
        }

        _appearance.SetData(uid, MonumentVisuals.FinaleReached, 1);

        if (!TryComp<MonumentComponent>(target, out var monument))
            return;

        _monument.Disable((uid, monument));
        comp.FinaleActive = false;

        Dirty(target, monument);
        _ui.SetUiState(uid.Owner, MonumentKey.Key, new MonumentBuiState(monument));
    }
}
