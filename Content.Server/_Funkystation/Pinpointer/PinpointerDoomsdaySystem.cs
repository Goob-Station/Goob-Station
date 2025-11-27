// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared._Funkystation.MalfAI.Events;
using Content.Shared.Nuke;
using Content.Shared.Pinpointer;
using System.Linq;

namespace Content.Server._Funkystation.Pinpointer;

/// <summary>
/// Handles pinpointer targeting changes during MalfAI doomsday protocol.
/// Non-EMAGged pinpointers will switch to target AI cores when doomsday protocol is initiated,
/// and revert to nuclear authentication disk tracking when the AI brain is destroyed or no longer exists.
/// </summary>
public sealed class PinpointerDoomsdaySystem : EntitySystem
{
    [Dependency] private readonly SharedPinpointerSystem _pinpointer = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    // Track which pinpointers have been switched to AI targeting during doomsday
    private readonly HashSet<EntityUid> _doomsdayTargetedPinpointers = new();
    private EntityUid? _currentDoomsdayAiBrain = null;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiDoomsdayStartedEvent>(OnDoomsdayStarted);
        SubscribeLocalEvent<EntityTerminatingEvent>(OnEntityTerminating);
    }

    private void OnDoomsdayStarted(MalfAiDoomsdayStartedEvent ev)
    {
        Console.WriteLine("[DEBUG_LOG] PinpointerDoomsdaySystem: Received MalfAiDoomsdayStartedEvent");

        // Track the AI brain specifically to find it in the event they're shunted
        var aiBrain = ev.Ai;

        Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Station: {ev.Station}, AI Brain: {aiBrain}");

        // Find all pinpointers that haven't been EMAGged and switch them to target the AI brain
        var pinpointerCount = 0;
        var switchedCount = 0;
        var pinpointerQuery = EntityQueryEnumerator<PinpointerComponent>();
        while (pinpointerQuery.MoveNext(out var pinpointerUid, out var pinpointer))
        {
            pinpointerCount++;

            // Skip if pinpointer is EMAGged (they should keep their current target)
            if (_emag.CheckFlag(pinpointerUid, EmagType.Interaction))
            {
                System.Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Pinpointer {pinpointerUid} is EMAGged, skipping");
                continue;
            }

            // Use the existing SetTargetWithCustomName method to set a custom name
            // This avoids needing to modify the UpdateTargetName flag which has access restrictions
            _pinpointer.SetTargetWithCustomName(pinpointerUid, aiBrain, "Station AI Brain", pinpointer);

            // Track this pinpointer so we can revert it later if needed
            _doomsdayTargetedPinpointers.Add(pinpointerUid);

            switchedCount++;
            System.Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Switched pinpointer {pinpointerUid} to target AI brain {aiBrain}");
        }

        System.Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Processed {pinpointerCount} pinpointers, switched {switchedCount} to AI brain");

        // Store the AI brain for monitoring
        _currentDoomsdayAiBrain = aiBrain;
    }

    private void OnEntityTerminating(ref EntityTerminatingEvent ev)
    {
        // If the AI brain we're tracking gets deleted, revert all pinpointers
        if (_currentDoomsdayAiBrain == ev.Entity.Owner)
        {
            Console.WriteLine("[DEBUG_LOG] PinpointerDoomsdaySystem: AI brain deleted, reverting pinpointers");
            RevertPinpointersToNuclearDisk();
        }
    }

    private void RevertPinpointersToNuclearDisk()
    {
        if (_doomsdayTargetedPinpointers.Count == 0)
            return;

        // Find nuclear authentication disk on any station
        var nukeDiskQuery = EntityQueryEnumerator<NukeDiskComponent>();
        EntityUid? nukeDisk = null;

        while (nukeDiskQuery.MoveNext(out var diskUid, out _))
        {
            nukeDisk = diskUid;
            break; // Use the first nuclear disk found
        }

        Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Found nuclear disk: {nukeDisk}");

        var revertedCount = 0;
        foreach (var pinpointerUid in _doomsdayTargetedPinpointers.ToList())
        {
            // Check if pinpointer still exists
            if (!EntityManager.EntityExists(pinpointerUid))
            {
                _doomsdayTargetedPinpointers.Remove(pinpointerUid);
                continue;
            }

            if (!TryComp<PinpointerComponent>(pinpointerUid, out var pinpointer))
            {
                _doomsdayTargetedPinpointers.Remove(pinpointerUid);
                continue;
            }

            // Revert to nuclear disk targeting with original configuration
            if (nukeDisk.HasValue)
            {
                // Use SetTargetWithCustomName to set proper target name like the original prototypes
                _pinpointer.SetTargetWithCustomName(pinpointerUid, nukeDisk.Value, "nuclear authentication disk", pinpointer);
            }
            else
            {
                // If no nuclear disk found, clear the target
                _pinpointer.SetTarget(pinpointerUid, null, pinpointer);
            }

            revertedCount++;
            Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Reverted pinpointer {pinpointerUid} back to nuclear disk");
        }

        Console.WriteLine($"[DEBUG_LOG] PinpointerDoomsdaySystem: Reverted {revertedCount} pinpointers back to nuclear disk tracking");

        // Clear tracking data
        _doomsdayTargetedPinpointers.Clear();
        _currentDoomsdayAiBrain = null;
    }
}
