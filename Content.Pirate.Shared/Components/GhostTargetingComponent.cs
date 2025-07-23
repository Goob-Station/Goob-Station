using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System.Collections.Generic;

namespace Content.Pirate.Shared.Components
{
    [RegisterComponent]
    public sealed partial class GhostTargetingComponent : Component
    {
        // Додаткові компоненти
        public bool HadFTLSmashImmune { get; set; } = false;
        public bool HadUniversalLanguageSpeaker { get; set; } = false;
        public bool HadExaminer { get; set; } = false;
        public bool HadSpeechDead { get; set; } = false;
        public string? OldSpeechVerb { get; set; }
        public List<int>? OldFixtureLayers { get; set; }

        // --- Основна інформація ---
        public static string Name => "GhostTargeting";
        [ViewVariables]
        public bool IsGhost { get; set; } = false;
        [ViewVariables]
        public NetEntity Target { get; set; } = NetEntity.Invalid;

        [ViewVariables]
        public List<string> GhostLayers { get; set; } = new();

        [DataField]
        public List<string> BaseGhostActions { get; set; } = new()
        {
            "ActionTargetGhost",
            "ActionClearTargetGhost",
            "ActionToggleGhostForm",
            "ActionGhostBlink"
        };
        [DataField]
        public List<EntityUid>? ActionEntities;

        // --- Збереження стану для фізичного перетворення ---

        [ViewVariables]
        public int? OldVisibilityMask { get; set; }

        [ViewVariables]
        public bool? OldDrawFov { get; set; }
        [ViewVariables]
        public bool? OldCanCollide { get; set; }
        [ViewVariables]
        public Robust.Shared.Physics.BodyType? OldBodyType { get; set; }
        [ViewVariables]
        public bool HadPhysics { get; set; } = false;
        [ViewVariables]
        public bool HadContentEye { get; set; } = false;
        [ViewVariables]
        public bool HadMovementIgnoreGravity { get; set; } = false;
        [ViewVariables]
        public bool HadCanMoveInAir { get; set; } = false;
        [ViewVariables]
        public int? OldVisibilityLayers { get; set; }

        [ViewVariables]
        public EntityUid? ToggleGhostFormActionUid { get; set; }

        // --- Збереження імунітетів до перетворення ---
        public bool HadZombieImmune { get; set; } = false;
        public bool HadBreathingImmunity { get; set; } = false;
        public bool HadPressureImmunity { get; set; } = false;
        public bool HadActiveListener { get; set; } = false;
        public bool HadWeakToHoly { get; set; } = false;
        public bool HadCrematoriumImmune { get; set; } = false;
    }
}
