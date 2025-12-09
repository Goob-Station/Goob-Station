using Content.Server.Administration;
using Content.Server.Administration.Commands;
using Content.Server.Administration.Notes;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Database;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Administration.Notes;

[AdminCommand(AdminFlags.EditNotes)]
public sealed class CreateAdminNoteCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotes = default!;

    public string Command => "createadminnote";
    public string Description => Loc.GetString("cmd-create-admin-note-command-desc");
    public string Help => Loc.GetString("cmd-create-admin-note-command-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is null)
        {
            shell.WriteError(Loc.GetString("cmd-create-admin-note-command-error-not-player"));
            return;
        }

        if (args.Length < 4)
        {
            shell.WriteError(Loc.GetString("cmd-create-admin-note-command-error-args"));
            return;
        }

        var playerRecord = await _db.GetPlayerRecordByUserName(args[0]);

        if (playerRecord is null)
        {
            shell.WriteError(Loc.GetString("cmd-create-admin-note-command-error-player-not-found"));
            return;
        }

        if (!Enum.TryParse(args[2], out NoteType noteType))
        {
            shell.WriteError(Loc.GetString("cmd-create-admin-note-command-error-wrong-note-type"));
            return;
        }

        if (!Enum.TryParse(args[3], out NoteSeverity severity))
        {
            shell.WriteError(Loc.GetString("cmd-create-admin-note-command-error-wrong-severity"));
            return;
        }

        DateTime? time = null;

        if (args.Length > 4)
        {
            var minutes = PlayTimeCommandUtilities.CountMinutes(args[4]);
            time = DateTime.Now + TimeSpan.FromMinutes(minutes);
        }

        var secret = true;

        if (args.Length > 5)
            secret = bool.Parse(args[5]);

        await _adminNotes.AddAdminRemark(shell.Player, playerRecord.UserId, noteType, args[1], severity, secret, time);

        shell.WriteError(Loc.GetString("cmd-create-admin-note-command-succeed"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("cmd-create-admin-note-command-arg-user"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-create-admin-note-command-arg-message"));
        }

        if (args.Length == 3)
        {
            return CompletionResult.FromHintOptions(
                Enum.GetNames<NoteType>(),
                Loc.GetString("cmd-create-admin-note-command-arg-note-type"));
        }

        if (args.Length == 4)
        {
            return CompletionResult.FromHintOptions(
                Enum.GetNames<NoteSeverity>(),
                Loc.GetString("cmd-create-admin-note-command-arg-note-severity"));
        }

        if (args.Length == 5)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-create-admin-note-command-arg-duration"));
        }

        if (args.Length == 6)
        {
            return CompletionResult.FromHintOptions(
                ["true", "false"],
                Loc.GetString("cmd-create-admin-note-command-arg-note-secret"));
        }

        return CompletionResult.Empty;
    }
}
