using Robust.Shared.Audio;

namespace Content.Server._CorvaxGoob.RoundEnd.PhotoAlbum;

[RegisterComponent]
public sealed partial class PhotoAlbumComponent : Component
{
    [DataField]
    public string ContainerId { get; set; } = "storagebase";

    /// <summary>
    /// Используется при отправке информации альбома в манифест.
    /// </summary>
    [DataField]
    public EntityUid? SignerUid = null;

    /// <summary>
    /// Используется при отправке информации альбома в манифест.
    /// </summary>
    [DataField]
    public string? SignerUsername = null;

    [DataField]
    public bool IsSigned = false;

    [DataField]
    public SoundSpecifier? SignSound { get; private set; } = new SoundCollectionSpecifier("PaperScribbles", AudioParams.Default.WithVariation(0.1f));
}
