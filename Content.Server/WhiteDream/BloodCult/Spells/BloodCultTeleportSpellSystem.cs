using Content.Server.DoAfter;
using Content.Server.WhiteDream.BloodCult.Runes;
using Content.Server.WhiteDream.BloodCult.Runes.Teleport;
using Content.Shared.DoAfter;
using Content.Shared.ListViewSelector;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.Audio;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Spells;

public sealed class BloodCultTeleportSpellSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly CultRuneTeleportSystem _runeTeleport = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultTeleportEvent>(OnTeleport);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, ListViewItemSelectedMessage>(OnTeleportRuneSelected);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, TeleportActionDoAfterEvent>(OnTeleportDoAfter);
    }

    private void OnTeleport(BloodCultTeleportEvent ev)
    {
        if (ev.Handled || !_runeTeleport.TryGetTeleportRunes(ev.Performer, out var runes))
            return;

        var metaData = new Dictionary<string, object>
        {
            ["target"] = GetNetEntity(ev.Target),
            ["duration"] = ev.DoAfterDuration
        };

        _ui.SetUiState(ev.Performer, ListViewSelectorUiKey.Key, new ListViewSelectorState(runes, metaData));
        _ui.TryToggleUi(ev.Performer, ListViewSelectorUiKey.Key, ev.Performer);
        ev.Handled = true;
    }

    private void OnTeleportRuneSelected(
        Entity<BloodCultSpellsHolderComponent> ent,
        ref ListViewItemSelectedMessage args
    )
    {
        if (!args.MetaData.TryGetValue("target", out var rawTarget) || rawTarget is not NetEntity netTarget ||
            !args.MetaData.TryGetValue("duration", out var rawDuration) || rawDuration is not TimeSpan duration)
            return;

        var target = GetEntity(netTarget);
        var teleportDoAfter = new TeleportActionDoAfterEvent
        {
            Rune = GetNetEntity(EntityUid.Parse(args.SelectedItem.Id))
        };
        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, duration, teleportDoAfter, target, target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnTeleportDoAfter(Entity<BloodCultSpellsHolderComponent> user, ref TeleportActionDoAfterEvent ev)
    {
        if (ev.Target is not { } target)
            return;

        var rune = GetEntity(ev.Rune);
        _audio.PlayPvs(ev.TeleportOutSound, target);

        _cultRune.StopPulling(target);
        _transform.SetCoordinates(target, Transform(rune).Coordinates);

        _audio.PlayPvs(ev.TeleportInSound, rune);
    }
}
