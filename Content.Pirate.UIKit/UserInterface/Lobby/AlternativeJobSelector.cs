// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Pirate.Common.AlternativeJobs;
using Content.Shared.Roles;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Pirate.UIKit.UserInterface.Lobby;

[Virtual]
public sealed class AlternativeJobSelector : OptionButton
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly ProtoId<JobPrototype> _parentJobId;
    private Dictionary<ProtoId<JobPrototype>, ProtoId<AlternativeJobPrototype>> _alternatives = new();
    public event Action<string>? OnAlternativeSelected;

    public AlternativeJobSelector(ProtoId<JobPrototype> parentJobId)
    {
        IoCManager.InjectDependencies(this);

        _parentJobId = parentJobId;

        // Initialize the selector
        PopulateAlternatives();

        OnItemSelected += OnItemSelectedHandler;
    }

    private void OnItemSelectedHandler(ItemSelectedEventArgs args)
    {
        SelectId(args.Id);
        var protoId = GetAlternativeIdFromIndex(args.Id);
        OnAlternativeSelected?.Invoke(protoId);
    }

    private ProtoId<JobPrototype> GetAlternativeIdFromIndex(int index)
    {
        if (index == 0) return _parentJobId;

        // ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _alternatives.Count, nameof(index));

        return _alternatives.ElementAt(index).Key;
    }

    private void PopulateAlternatives()
    {
        Clear();
        _alternatives.Clear();

        // Add the original job as the first option
        if (_prototypeManager.TryIndex(_parentJobId, out var jobProto, false))
        {
            AddItem(jobProto.LocalizedName, 0); // Try to find localized name
        }
        else
        {
            AddItem(_parentJobId, 0); // Fallback to prototype ID
        }
        _alternatives.Add(_parentJobId, _parentJobId.Id);

        // Find all alternative jobs for this job
        var index = 1;
        foreach (var altJob in _prototypeManager.EnumeratePrototypes<AlternativeJobPrototype>())
        {
            if (altJob.ParentJobId == _parentJobId)
            {
                _alternatives.Add(altJob.ID, altJob);
                AddItem(altJob.LocalizedJobName, index);
                index++;
            }
        }

        // Disable the selector if there are no alternatives
        Disabled = _alternatives.Count <= 1;
    }

    public void SelectAlternative(string alternativeId)
    {
        // Check if protoID os valid
        if (!_prototypeManager.TryIndex(alternativeId, out AlternativeJobPrototype? altJobPrototype)) return;
        // Check if it's an valid alternative for this job
        if (!_alternatives.ContainsValue(altJobPrototype)) return;

        var index = 0;
        foreach (var (_, altJob) in _alternatives)
        {
            if (altJob == altJobPrototype)
            {
                SelectId(index);
                return;
            }
            index++;
        }
    }

}




