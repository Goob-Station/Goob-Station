using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CosmicFinaleComponent : Component
{
    [DataField]
    public FinaleState CurrentState = FinaleState.Unavailable;

    [DataField]
    public bool FinaleDelayStarted = false;

    [DataField]
    public bool FinaleActive = false;

    [DataField]
    public bool Occupied = false;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan FinaleTimer = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan BufferTimer = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CultistsCheckTimer = default!;

    [DataField, AutoNetworkedField]
    public TimeSpan BufferRemainingTime = TimeSpan.FromSeconds(300);

    [DataField, AutoNetworkedField]
    public TimeSpan FinaleRemainingTime = TimeSpan.FromSeconds(126);

    [DataField, AutoNetworkedField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier CancelEventSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");

    [DataField]
    public TimeSpan FinaleSongLength;

    [DataField]
    public TimeSpan SongLength;

    [DataField]
    public SoundSpecifier? SelectedSong;

    [DataField]
    public TimeSpan InteractionTime = TimeSpan.FromSeconds(8);

    [DataField]
    public SoundSpecifier BufferMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/premonition.ogg");

    [DataField]
    public SoundSpecifier FinaleMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/a_new_dawn.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? SongTimer;

    /// <summary>
    /// The degen that people suffer if they don't have mindshields, aren't a chaplain, or aren't cultists while the Finale is Available or Active. This feature is currently disabled.
    /// </summary>
    [DataField]
    public DamageSpecifier FinaleDegen = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2.25},
            { "Cold", 2.25},
            { "Radiation", 2.25},
            { "Asphyxiation", 2.25}
        }
    };
}

[Serializable]
public enum FinaleState : byte
{
    Unavailable,
    ReadyBuffer,
    ReadyFinale,
    ActiveBuffer,
    ActiveFinale,
    Victory,
}
