using System.Linq;
using Content.Server._Mono.Projectiles.TargetGuided;
using Content.Shared._Mono.FireControl;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Shuttles.Components;
using EntityCoordinates = Robust.Shared.Map.EntityCoordinates;

namespace Content.Server._Mono.FireControl;

public sealed partial class FireControlSystem
{
    [Dependency] private readonly TargetGuidedSystem _targetGuided = null!;

    /// <summary>
    /// List of active guided missiles that need cursor position updates
    /// </summary>
    private readonly HashSet<EntityUid> _activeMissiles = new();

    /// <summary>
    /// Map of console entities to their current mouse positions
    /// </summary>
    private readonly Dictionary<EntityUid, EntityCoordinates> _consoleMousePositions = new();

    /// <summary>
    /// Registers handlers for events related to target guided projectiles.
    /// </summary>
    private void InitializeTargetGuided()
    {
        SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnTargetGuidedShot);
        SubscribeLocalEvent<TargetGuidedComponent, ComponentShutdown>(OnGuidedMissileShutdown);
        // Track fire messages to update cursor positions
        SubscribeLocalEvent<FireControlConsoleComponent, FireControlConsoleFireEvent>(OnConsoleFireEvent);
    }

    /// <summary>
    /// Track console fire events to update cursor positions
    /// </summary>
    private void OnConsoleFireEvent(EntityUid uid, FireControlConsoleComponent component, FireControlConsoleFireEvent args)
    {
        // Store the current mouse position for this console
        _consoleMousePositions[uid] = GetCoordinates(args.Coordinates);
    }

    /// <summary>
    /// Subscribed to AmmoShotEvent to check for and configure guided projectiles.
    /// </summary>
    private void OnTargetGuidedShot(EntityUid uid, GunComponent component, AmmoShotEvent args)
    {
        if (args.FiredProjectiles.Count == 0)
            return;

        // Get the shooter entity
        EntityUid? shooter = null;
        if (TryComp<ProjectileComponent>(args.FiredProjectiles[0], out var projectileComp))
        {
            shooter = projectileComp.Shooter;
        }

        // We need to get the target coordinates from the gun component
        var targetCoords = component.ShootCoordinates;
        if (!targetCoords.HasValue || !targetCoords.Value.IsValid(EntityManager))
            return;

        // Find the controlling console for position updates if this is a fire controllable
        EntityUid? controllingConsole = null;
        if (TryComp<FireControllableComponent>(uid, out var fireControllable) &&
            fireControllable.ControllingServer != null)
        {
            // Find the active console that fired this
            var query = EntityQueryEnumerator<FireControlConsoleComponent>();
            while (query.MoveNext(out var consoleUid, out var console))
            {
                if (console.ConnectedServer == fireControllable.ControllingServer)
                {
                    controllingConsole = consoleUid;

                    // Store initial cursor position if we're seeing it for the first time
                    if (!_consoleMousePositions.ContainsKey(consoleUid))
                    {
                        _consoleMousePositions[consoleUid] = targetCoords.Value;
                    }

                    break;
                }
            }
        }

        foreach (var projectileUid in args.FiredProjectiles)
        {
            if (!TryComp<TargetGuidedComponent>(projectileUid, out var guidedComp))
                continue;

            // If firing ship is in FTL, missile won't have guidance
            if (shooter.HasValue && Transform(shooter.Value).GridUid is { } shipGrid)
            {
                if (TryComp<FTLComponent>(shipGrid, out _))
                {
                    // Skip guidance setup if ship is in FTL
                    continue;
                }
            }

            // Set up initial target for guided missile
            guidedComp.TargetPosition = targetCoords.Value;

            // Add to our tracking list for cursor position updates
            _activeMissiles.Add(projectileUid);

            // Record the console this was fired from for position updates
            if (controllingConsole.HasValue)
            {
                guidedComp.ControllingConsole = controllingConsole;
            }
        }
    }

    /// <summary>
    /// Cleanup guided missiles when they're destroyed
    /// </summary>
    private void OnGuidedMissileShutdown(EntityUid uid, TargetGuidedComponent component, ComponentShutdown args)
    {
        _activeMissiles.Remove(uid);
    }

    /// <summary>
    /// Updates the cursor position for any tracking missiles from a given console
    /// </summary>
    public void OnGuidanceUpdate(EntityUid consoleUid, EntityCoordinates targetCoordinates)
    {
        // Store the updated position for this console
        _consoleMousePositions[consoleUid] = targetCoordinates;

        // Update any active missiles being controlled by this console
        foreach (var missile in _activeMissiles)
        {
            if (!TryComp<TargetGuidedComponent>(missile, out var guidedComp))
                continue;

            if (guidedComp.ControllingConsole != consoleUid)
                continue;

            // Don't update position if the missile's ship is in FTL
            if (TryComp<ProjectileComponent>(missile, out var projectileComp) &&
                projectileComp.Shooter.HasValue &&
                Transform(projectileComp.Shooter.Value).GridUid is { } shipGrid &&
                TryComp<FTLComponent>(shipGrid, out _))
            {
                continue;
            }

            guidedComp.TargetPosition = targetCoordinates;
        }
    }

    /// <summary>
    /// Helper method to get the current position of a specific console
    /// </summary>
    public EntityCoordinates? GetConsolePosition(EntityUid consoleUid)
    {
        if (_consoleMousePositions.TryGetValue(consoleUid, out var coords))
            return coords;

        return null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Update target positions for active missiles based on the current cursor position
        foreach (var missileUid in _activeMissiles.ToArray())
        {
            if (!TryComp<TargetGuidedComponent>(missileUid, out var guidedComp) ||
                !guidedComp.ControllingConsole.HasValue)
                continue;

            // Get the controlling console
            var consoleUid = guidedComp.ControllingConsole.Value;
            if (!_consoleMousePositions.TryGetValue(consoleUid, out var mousePosition))
                continue;

            // Don't update position if the missile's ship is in FTL
            if (TryComp<ProjectileComponent>(missileUid, out var projectileComp) &&
                projectileComp.Shooter.HasValue &&
                Transform(projectileComp.Shooter.Value).GridUid is { } shipGrid &&
                TryComp<FTLComponent>(shipGrid, out _))
            {
                continue;
            }

            // Update the missile's target to the console's current mouse position
            _targetGuided.SetTargetPosition(missileUid, mousePosition);
        }

        // Clean up any console positions for consoles that no longer exist or have no active missiles
        CleanupConsolePositions();
    }

    /// <summary>
    /// Remove any console positions that no longer have active missiles
    /// </summary>
    private void CleanupConsolePositions()
    {
        // Get all consoles that are actually controlling missiles
        var activeConsoles = new HashSet<EntityUid>();
        foreach (var missileUid in _activeMissiles)
        {
            if (TryComp<TargetGuidedComponent>(missileUid, out var guidedComp) &&
                guidedComp.ControllingConsole.HasValue)
            {
                activeConsoles.Add(guidedComp.ControllingConsole.Value);
            }
        }

        // Remove positions for consoles without any missiles
        foreach (var consoleUid in _consoleMousePositions.Keys.ToList())
        {
            if (!activeConsoles.Contains(consoleUid) || !EntityManager.EntityExists(consoleUid))
            {
                _consoleMousePositions.Remove(consoleUid);
            }
        }
    }
}
