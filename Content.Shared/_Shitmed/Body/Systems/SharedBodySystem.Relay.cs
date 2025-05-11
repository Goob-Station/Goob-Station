// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<BodyComponent, GetDoAfterDelayMultiplierEvent>(RelayBodyPartEvent);


    }

    protected void RefRelayBodyPartEvent<T>(EntityUid uid, BodyComponent component, ref T args) where T : IBodyPartRelayEvent
    {
        RelayEvent((uid, component), ref args);
    }

    protected void RelayBodyPartEvent<T>(EntityUid uid, BodyComponent component, T args) where T : IBodyPartRelayEvent
    {
        RelayEvent((uid, component), args);
    }

    public void RelayEvent<T>(Entity<BodyComponent> body, ref T args) where T : IBodyPartRelayEvent
    {
        // this copies the by-ref event if it is a struct
        var ev = new BodyPartRelayedEvent<T>(args);
        foreach (var part in GetBodyChildrenOfType(body.Owner, args.TargetBodyPart, body.Comp))
        {
            RaiseLocalEvent(part.Id, ev);
        }

        // and now we copy it back
        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<BodyComponent> body, T args) where T : IBodyPartRelayEvent
    {
        var ev = new BodyPartRelayedEvent<T>(args);

        foreach (var part in GetBodyChildrenOfType(body.Owner, args.TargetBodyPart, body.Comp))
        {
            RaiseLocalEvent(part.Id, ev);
        }
    }
}

public sealed class BodyPartRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public BodyPartRelayedEvent(TEvent args)
    {
        Args = args;
    }
}

/// <summary>
///     Events that should be relayed to body parts should implement this interface.
/// </summary>
public interface IBodyPartRelayEvent
{
    /// <summary>
    ///     What body part should this event be relayed to, if any?
    /// </summary>
    public BodyPartType TargetBodyPart { get; }
}