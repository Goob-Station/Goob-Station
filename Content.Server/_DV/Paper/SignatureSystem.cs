<<<<<<< HEAD
using Content.Shared.Access.Systems;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Shared._DV.Paper;

public sealed class SignatureSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tags = default!;

    // The sprite used to visualize "signatures" on paper entities.
    private const string SignatureStampState = "paper_stamp-signature";

    public override void Initialize()
    {
        SubscribeLocalEvent<PaperComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
    }

    private void OnGetAltVerbs(Entity<PaperComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

<<<<<<< HEAD
        if (args.Using is not {} pen || !_tag.HasTag(pen, "Write"))
=======
        if (args.Using is not {} pen || !_tags.HasTag(pen, "Write"))
>>>>>>> 38c6f918769a01f7fc55d94840f7b239992b85fe
            return;

        var user = args.User;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TrySignPaper(ent, user, pen);
            },
            Text = Loc.GetString("paper-sign-verb"),
            DoContactInteraction = true,
            Priority = 10
        };
        args.Verbs.Add(verb);
    }

    /// <summary>
    ///     Tries add add a signature to the paper with signer's name.
    /// </summary>
    public bool TrySignPaper(Entity<PaperComponent> paper, EntityUid signer, EntityUid pen)
    {
        var comp = paper.Comp;

        var ev = new SignAttemptEvent(paper, signer);
        RaiseLocalEvent(pen, ref ev);
        if (ev.Cancelled)
            return false;

        var signatureName = DetermineEntitySignature(signer);

        var stampInfo = new StampDisplayInfo()
        {
            StampedName = signatureName,
<<<<<<< HEAD
            StampedColor = Color.DarkSlateGray, // TODO: make configurable? Perhaps it should depend on the pen.
        };

        // TODO: remove redunant contains check when TryStamp isnt a meme
        if (comp.StampedBy.Contains(stampInfo) || !_paper.TryStamp(paper, stampInfo, SignatureStampState))
=======
            StampedColor = Color.DarkSlateGray, //TODO Make this configurable depending on the pen.
        };

        if (!comp.StampedBy.Contains(stampInfo) && _paper.TryStamp(paper, stampInfo, SignatureStampState))
        {
            // Show popups and play a paper writing sound
            var signedOtherMessage = Loc.GetString("paper-signed-other", ("user", signer), ("target", paper.Owner));
            _popup.PopupEntity(signedOtherMessage, signer, Filter.PvsExcept(signer, entityManager: EntityManager), true);

            var signedSelfMessage = Loc.GetString("paper-signed-self", ("target", paper.Owner));
            _popup.PopupEntity(signedSelfMessage, signer, signer);

            _audio.PlayPvs(comp.Sound, signer);

            _paper.UpdateUserInterface(paper);

            return true;
        }
        else
>>>>>>> 38c6f918769a01f7fc55d94840f7b239992b85fe
        {
            // Show an error popup
            _popup.PopupEntity(Loc.GetString("paper-signed-failure", ("target", paper.Owner)), signer, signer, PopupType.SmallCaution);

            return false;
        }
<<<<<<< HEAD

        // Show popups and play a paper writing sound
        var signedOtherMessage = Loc.GetString("paper-signed-other", ("user", signer), ("target", paper.Owner));
        _popup.PopupEntity(signedOtherMessage, signer, Filter.PvsExcept(signer, entityManager: EntityManager), true);

        var signedSelfMessage = Loc.GetString("paper-signed-self", ("target", paper.Owner));
        _popup.PopupClient(signedSelfMessage, signer, signer);

        _audio.PlayPredicted(comp.Sound, signer, signer);

        return true;
=======
>>>>>>> 38c6f918769a01f7fc55d94840f7b239992b85fe
    }

    private string DetermineEntitySignature(EntityUid uid)
    {
        // If the entity has an ID, use the name on it.
        if (_idCard.TryFindIdCard(uid, out var id) && !string.IsNullOrWhiteSpace(id.Comp.FullName))
<<<<<<< HEAD
        {
            return id.Comp.FullName;
        }
=======
            return id.Comp.FullName;
>>>>>>> 38c6f918769a01f7fc55d94840f7b239992b85fe

        // Alternatively, return the entity name
        return Name(uid);
    }
}
