using Content.Shared._Funkystation.Clothing.Components;
using Content.Shared.Actions;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.SubFloor;

public abstract partial class SharedTrayScannerSystem
{
    [Dependency] private SharedAudioSystem _goobAudio = default!;
    [Dependency] private ActionContainerSystem _goobActionContainer = default!;

    private void GoobInitialize()
    {
        SubscribeLocalEvent<TrayScannerToggleComponent, MapInitEvent>(OnToggleMapInit);
        SubscribeLocalEvent<TrayScannerToggleComponent, GetItemActionsEvent>(OnToggleGetActions);
        SubscribeLocalEvent<TrayScannerToggleComponent, ToggleTrayScannerEvent>(OnToggleAction);
    }

    private void OnToggleMapInit(Entity<TrayScannerToggleComponent> ent, ref MapInitEvent args)
    {
        _goobActionContainer.EnsureAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
        Dirty(ent);
    }

    private void OnToggleGetActions(Entity<TrayScannerToggleComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags is not { } slot || (slot & ent.Comp.RequiredSlot) == 0)
            return;

        args.AddAction(ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnToggleAction(Entity<TrayScannerToggleComponent> ent, ref ToggleTrayScannerEvent args)
    {
        if (args.Handled || !TryComp<TrayScannerComponent>(ent, out var scanner))
            return;

        args.Handled = true;

        var enabled = !scanner.Enabled;
        SetScannerEnabled(ent, enabled, scanner);
        _goobAudio.PlayPredicted(enabled ? ent.Comp.SoundOn : ent.Comp.SoundOff, ent, args.Performer);
    }

    private void GoobScannerEnabledChanged(EntityUid uid, bool enabled)
    {
        if (!TryComp<GoggleShaderComponent>(uid, out var shader) || shader.Enabled == enabled)
            return;

        shader.Enabled = enabled;
        Dirty(uid, shader);

        var ev = new GoggleShaderToggledEvent(enabled);
        RaiseLocalEvent(uid, ref ev);
    }
}
