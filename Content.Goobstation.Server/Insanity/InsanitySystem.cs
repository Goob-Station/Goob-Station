// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Traits.Assorted;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Insanity;

public sealed partial class InsanitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StunSystem _stunSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<InsanityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextInsanityTick )
                continue;


            ClearPreviousEffect(comp, uid);
            TryInsanityEffect(uid, comp);


            comp.NextInsanityTick = _timing.CurTime + comp.ExecutionInterval;
        }
    }

    private void ClearPreviousEffect(InsanityComponent comp, EntityUid uid)
    {
        if (comp.IsBlinded)
        {
            RemComp<TemporaryBlindnessComponent>(uid);
            comp.IsBlinded = false;
        }

        if (comp.IsMuted)
        {
            RemComp<MutedComponent>(uid);
            comp.IsMuted = false;
        }

    }

    private void TryInsanityEffect(EntityUid uid, InsanityComponent comp)
    {
        var seed = _random.Next(0, 6);

        switch (seed)
        {
            case 0:
            {
                _popupSystem.PopupEntity(Loc.GetString("insanity-comp-one"), uid, uid, PopupType.LargeCaution);
                break;
            }
            case 1:
            {
                _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(4), true);
                break;
            }
            case 2:
            {
                var schizo = new SoundPathSpecifier("Audio/Ambience/ambigen2.ogg");
                _audioSystem.PlayLocal(schizo, uid, uid);
                break;
            }
            case 3:
            {
                _popupSystem.PopupEntity(Loc.GetString("insanity-comp-two"), uid, uid, PopupType.LargeCaution);
                break;
            }
            case 4:
            {
                EnsureComp<TemporaryBlindnessComponent>(uid);
                comp.IsBlinded = true;
                break;
            }
            case 5:
            {
                EnsureComp<MutedComponent>(uid);
                comp.IsMuted = true;
                break;
            }

        }
    }
}
