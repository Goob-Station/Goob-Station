// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Item;
using Content.Shared.Mobs; // DOWNSTREAM-TPirates: ghost follow menu update
using Content.Shared.Popups;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost
{
    /// <summary>
    /// System for the <see cref="GhostComponent"/>.
    /// Prevents ghosts from interacting when <see cref="GhostComponent.CanGhostInteract"/> is false.
    /// </summary>
    public abstract class SharedGhostSystem : EntitySystem
    {
        [Dependency] protected readonly SharedPopupSystem Popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GhostComponent, UseAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, InteractionAttemptEvent>(OnAttemptInteract);
            SubscribeLocalEvent<GhostComponent, EmoteAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, DropAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, PickupAttemptEvent>(OnAttempt);
            // EE Interaction Verb Begin
            SubscribeLocalEvent<GhostComponent, InteractionVerbAttemptEvent>(OnAttempt);
            // End
        }

        private void OnAttemptInteract(Entity<GhostComponent> ent, ref InteractionAttemptEvent args)
        {
            if (!ent.Comp.CanGhostInteract)
                args.Cancelled = true;
        }

        private void OnAttempt(EntityUid uid, GhostComponent component, CancellableEntityEventArgs args)
        {
            if (!component.CanGhostInteract)
                args.Cancel();
        }

        /// <summary>
        /// Sets the ghost's time of death.
        /// </summary>
        public void SetTimeOfDeath(Entity<GhostComponent?> entity, TimeSpan value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.TimeOfDeath == value)
                return;

            entity.Comp.TimeOfDeath = value;
            Dirty(entity);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetTimeOfDeath(EntityUid uid, TimeSpan value, GhostComponent? component)
        {
            SetTimeOfDeath((uid, component), value);
        }

        /// <summary>
        /// Sets whether or not the ghost player is allowed to return to their original body.
        /// </summary>
        public void SetCanReturnToBody(Entity<GhostComponent?> entity, bool value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.CanReturnToBody == value)
                return;

            entity.Comp.CanReturnToBody = value;
            Dirty(entity);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetCanReturnToBody(EntityUid uid, bool value, GhostComponent? component = null)
        {
            SetCanReturnToBody((uid, component), value);
        }

        [Obsolete("Use the Entity<GhostComponent?> overload")]
        public void SetCanReturnToBody(GhostComponent component, bool value)
        {
            SetCanReturnToBody((component.Owner, component), value);
        }


        /// <summary>
        /// Sets whether the ghost is allowed to interact with other entities.
        /// </summary>
        public void SetCanGhostInteract(Entity<GhostComponent?> entity, bool value)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            if (entity.Comp.CanGhostInteract == value)
                return;

            entity.Comp.CanGhostInteract = value;
            Dirty(entity);
        }
    }

    /// <summary>
    /// A client to server request to get places a ghost can warp to.
    /// Response is sent via <see cref="GhostWarpsResponseEvent"/>
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsRequestEvent : EntityEventArgs
    {
    }

    #region DOWNSTREAM-TPirates: ghost follow menu update
    [Serializable, NetSerializable]
    public enum GhostWarpType : byte
    {
        Location,
        Player,
        Dead,
        Ghost,
        Mob
    }
    #endregion

    #region DOWNSTREAM-TPirates: ghost follow menu update
    /// <summary>
    /// Display-only health state for ghost warp chip color (Healthy=green, Wounded=orange, Critical=red, Dead=grey).
    /// </summary>
    [Serializable, NetSerializable]
    public enum GhostWarpHealthState : byte
    {
        Unknown = 0,
        Healthy = 1,
        Wounded = 2,
        Critical = 3,
        Dead = 4
    }
    #endregion

    /// <summary>
    /// An individual place a ghost can warp to.
    /// This is used as part of <see cref="GhostWarpsResponseEvent"/>
    /// </summary>
    [Serializable, NetSerializable]
    public struct GhostWarp
    {
        public GhostWarp(NetEntity entity, string displayName, bool isWarpPoint) // DOWNSTREAM-TPirates: ghost follow menu update
            : this(entity, displayName, isWarpPoint ? GhostWarpType.Location : GhostWarpType.Player, 0) // DOWNSTREAM-TPirates: ghost follow menu update
        {
        }

        public GhostWarp(NetEntity entity, string displayName, GhostWarpType type, int observerCount = 0) // DOWNSTREAM-TPirates: ghost follow menu update
            : this(entity, displayName, type, observerCount, string.Empty, MobState.Invalid, string.Empty, GhostWarpHealthState.Unknown) // DOWNSTREAM-TPirates: ghost follow menu update
        {
        }

        #region DOWNSTREAM-TPirates: ghost follow menu update
        public GhostWarp(NetEntity entity, string displayName, GhostWarpType type, int observerCount, string? jobIconId, MobState mobState)
            : this(entity, displayName, type, observerCount, jobIconId, mobState, string.Empty, GhostWarpHealthState.Unknown)
        {
        }

        public GhostWarp(NetEntity entity, string displayName, GhostWarpType type, int observerCount, string? jobIconId, MobState mobState, string? professionTitle)
            : this(entity, displayName, type, observerCount, jobIconId, mobState, professionTitle, GhostWarpHealthState.Unknown)
        {
        }

        public GhostWarp(NetEntity entity, string displayName, GhostWarpType type, int observerCount, string? jobIconId, MobState mobState, string? professionTitle, GhostWarpHealthState healthState, string? departmentId = null)
        {
            Entity = entity;
            DisplayName = displayName;
            Type = type;
            ObserverCount = observerCount;
            JobIconId = jobIconId ?? string.Empty;
            MobState = mobState;
            ProfessionTitle = professionTitle ?? string.Empty;
            HealthState = healthState;
            DepartmentId = departmentId ?? string.Empty;
        }

        /// <summary>
        /// The entity representing the warp point.
        /// This is passed back to the server in <see cref="GhostWarpToTargetRequestEvent"/>
        /// </summary>
        public NetEntity Entity { get; }

        /// <summary>
        /// The display name to be surfaced in the ghost warps menu
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Whether this warp represents a warp point or a player
        /// </summary>
        public bool IsWarpPoint => Type == GhostWarpType.Location;

        public GhostWarpType Type { get; }

        /// <summary>
        /// Number of non-admin ghosts currently following this entity.
        /// </summary>
        public int ObserverCount { get; }

        /// <summary>
        /// Job icon prototype id for profession display (e.g. in ghost warp menu). Empty for locations/none.
        /// </summary>
        public string JobIconId { get; }

        /// <summary>
        /// Mob state for health-based chip color (Alive=green, Critical=red, Dead/Invalid=grey).
        /// </summary>
        public MobState MobState { get; }

        /// <summary>
        /// Profession/job title for tooltip on the job icon. Empty for locations/none.
        /// </summary>
        public string ProfessionTitle { get; }

        /// <summary>
        /// Display health state for chip color (Healthy=green, Wounded=orange, Critical=red, Dead=grey).
        /// </summary>
        public GhostWarpHealthState HealthState { get; }

        /// <summary>
        /// Department prototype ID for department-based chip color. Empty when unknown.
        /// When set, client uses <see cref="Content.Shared.Roles.DepartmentPrototype.Color"/>; language-independent.
        /// </summary>
        public string DepartmentId { get; }
        #endregion
    }

    /// <summary>
    /// A server to client response for a <see cref="GhostWarpsRequestEvent"/>.
    /// Contains players, and locations a ghost can warp to
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsResponseEvent : EntityEventArgs
    {
        public GhostWarpsResponseEvent(List<GhostWarp> warps)
        {
            Warps = warps;
        }

        /// <summary>
        /// A list of warp points.
        /// </summary>
        public List<GhostWarp> Warps { get; }
    }

    /// <summary>
    ///  A client to server request for their ghost to be warped to an entity
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpToTargetRequestEvent : EntityEventArgs
    {
        public NetEntity Target { get; }

        public GhostWarpToTargetRequestEvent(NetEntity target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// A client to server request for their ghost to be warped to the most followed entity.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostnadoRequestEvent : EntityEventArgs;

    #region DOWNSTREAM-TPirates: ghost follow menu update
    /// <summary>
    /// Server to client: observer count for a warp target entity has changed.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpObserverCountChangedEvent : EntityEventArgs
    {
        public NetEntity Entity { get; }
        public int ObserverCount { get; }

        public GhostWarpObserverCountChangedEvent(NetEntity entity, int observerCount)
        {
            Entity = entity;
            ObserverCount = observerCount;
        }
    }
    #endregion

    /// <summary>
    /// A client to server request for their ghost to return to body
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostReturnToBodyRequest : EntityEventArgs
    {
    }

    /// <summary>
    /// A server to client update with the available ghost role count
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostUpdateGhostRoleCountEvent : EntityEventArgs
    {
        public int AvailableGhostRoles { get; }

        public GhostUpdateGhostRoleCountEvent(int availableGhostRoleCount)
        {
            AvailableGhostRoles = availableGhostRoleCount;
        }
    }
}
