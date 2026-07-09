// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.CriminalRecords.Components;
using Content.Shared.CriminalRecords.Systems;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.IdentityManagement;
using Content.Shared.Security.Components;
using System.Linq;
#region Pirate: records photos
using Content.Shared.Customization.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared._Pirate.Contractors.Prototypes;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Hands.Systems;
using Content.Shared.Hands.Components;
using Content.Server._Pirate.Photo;
#endregion

namespace Content.Server.CriminalRecords.Systems;

/// <summary>
/// Handles all UI for criminal records console
/// </summary>
public sealed partial class CriminalRecordsConsoleSystem : SharedCriminalRecordsConsoleSystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly CriminalRecordsSystem _criminalRecords = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    #region Pirate: records photos
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PhotoSystem _photoSystem = default!;
    private readonly HashSet<EntityUid> _activePortraitPrintJobs = new();
    private static readonly SoundSpecifier PortraitPrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");
    private static readonly SoundSpecifier PortraitUploadSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_prompt_confirm.ogg");
    private static readonly TimeSpan PortraitPrintDelay = TimeSpan.FromSeconds(2.3f);
    private static readonly IReadOnlyDictionary<string, TimeSpan> EmptyPlayTimes = new Dictionary<string, TimeSpan>();
    #endregion

    public override void Initialize()
    {
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);
        #region Pirate: records photos
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, ComponentShutdown>(OnConsoleShutdown);
        SubscribeLocalEvent<CriminalRecordsConsoleComponent, RecordRemovedEvent>(OnRecordRemoved);
        #endregion

        Subs.BuiEvents<CriminalRecordsConsoleComponent>(CriminalRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
            subs.Event<SetStationRecordFilter>(OnFiltersChanged);
            subs.Event<CriminalRecordChangeStatus>(OnChangeStatus);
            subs.Event<CriminalRecordAddHistory>(OnAddHistory);
            subs.Event<CriminalRecordDeleteHistory>(OnDeleteHistory);
            subs.Event<CriminalRecordSetStatusFilter>(OnStatusFilterPressed);
            #region Pirate: records photos
            subs.Event<CriminalRecordCreateRecord>(OnCreateRecord);
            subs.Event<DeleteStationRecord>(OnDeleteRecord);
            subs.Event<CriminalRecordEditIdentity>(OnEditIdentity);
            subs.Event<CriminalRecordEditForensics>(OnEditForensics);
            subs.Event<CriminalRecordPrintPhoto>(OnPrintPhoto);
            subs.Event<CriminalRecordUploadPhoto>(OnUploadPhoto);
            subs.Event<CriminalRecordStoreGeneratedPhoto>(OnStoreGeneratedPhoto);
            #endregion
        });

        Subs.BuiEvents<IdExaminableComponent>(SetWantedVerbMenu.Key, subs => // Goobstation-WantedMenu
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<CriminalRecordChangeStatus>(OnChangeStatus);
        });
    }

    private void UpdateUserInterface<T>(Entity<CriminalRecordsConsoleComponent> ent, ref T args)
    {
        // TODO: this is probably wasteful, maybe better to send a message to modify the exact state?
        UpdateUserInterface(ent);
    }

    private void OnKeySelected(Entity<CriminalRecordsConsoleComponent> ent, ref SelectStationRecord msg)
    {
        // no concern of sus client since record retrieval will fail if invalid id is given
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }
    private void OnStatusFilterPressed(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordSetStatusFilter msg)
    {
        ent.Comp.FilterStatus = msg.FilterStatus;
        UpdateUserInterface(ent);
    }

    private void OnFiltersChanged(Entity<CriminalRecordsConsoleComponent> ent, ref SetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    #region Pirate: records photos
    private void OnRecordRemoved(Entity<CriminalRecordsConsoleComponent> ent, ref RecordRemovedEvent args)
    {
        if (args.Key.OriginStation != _station.GetOwningStation(ent))
            return;

        if (ent.Comp.ActiveKey == args.Key.Id)
            ent.Comp.ActiveKey = null;

        UpdateUserInterface(ent);
    }

    private void OnConsoleShutdown(Entity<CriminalRecordsConsoleComponent> ent, ref ComponentShutdown args) // Pirate: records photos
    {
        _activePortraitPrintJobs.Remove(ent);
    }

    private void OnCreateRecord(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordCreateRecord msg)
    {
        if (!_access.IsAllowed(msg.Actor, ent))
            return;

        if (_station.GetOwningStation(ent) is not { } station ||
            !TryComp<StationRecordsComponent>(station, out var stationRecords))
        {
            return;
        }

        var name = msg.Name.Trim();
        if (name.Length > ent.Comp.MaxStringLength)
            name = name[..(int) ent.Comp.MaxStringLength];

        if (string.IsNullOrWhiteSpace(name))
            return;

        var criminalMatches = GetCriminalRecordIdsByName(station, name, stationRecords);
        if (criminalMatches.Count > 1)
            return;

        if (criminalMatches.Count == 1)
        {
            ent.Comp.ActiveKey = criminalMatches[0];
            UpdateUserInterface(ent);
            return;
        }

        var matchingIds = _records.GetRecordIdsByName(station, name, stationRecords);
        if (matchingIds.Count > 1)
            return;

        if (matchingIds.Count == 1)
        {
            var existingKey = new StationRecordKey(matchingIds[0], station);
            if (!_records.TryGetRecord<GeneralStationRecord>(existingKey, out var generalRecord))
                return;

            _records.AddRecordEntry(existingKey, CreateCriminalRecordFromGeneral(generalRecord));
            ent.Comp.ActiveKey = existingKey.Id;
            _records.Synchronize(existingKey);
            UpdateUserInterface(ent);
            return;
        }

        var profile = HumanoidCharacterProfile.DefaultWithSpecies(SharedHumanoidAppearanceSystem.DefaultSpecies)
            .WithName(name)
            .WithAge(18);

        var record = new GeneralStationRecord
        {
            Name = name,
            Age = 18,
            JobTitle = Loc.GetString("suit-sensor-component-unknown-job"),
            JobIcon = string.Empty,
            JobPrototype = string.Empty,
            Nationality = string.Empty,
            Employer = string.Empty,
            Species = string.Empty,
            Gender = Gender.Male,
            DisplayPriority = 0,
            Fingerprint = null,
            DNA = null,
        };

        var key = _records.AddRecordEntry(station, record, stationRecords);
        if (!key.IsValid())
            return;

        ent.Comp.ActiveKey = key.Id;
        RaiseLocalEvent(new AfterGeneralRecordCreatedEvent(key, record, profile));
        UpdateUserInterface(ent);
    }

    private void OnDeleteRecord(Entity<CriminalRecordsConsoleComponent> ent, ref DeleteStationRecord msg)
    {
        if (!_access.IsAllowed(msg.Actor, ent))
            return;

        if (_station.GetOwningStation(ent) is not { } station)
            return;

        var key = new StationRecordKey(msg.Id, station);
        if (!_records.TryGetRecord<CriminalRecord>(key, out var criminalRecord)
            || !_records.RemoveRecordEntry<CriminalRecord>(key))
        {
            return;
        }

        var name = _records.TryGetRecord<GeneralStationRecord>(key, out var stationRecord)
            ? stationRecord.Name
            : criminalRecord.GeneralRecordSnapshot?.Name ?? string.Empty;

        _criminalRecords.NotifyCriminalRecordDeleted(name);

        if (ent.Comp.ActiveKey == msg.Id)
            ent.Comp.ActiveKey = null;

        UpdateUserInterface(ent);
    }
    #endregion

    private void GetOfficer(EntityUid uid, out string officer)
    {
        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, uid);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        officer = tryGetIdentityShortInfoEvent.Title ?? Loc.GetString("criminal-records-console-unknown-officer");
    }

    private void OnChangeStatus(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordChangeStatus msg)
    {
        // prevent malf client violating wanted/reason nullability
        if (msg.Status == SecurityStatus.Wanted != (msg.Reason != null) &&
            msg.Status == SecurityStatus.Suspected != (msg.Reason != null) &&
            msg.Status == SecurityStatus.Hostile != (msg.Reason != null) &&
            msg.Status == SecurityStatus.Search != (msg.Reason != null) && // Goobstation
            msg.Status == SecurityStatus.Dangerous != (msg.Reason != null) &&  // Goobstation
            msg.Status == SecurityStatus.Demote != (msg.Reason != null)) // Goobstation
            return;

        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (!_records.TryGetRecord<CriminalRecord>(key.Value, out var record) || record.Status == msg.Status)
            return;

        // validate the reason
        string? reason = null;
        if (msg.Reason != null)
        {
            reason = msg.Reason.Trim();
            if (reason.Length < 1 || reason.Length > ent.Comp.MaxStringLength)
                return;
        }

        var oldStatus = record.Status;

        var name = record.GeneralRecordSnapshot?.Name ?? _records.RecordName(key.Value); // Pirate: records photos
        GetOfficer(mob.Value, out var officer);

        // when arresting someone add it to history automatically
        // fallback exists if the player was not set to wanted beforehand
        if (msg.Status == SecurityStatus.Detained)
        {
            var oldReason = record.Reason ?? Loc.GetString("criminal-records-console-unspecified-reason");
            var history = Loc.GetString("criminal-records-console-auto-history", ("reason", oldReason));
            _criminalRecords.TryAddHistory(key.Value, history, officer);
        }

        // will probably never fail given the checks above
        name = record.GeneralRecordSnapshot?.Name ?? _records.RecordName(key.Value); // Pirate: records photos
        officer = Loc.GetString("criminal-records-console-unknown-officer");
        var jobName = "Unknown";

        _records.TryGetRecord<GeneralStationRecord>(key.Value, out var entry);
        if (entry != null)
            jobName = entry.JobTitle;
        else if (_records.TryGetRecord<CriminalRecord>(key.Value, out var criminalEntry) // Pirate: records photos
                 && criminalEntry.GeneralRecordSnapshot != null) // Pirate: records photos
            jobName = criminalEntry.GeneralRecordSnapshot.JobTitle; // Pirate: records photos

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, mob.Value);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        if (tryGetIdentityShortInfoEvent.Title != null)
            officer = tryGetIdentityShortInfoEvent.Title;

        _criminalRecords.TryChangeStatus(key.Value, msg.Status, msg.Reason, officer);

        (string, object)[] args;
        if (reason != null)
            args = new (string, object)[] { ("name", name), ("officer", officer), ("reason", reason), ("job", jobName) };
        else
            args = new (string, object)[] { ("name", name), ("officer", officer), ("job", jobName) };

        // figure out which radio message to send depending on transition
        var statusString = (oldStatus, msg.Status) switch
        {
            (_, SecurityStatus.Hostile) => "hostile",
            (_, SecurityStatus.Eliminated) => "eliminated",
            // person has been detained
            (_, SecurityStatus.Detained) => "detained",
            // person did something sus
            (_, SecurityStatus.Suspected) => "suspected",
            // released on parole
            (_, SecurityStatus.Paroled) => "paroled",
            // prisoner did their time
            (_, SecurityStatus.Discharged) => "released",
            // going from any other state to wanted, AOS or prisonbreak / lazy secoff never set them to released and they reoffended
            (_, SecurityStatus.Wanted) => "wanted",
            (SecurityStatus.Hostile, SecurityStatus.None) => "not-hostile",
            (SecurityStatus.Eliminated, SecurityStatus.None) => "not-eliminated",
            // person has been sentenced to perma
            (_, SecurityStatus.Perma) => "perma", // Goobstation
            // person needs to be searched
            (_, SecurityStatus.Search) => "search", // Goobstation
            // person is very dangerous
            (_, SecurityStatus.Dangerous) => "dangerous", // Goobstation
            // person is demoted from their job
            (_, SecurityStatus.Demote) => "demote", // Goobstation
            // person is no longer sus
            (SecurityStatus.Suspected, SecurityStatus.None) => "not-suspected",
            // going from wanted to none, must have been a mistake
            (SecurityStatus.Wanted, SecurityStatus.None) => "not-wanted",
            // criminal status removed
            (SecurityStatus.Detained, SecurityStatus.None) => "released",
            // criminal is no longer on parole
            (SecurityStatus.Paroled, SecurityStatus.None) => "not-parole",
            // criminal is no longer in perma
            (SecurityStatus.Perma, SecurityStatus.None) => "not-perma", // Goobstation
            // person no longer needs to be searched
            (SecurityStatus.Search, SecurityStatus.None) => "not-search", // Goobstation
            // person is no longer dangerous
            (SecurityStatus.Dangerous, SecurityStatus.None) => "not-dangerous", // Goobstation
            // person no longer demoted
            (SecurityStatus.Demote, SecurityStatus.None) => "not-demoted", // Goobstation
            // this is impossible
            _ => "not-wanted"
        };
        _radio.SendRadioMessage(ent, Loc.GetString($"criminal-records-console-{statusString}", args),
            ent.Comp.SecurityChannel, ent);

        UpdateUserInterface(ent);
    }

    private void OnAddHistory(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordAddHistory msg)
    {
        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        var line = msg.Line.Trim();
        if (line.Length < 1 || line.Length > ent.Comp.MaxStringLength)
            return;

        GetOfficer(mob.Value, out var officer);

        if (!_criminalRecords.TryAddHistory(key.Value, line, officer))
            return;

        // no radio message since its not crucial to officers patrolling

        UpdateUserInterface(ent);
    }

    private void OnDeleteHistory(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordDeleteHistory msg)
    {
        if (!CheckSelected(ent, msg.Actor, out _, out var key))
            return;

        if (!_criminalRecords.TryDeleteHistory(key.Value, msg.Index))
            return;

        // a bit sus but not crucial to officers patrolling

        UpdateUserInterface(ent);
    }

    #region Pirate: records photos
    private void OnEditIdentity(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordEditIdentity msg)
    {
        if (!CheckSelected(ent, msg.Actor, out _, out var key))
            return;

        if (!_records.TryGetRecord<CriminalRecord>(key.Value, out var criminalRecord))
        {
            return;
        }

        var stationRecord = _records.TryGetRecord<GeneralStationRecord>(key.Value, out var existingGeneral)
            ? existingGeneral
            : criminalRecord.GeneralRecordSnapshot is { } snapshot
                ? snapshot with { }
                : null;

        if (stationRecord == null)
            return;

        if (!_prototypeManager.TryIndex<SpeciesPrototype>(msg.Species, out var speciesProto)
            || !speciesProto.RoundStart)
        {
            return;
        }

        var age = Math.Clamp(msg.Age, speciesProto.MinAge, speciesProto.MaxAge);
        var profile = BuildEditedProfile(stationRecord, criminalRecord, speciesProto.ID, age, msg.Gender);
        var nationality = NormalizeNationality(msg.Nationality, profile);
        profile = profile.WithNationality(nationality);
        var employer = NormalizeEmployer(msg.Employer, profile);
        profile = profile.WithEmployer(employer);

        stationRecord.Species = speciesProto.ID;
        stationRecord.Age = age;
        stationRecord.Gender = msg.Gender;
        stationRecord.Nationality = nationality;
        stationRecord.Employer = employer;
        criminalRecord.GeneralRecordSnapshot = stationRecord with { };

        if (existingGeneral != null)
        {
            existingGeneral.Species = stationRecord.Species;
            existingGeneral.Age = stationRecord.Age;
            existingGeneral.Gender = stationRecord.Gender;
            existingGeneral.Nationality = stationRecord.Nationality;
            existingGeneral.Employer = stationRecord.Employer;
        }

        if (criminalRecord.PortraitProfileSnapshot != null || criminalRecord.PortraitImageData is { Length: > 0 })
            criminalRecord.PortraitProfileSnapshot = profile;

        _records.Synchronize(key.Value);
    }

    private void OnEditForensics(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordEditForensics msg)
    {
        if (!CheckSelected(ent, msg.Actor, out _, out var key))
            return;

        if (!_records.TryGetRecord<CriminalRecord>(key.Value, out var criminalRecord))
            return;

        var stationRecord = _records.TryGetRecord<GeneralStationRecord>(key.Value, out var existingGeneral)
            ? existingGeneral
            : criminalRecord.GeneralRecordSnapshot is { } snapshot
                ? snapshot with { }
                : null;

        if (stationRecord == null)
            return;

        var fingerprint = msg.Fingerprint.Trim();
        var dna = msg.Dna.Trim();
        if (fingerprint.Length > ent.Comp.MaxStringLength || dna.Length > ent.Comp.MaxStringLength)
            return;

        stationRecord.Fingerprint = string.IsNullOrWhiteSpace(fingerprint)
            ? null
            : fingerprint;
        stationRecord.DNA = string.IsNullOrWhiteSpace(dna)
            ? null
            : dna;
        criminalRecord.GeneralRecordSnapshot = stationRecord with { };

        if (existingGeneral != null)
        {
            existingGeneral.Fingerprint = stationRecord.Fingerprint;
            existingGeneral.DNA = stationRecord.DNA;
        }

        _records.Synchronize(key.Value);
    }

    private HumanoidCharacterProfile BuildEditedProfile(
        GeneralStationRecord stationRecord,
        CriminalRecord criminalRecord,
        string species,
        int age,
        Gender gender)
    {
        var sameSpeciesSnapshot = criminalRecord.PortraitProfileSnapshot != null
            && criminalRecord.PortraitProfileSnapshot.Species == species;

        var profile = sameSpeciesSnapshot
            ? new HumanoidCharacterProfile(criminalRecord.PortraitProfileSnapshot!)
            : HumanoidCharacterProfile.DefaultWithSpecies(species).WithName(stationRecord.Name);

        return profile
            .WithSpecies(species)
            .WithAge(age)
            .WithGender(gender)
            .WithNationality(stationRecord.Nationality)
            .WithEmployer(stationRecord.Employer);
    }

    private string NormalizeNationality(string requestedNationality, HumanoidCharacterProfile profile)
    {
        if (string.IsNullOrWhiteSpace(requestedNationality))
            return string.Empty;

        if (_prototypeManager.TryIndex<NationalityPrototype>(requestedNationality, out var nationality)
            && RequirementsMet(nationality.Requirements, profile.WithNationality(requestedNationality)))
        {
            return requestedNationality;
        }

        if (_prototypeManager.TryIndex<NationalityPrototype>(SharedHumanoidAppearanceSystem.DefaultNationality, out var defaultNationality)
            && RequirementsMet(defaultNationality.Requirements, profile.WithNationality(defaultNationality.ID)))
        {
            return defaultNationality.ID;
        }

        foreach (var prototype in _prototypeManager.EnumeratePrototypes<NationalityPrototype>())
        {
            if (RequirementsMet(prototype.Requirements, profile.WithNationality(prototype.ID)))
                return prototype.ID;
        }

        return SharedHumanoidAppearanceSystem.DefaultNationality;
    }

    private string NormalizeEmployer(string requestedEmployer, HumanoidCharacterProfile profile)
    {
        if (string.IsNullOrWhiteSpace(requestedEmployer))
            return string.Empty;

        if (_prototypeManager.TryIndex<EmployerPrototype>(requestedEmployer, out var employer)
            && RequirementsMet(employer.Requirements, profile.WithEmployer(requestedEmployer)))
        {
            return requestedEmployer;
        }

        if (_prototypeManager.TryIndex<EmployerPrototype>(SharedHumanoidAppearanceSystem.DefaultEmployer, out var defaultEmployer)
            && RequirementsMet(defaultEmployer.Requirements, profile.WithEmployer(defaultEmployer.ID)))
        {
            return defaultEmployer.ID;
        }

        foreach (var prototype in _prototypeManager.EnumeratePrototypes<EmployerPrototype>())
        {
            if (RequirementsMet(prototype.Requirements, profile.WithEmployer(prototype.ID)))
                return prototype.ID;
        }

        return SharedHumanoidAppearanceSystem.DefaultEmployer;
    }

    private bool RequirementsMet(IReadOnlyCollection<JobRequirement>? requirements, HumanoidCharacterProfile profile)
    {
        if (requirements == null || requirements.Count == 0)
            return true;

        foreach (var requirement in requirements)
        {
            if (!requirement.Check(EntityManager, _prototypeManager, profile, EmptyPlayTimes, out _))
                return false;
        }

        return true;
    }
    private void OnPrintPhoto(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordPrintPhoto msg)
    {
        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (msg.RecordKey != key.Value.Id)
            return;

        if (_activePortraitPrintJobs.Contains(ent))
        {
            _popup.PopupEntity(Loc.GetString("criminal-records-console-photo-print-busy"), ent, mob.Value);
            return;
        }

        if (!_records.TryGetRecord<CriminalRecord>(key.Value, out var record))
            return;

        byte[]? imageData = null;
        byte[]? previewData = null;
        var recordChanged = false;

        if (record.PortraitImageData is { Length: > 0 } existingImage &&
            _photoSystem.TryPreparePhotoCardData(existingImage, record.PortraitPreviewData, out var preparedImageData, out var preparedPreviewData))
        {
            imageData = preparedImageData;
            previewData = preparedPreviewData;

            if (!ByteArraysEqual(record.PortraitImageData, preparedImageData) ||
                !ByteArraysEqual(record.PortraitPreviewData, preparedPreviewData))
            {
                record.PortraitImageData = [.. preparedImageData];
                record.PortraitPreviewData = preparedPreviewData == null ? null : [.. preparedPreviewData];
                recordChanged = true;
            }
        }
        else if (msg.GeneratedImageData is { Length: > 0 } generatedImage &&
                 _photoSystem.TryPreparePhotoCardData(generatedImage, null, out var generatedPreparedImage, out var generatedPreparedPreview))
        {
            imageData = generatedPreparedImage;
            previewData = generatedPreparedPreview;

            record.PortraitImageData = [.. generatedPreparedImage];
            record.PortraitPreviewData = generatedPreparedPreview == null ? null : [.. generatedPreparedPreview];
            recordChanged = true;
        }

        if (recordChanged)
            _records.Synchronize(key.Value);

        if (imageData is not { Length: > 0 })
            return;

        string? printedName = null;
        if (_records.TryGetRecord<GeneralStationRecord>(key.Value, out var stationRecord))
            printedName = stationRecord.Name;
        else if (record.GeneralRecordSnapshot != null)
            printedName = record.GeneralRecordSnapshot.Name;

        _activePortraitPrintJobs.Add(ent);
        _audio.PlayPvs(PortraitPrintSound, ent);
        _popup.PopupEntity(Loc.GetString("criminal-records-console-photo-print-start"), ent, mob.Value);

        Timer.Spawn(PortraitPrintDelay, () =>
        {
            _activePortraitPrintJobs.Remove(ent);

            if (TerminatingOrDeleted(ent))
                return;

            var printed = Spawn("PhotoCard", Transform(ent).Coordinates);
            if (!TryComp<PhotoCardComponent>(printed, out var photo))
            {
                Del(printed);
                return;
            }

            var finalImage = imageData!; // Pirate: records photos
            _photoSystem.TrySetPhotoCardData(printed, photo, finalImage, previewData, customName: printedName); // Pirate: records photos

            if (!TerminatingOrDeleted(mob.Value))
                _popup.PopupEntity(Loc.GetString("criminal-records-console-photo-print-complete"), printed, mob.Value);
        });
    }

    private void OnUploadPhoto(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordUploadPhoto msg)
    {
        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (!TryGetHeldPhotoCard(mob.Value, out var heldPhoto) ||
            heldPhoto.ImageData is not { Length: > 0 } imageData ||
            !_records.TryGetRecord<CriminalRecord>(key.Value, out var record))
            return;

        if (!_photoSystem.TryPreparePhotoCardData(imageData, heldPhoto.PreviewData, out var preparedImageData, out var preparedPreviewData))
            return;

        record.PortraitImageData = [.. preparedImageData];
        record.PortraitPreviewData = preparedPreviewData == null ? null : [.. preparedPreviewData];

        _audio.PlayPvs(PortraitUploadSound, ent);
        _records.Synchronize(key.Value);
        UpdateUserInterface(ent);
    }

    private void OnStoreGeneratedPhoto(Entity<CriminalRecordsConsoleComponent> ent, ref CriminalRecordStoreGeneratedPhoto msg)
    {
        if (!_access.IsAllowed(msg.Actor, ent))
            return;

        if (ent.Comp.ActiveKey != msg.RecordKey)
            return;

        if (_station.GetOwningStation(ent) is not { } station)
            return;

        var key = new StationRecordKey(msg.RecordKey, station);
        if (!_records.TryGetRecord<CriminalRecord>(key, out var record) ||
            record.PortraitProfileSnapshot == null ||
            record.PortraitImageData is { Length: > 0 } ||
            !_photoSystem.TryPreparePhotoCardData(msg.ImageData, null, out var preparedImageData, out var preparedPreviewData))
        {
            return;
        }

        record.PortraitImageData = [.. preparedImageData];
        record.PortraitPreviewData = preparedPreviewData == null ? null : [.. preparedPreviewData];
        _records.Synchronize(key);

        UpdateUserInterface(ent);

        record.PortraitImageData = [.. preparedImageData];
        record.PortraitPreviewData = preparedPreviewData == null ? null : [.. preparedPreviewData];
        _records.Synchronize(key);

        if (ent.Comp.ActiveKey == msg.RecordKey)
            UpdateUserInterface(ent);
    }

    private bool TryGetHeldPhotoCard(EntityUid user, [NotNullWhen(true)] out PhotoCardComponent? photo)
    {
        photo = null;

        if (!TryComp<HandsComponent>(user, out var hands))
            return false;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
        {
            if (TryComp<PhotoCardComponent>(held, out var photoCard))
            {
                photo = photoCard;
                return true;
            }
        }

        return false;
    }

    private static bool ByteArraysEqual(byte[]? left, byte[]? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null || left.Length != right.Length)
            return false;

        return left.AsSpan().SequenceEqual(right);
    }
    #endregion

    private void UpdateUserInterface(Entity<CriminalRecordsConsoleComponent> ent)
    {
        var (uid, console) = ent;
        var owningStation = _station.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, CriminalRecordsConsoleKey.Key, new CriminalRecordsConsoleState());
            return;
        }

        // get the listing of records to display
        #region Pirate: records photos
        var listing = new Dictionary<uint, string>();
        foreach (var (recordId, criminalRecord) in _records.GetRecordsOfType<CriminalRecord>(owningStation.Value, stationRecords))
        {
            var key = new StationRecordKey(recordId, owningStation.Value);
            var effectiveRecord = TryGetEffectiveGeneralRecord(key, criminalRecord, stationRecords);
            if (effectiveRecord == null || _records.IsSkipped(console.Filter, effectiveRecord))
                continue;

            listing[recordId] = effectiveRecord.Name;
        }
        #endregion

        // filter the listing by the selected criminal record status
        //if NONE, dont filter by status, just show all crew
        if (console.FilterStatus != SecurityStatus.None)
        {
            listing = listing
                .Where(x => _records.TryGetRecord<CriminalRecord>(new StationRecordKey(x.Key, owningStation.Value), out var record) && record.Status == console.FilterStatus)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        var state = new CriminalRecordsConsoleState(listing, console.Filter);
        if (console.ActiveKey is { } id)
        {
            // get records to display when a crewmember is selected
            var key = new StationRecordKey(id, owningStation.Value);
            #region Pirate: records photos
            if (_records.TryGetRecord(key, out state.CriminalRecord, stationRecords)
                && TryGetEffectiveGeneralRecord(key, state.CriminalRecord, stationRecords) is { } stationRecord)
            {
                state.StationRecord = stationRecord;
                state.SelectedKey = id;
            }
            else
            {
                console.ActiveKey = null;
            }
            #endregion
        }

        // Set the Current Tab aka the filter status type for the records list
        state.FilterStatus = console.FilterStatus;

        _ui.SetUiState(uid, CriminalRecordsConsoleKey.Key, state);
    }

    #region Pirate: records photos
    private CriminalRecord CreateCriminalRecordFromGeneral(GeneralStationRecord record)
    {
        var criminalRecord = new CriminalRecord
        {
            GeneralRecordSnapshot = record with { }
        };

        if (!string.IsNullOrWhiteSpace(record.Species))
            criminalRecord.PortraitProfileSnapshot = BuildProfileFromGeneralRecord(record);

        return criminalRecord;
    }

    private HumanoidCharacterProfile BuildProfileFromGeneralRecord(GeneralStationRecord record)
    {
        var species = string.IsNullOrWhiteSpace(record.Species)
            ? (string) SharedHumanoidAppearanceSystem.DefaultSpecies
            : record.Species;

        return HumanoidCharacterProfile.DefaultWithSpecies(species)
            .WithName(record.Name)
            .WithAge(record.Age <= 0 ? 18 : record.Age)
            .WithGender(record.Gender)
            .WithNationality(record.Nationality)
            .WithEmployer(record.Employer);
    }

    private GeneralStationRecord? TryGetEffectiveGeneralRecord(
        StationRecordKey key,
        CriminalRecord criminalRecord,
        StationRecordsComponent? stationRecords = null)
    {
        if (_records.TryGetRecord<GeneralStationRecord>(key, out var stationRecord, stationRecords))
            return stationRecord;

        return criminalRecord.GeneralRecordSnapshot is { } snapshot ? snapshot with { } : null;
    }

    private List<uint> GetCriminalRecordIdsByName(EntityUid station, string name, StationRecordsComponent? stationRecords = null)
    {
        var matches = new List<uint>();
        foreach (var (recordId, criminalRecord) in _records.GetRecordsOfType<CriminalRecord>(station, stationRecords))
        {
            var key = new StationRecordKey(recordId, station);
            var effectiveRecord = TryGetEffectiveGeneralRecord(key, criminalRecord, stationRecords);
            if (effectiveRecord?.Name == name)
                matches.Add(recordId);
        }

        return matches;
    }
    #endregion

    /// <summary>
    /// Boilerplate that most actions use, if they require that a record be selected.
    /// Obviously shouldn't be used for selecting records.
    /// </summary>
    private bool CheckSelected(Entity<CriminalRecordsConsoleComponent> ent, EntityUid user,
        [NotNullWhen(true)] out EntityUid? mob, [NotNullWhen(true)] out StationRecordKey? key)
    {
        key = null;
        mob = null;
        if (!_access.IsAllowed(user, ent))
        {
            _popup.PopupEntity(Loc.GetString("criminal-records-permission-denied"), ent, user);
            return false;
        }

        if (ent.Comp.ActiveKey is not { } id)
            return false;

        // checking the console's station since the user might be off-grid using on-grid console
        if (_station.GetOwningStation(ent) is not { } station)
            return false;

        key = new StationRecordKey(id, station);
        mob = user;
        return true;
    }

    /// <summary>
    /// Checks if the new identity's name has a criminal record attached to it, and gives the entity the icon that
    /// belongs to the status if it does.
    /// </summary>
    public void CheckNewIdentity(EntityUid uid)
    {
        var name = Identity.Name(uid, EntityManager);
        var xform = Transform(uid);

        // TODO use the entity's station? Not the station of the map that it happens to currently be on?
        var station = _station.GetStationInMap(xform.MapID);

        if (station != null) // Pirate: records photos
        {
            var matches = GetCriminalRecordIdsByName(station.Value, name); // Pirate: records photos
            if (matches.Count == 1 // Pirate: records photos
                && _records.TryGetRecord<CriminalRecord>(new StationRecordKey(matches[0], station.Value), out var record)) // Pirate: records photos
            {
                if (record.Status != SecurityStatus.None)
                {
                    _criminalRecords.SetCriminalIcon(name, record.Status, uid);
                    return;
                }
            }
        }
        RemComp<CriminalRecordComponent>(uid);
    }
}
