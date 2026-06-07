using Content.Shared.Stealth.Components;

namespace Content.Server.Pointing.EntitySystems
{
    internal sealed partial class PointingSystem
    {
        /// <summary>
        /// Checks if a player can point at a stealthed entity.
        /// True if the entity can be pointed at, false if it's stealthed
        /// </summary>
        private bool CanPointAtStealthedEntity(EntityUid player, EntityUid pointed)
        {
            // Disallow pointing at stealthed entities
            if (pointed != player && TryComp<StealthComponent>(pointed, out var stealth) && stealth.Enabled)
            {
                return false;
            }

            return true;
        }
    }
}
