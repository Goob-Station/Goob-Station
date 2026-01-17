// SPDX-FileCopyrightText: 2026 Site-14 Contributors
//
// SPDX-License-Identifier: MPL-2.0
//
// Additional Use Restrictions apply:
// See /LICENSES/SITE14-ADDENDUM.md

using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Input;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Blinking;

public sealed class BlinkingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkingComponent, MapInitEvent>(OnMapInit);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Blink,
                InputCmdHandler.FromDelegate(session => OnBlinkPressed(session, true),
                    session => OnBlinkPressed(session, false), false, false))
            .Register<BlinkingSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<BlinkingSystem>();
    }

    private void OnMapInit(Entity<BlinkingComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastBlinkTime = _timing.CurTime;
        Dirty(ent);
    }

    private void OnBlinkPressed(ICommonSession? session, bool pressed)
    {
        if (session?.AttachedEntity is not { } uid)
            return;

        if (!TryComp<BlinkingComponent>(uid, out var comp))
            return;

        if (pressed
            && !comp.IsBlinking
            && !comp.IsHoldingClosed)
        {
            StartHoldingClosed((uid, comp));
        }
        else if (comp.IsHoldingClosed)
        {
            StopHoldingClosed((uid, comp));
        }
    }

    private void StartHoldingClosed(Entity<BlinkingComponent> ent)
    {
        var comp = ent.Comp;
        comp.IsHoldingClosed = true;
        comp.IsBlinking = true;
        comp.BlinkStartTime = _timing.CurTime;
        comp.CurrentClosedDuration = 0f;
        Dirty(ent);

        if (comp.BlinkSound != null)
            _audio.PlayPredicted(comp.BlinkSound, ent, ent);
    }

    private void StopHoldingClosed(Entity<BlinkingComponent> ent)
    {
        var comp = ent.Comp;
        comp.IsHoldingClosed = false;

        comp.CurrentClosedDuration = comp.MinClosedDuration;

        comp.BlinkStartTime = _timing.CurTime - TimeSpan.FromSeconds(comp.CloseAnimationTime);

        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BlinkingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsHoldingClosed)
            {
                comp.LastBlinkTime = _timing.CurTime;
                continue;
            }

            if (comp.IsBlinking)
            {
                var blinkElapsed = (float)(_timing.CurTime - comp.BlinkStartTime).TotalSeconds;
                var totalDuration = comp.CloseAnimationTime + comp.CurrentClosedDuration + comp.OpenAnimationTime;
                if (blinkElapsed >= totalDuration)
                    EndBlink((uid, comp));

                continue;
            }

            if (!comp.AutoBlink)
                continue;

            var timeSinceBlink = (float)(_timing.CurTime - comp.LastBlinkTime).TotalSeconds;
            if (timeSinceBlink >= comp.MaxTimeWithoutBlink)
                TriggerBlink((uid, comp));
        }
    }

    public void TriggerBlink(Entity<BlinkingComponent> ent)
    {
        var comp = ent.Comp;
        if (comp.IsBlinking || comp.IsHoldingClosed)
            return;

        var timeSinceBlink = (float)(_timing.CurTime - comp.LastBlinkTime).TotalSeconds;

        var progress = Math.Clamp(timeSinceBlink / comp.MaxTimeWithoutBlink, 0f, 1f);
        comp.CurrentClosedDuration = MathHelper.Lerp(comp.MinClosedDuration, comp.MaxClosedDuration, progress);

        comp.IsBlinking = true;
        comp.BlinkStartTime = _timing.CurTime;
        Dirty(ent);

        if (comp.BlinkSound != null)
            _audio.PlayPredicted(comp.BlinkSound, ent, ent);
    }

    private void EndBlink(Entity<BlinkingComponent> ent)
    {
        ent.Comp.IsBlinking = false;
        ent.Comp.LastBlinkTime = _timing.CurTime;
        Dirty(ent);
    }

    public float GetBlinkUrgency(BlinkingComponent comp)
    {
        if (comp.IsBlinking || comp.IsHoldingClosed || !comp.AutoBlink)
            return 0f;

        var timeSinceBlink = (float)(_timing.CurTime - comp.LastBlinkTime).TotalSeconds;

        if (timeSinceBlink < comp.BlurStartTime)
            return 0f;

        return (timeSinceBlink - comp.BlurStartTime) * comp.BlurGrowthRate;
    }

    public float GetEyelidClosure(BlinkingComponent comp)
    {
        if (!comp.IsBlinking && !comp.IsHoldingClosed)
            return 0f;

        var elapsed = (float)(_timing.CurTime - comp.BlinkStartTime).TotalSeconds;
        var closeEnd = comp.CloseAnimationTime;

        if (comp.IsHoldingClosed)
        {
            if (elapsed < closeEnd)
                return comp.CloseAnimationTime > 0f ? elapsed / comp.CloseAnimationTime : 1f;
            return 1f;
        }

        var closedEnd = closeEnd + comp.CurrentClosedDuration;
        var totalEnd = closedEnd + comp.OpenAnimationTime;

        if (elapsed < closeEnd)
        {
            return comp.CloseAnimationTime > 0f ? elapsed / comp.CloseAnimationTime : 1f;
        }
        else if (elapsed < closedEnd)
        {
            return 1f;
        }
        else if (elapsed < totalEnd)
        {
            var openElapsed = elapsed - closedEnd;
            return comp.OpenAnimationTime > 0f ? 1f - openElapsed / comp.OpenAnimationTime : 0f;
        }

        return 0f;
    }
}
