// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Explosion.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Explosion;
using Content.Shared.Nuke;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Server.Nuke
{
    [RegisterComponent]
    [Access(typeof(NukeSystem))]
    public sealed partial class NukeComponent : SharedNukeComponent
    {
        [DataField]
        public int Timer = 300;

        [DataField]
        public int MinimumTime = 180;

        [DataField]
        public int Cooldown = 30;

        [DataField("diskSlot")]
        public ItemSlot DiskSlot = new();

        [DataField("alertTime")]
        public float AlertSoundTime = 10.0f;

        [DataField("disarmDoafterLength")]
        public float DisarmDoAfterLength = 30.0f;

        // Goobstation start - nuke ops siege rework
        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Converted;

        [DataField("convertedTimer")]
        public int ConvertedTimer = 720;

        [DataField("diskTimerReduction")]
        public int DiskTimerReduction = 300;

        [DataField("diskReductionMinTime")]
        public int DiskReductionMinTime = 60;

        public EntityUid? LastDiskInserter;

        [DataField("conversionDoafterLength")]
        public float ConversionDoAfterLength = 10f;

        [DataField]
        public EntProtoId WreckPrototype = "NuclearBombWreck";
        // Goobstation end

        [DataField("alertLevelOnActivate")] public string AlertLevelOnActivate = default!;
        [DataField("alertLevelOnOverride")] public string AlertLevelOnOverride = default!; // Goobstation
        [DataField("alertLevelOnDeactivate")] public string AlertLevelOnDeactivate = default!;

        public int LastPlayedKeypadSemitones = 0;

        [DataField("keypadPressSound")]
        public SoundSpecifier KeypadPressSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

        [DataField("accessGrantedSound")]
        public SoundSpecifier AccessGrantedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/confirm_beep.ogg");

        [DataField("accessDeniedSound")]
        public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

        [DataField("alertSound")]
        public SoundSpecifier AlertSound = new SoundPathSpecifier("/Audio/Machines/Nuke/nuke_alarm.ogg");

        [DataField("armSound")]
        public SoundSpecifier ArmSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");

        [DataField("disarmSound")]
        public SoundSpecifier DisarmSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");

        [DataField("armMusic")]
        public SoundSpecifier ArmMusic = new SoundCollectionSpecifier("NukeMusic");

        #region ExplosiveComponent
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("explosionType", required: true)]
        public ProtoId<ExplosionPrototype> ExplosionType = default!;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("maxIntensity")]
        public float MaxIntensity = 100;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("intensitySlope")]
        public float IntensitySlope = 5;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("totalIntensity")]
        public float TotalIntensity = 100000;

        public bool Exploded;
        #endregion

        public EntityUid? OriginStation;

        public (MapId, EntityUid?)? OriginMapGrid;

        [DataField] public int CodeLength = 6;
        [DataField] public string Code = string.Empty;

        [DataField]
        public float RemainingTime;

        [DataField]
        public float CooldownTime;

        [DataField]
        public string EnteredCode = "";

        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan LastCodeEnteredAt = TimeSpan.Zero;

        [DataField]
        public NukeStatus Status = NukeStatus.AWAIT_DISK;

        public bool PlayedNukeSong = false;

        public bool PlayedAlertSound = false;

        public EntityUid? AlertAudioStream = default;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("requiredFloorRadius")]
        public float RequiredFloorRadius = 5;

        // Goobstation start
        [DataField("honkopsArmMusic")]
        public SoundSpecifier HonkopsArmMusic = new SoundCollectionSpecifier("HonkopsNukeMusic");

        [DataField("loneopArmMusic")]
        public SoundSpecifier LoneopArmMusic = new SoundCollectionSpecifier("LoneopNukeMusic");
        // Goobstation end
    }
}

