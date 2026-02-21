using Content.Shared._ES.PainFlash;
using Content.Shared._ES.PainFlash.Components;
using Content.Shared.CCVar;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._ES.PainFlash;

/// <inheritdoc/>
public sealed class ESPainFlashSystem : ESSharedPainFlashSystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private ESPainFlashOverlay _overlay = default!;

    private bool _reducedMotion;

    private readonly List<ESPainFlashInstance> _painInstances = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESPainFlashComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ESPainFlashComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ESPainFlashComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ESPainFlashComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<ESPainFlashMessage>(OnPainFlashMessage);

        _config.OnValueChanged(CCVars.ReducedMotion, b => { _reducedMotion = b; }, invokeImmediately: true);

        _overlay = new();
    }

    private void OnPlayerAttached(Entity<ESPainFlashComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
        _overlay.ResetPainAccumulator();
    }

    private void OnPlayerDetached(Entity<ESPainFlashComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
        _overlay.ResetPainAccumulator();
    }

    private void OnInit(Entity<ESPainFlashComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent)
            return;
        _overlayManager.AddOverlay(_overlay);
        _overlay.ResetPainAccumulator();
    }

    private void OnShutdown(Entity<ESPainFlashComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent)
            return;
        _overlayManager.RemoveOverlay(_overlay);
        _overlay.ResetPainAccumulator();
    }

    private void OnPainFlashMessage(ESPainFlashMessage ev)
    {
        if (_reducedMotion)
            return;

        // Track if we've already had a matching pain flash play on the client.
        // Note that this only works because the number of pain flashes from the server
        // is always greater than or equal to the number of pain flashes on the client.
        if (_painInstances.Remove(new ESPainFlashInstance(ev.Damage, ev.Tick)))
            return;

        _overlay.AddPain(ev.Damage);
    }

    protected override void OnDamageChanged(Entity<ESPainFlashComponent> ent, ref DamageChangedEvent args)
    {
        if (_reducedMotion)
            return;

        if (_player.LocalEntity != ent)
            return;

        if (Timing.ApplyingState || !Timing.IsFirstTimePredicted)
            return;

        if (!IsPainFlashTrigger(args, out var damage))
            return;

        // Always log pain instances for flashes we play directly form the client.
        _painInstances.Add(new ESPainFlashInstance(damage, Timing.CurTick));

        _overlay.AddPain(damage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_painInstances.Count == 0)
            return;

        // Technically should not be necessary but this is a memory leak waiting to happen.
        // dumps all pain instances older than some arbitrary number. This is basically just a lag buffer in case
        // we somehow never receive the corresponding server pain flash message.
        _painInstances.RemoveAll(p => (int) Timing.CurTick.Value - (int) p.Tick.Value > 5000);
    }
}

public readonly record struct ESPainFlashInstance(FixedPoint2 Damage, GameTick Tick);
