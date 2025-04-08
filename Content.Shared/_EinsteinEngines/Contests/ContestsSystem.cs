using Content.Goobstation.Common.CCVar; // Goob Edit
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;

namespace Content.Shared._EinsteinEngines.Contests // Goob Edit
{
    public sealed partial class ContestsSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        /// <summary>
        ///     The presumed average mass of a player entity
        ///     Defaulted to the average mass of an adult human
        /// </summary>
        private const float AverageMass = 71f;

        #region Mass Contests
        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        /// </summary>
        /// <param name="performerUid">Uid of Performer</param>
        public float MassContest(EntityUid performerUid, float otherMass = AverageMass)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && performerPhysics.Mass != 0)
                return Math.Clamp(performerPhysics.Mass / otherMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage));// Goob edit

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, float)"/>
        /// <remarks>
        ///     MaybeMassContest, in case your entity doesn't exist
        /// </remarks>
        public float MassContest(EntityUid? performerUid, float otherMass = AverageMass)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests)) // Goob edit
            {
                var ratio = performerUid is { } uid ? MassContest(uid, otherMass) : 1f;
                return ratio;
            }

            return 1f;
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        ///     If a function already has the performer's physics component, this is faster
        /// </summary>
        /// <param name="performerPhysics"></param>
        public float MassContest(PhysicsComponent performerPhysics, float otherMass = AverageMass)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && performerPhysics.Mass != 0)
                return Math.Clamp(performerPhysics.Mass / otherMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage));

            return 1f;
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and a target, accepts either EntityUids or PhysicsComponents in any combination
        ///     If you have physics components already in your function, use <see cref="MassContest(PhysicsComponent, float)" /> instead
        /// </summary>
        /// <param name="performerUid"></param>
        /// <param name="targetUid"></param>
        public float MassContest(EntityUid performerUid, EntityUid targetUid)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage)); // Goob edit

            return 1f; // Goob edit
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(EntityUid performerUid, PhysicsComponent targetPhysics)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage));

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(PhysicsComponent performerPhysics, EntityUid targetUid)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage)); // Goob edit

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(PhysicsComponent performerPhysics, PhysicsComponent targetPhysics)
        {
            if (_cfg.GetCVar(GoobCVars.DoMassContests) // Goob edit
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage), 1 + _cfg.GetCVar(GoobCVars.MassContestsMaxPercentage)); // Goob edit

            return 1f;
        }

        #endregion
    }
}
