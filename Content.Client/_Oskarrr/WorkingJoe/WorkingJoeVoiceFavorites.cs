// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client._Oskarrr.WorkingJoe;

public sealed class WorkingJoeVoiceFavorites
{
    private static readonly ResPath Path = new("/working_joe_voice_favorites.txt");
    private readonly HashSet<string> _favorites = new();
    private readonly IResourceManager _resource;

    public WorkingJoeVoiceFavorites(IResourceManager resource)
    {
        _resource = resource;
        Load();
    }

    public bool Contains(string lineId) => _favorites.Contains(lineId);

    public void Toggle(string lineId)
    {
        if (!_favorites.Remove(lineId))
            _favorites.Add(lineId);

        Save();
    }

    private void Load()
    {
        if (!_resource.UserData.Exists(Path))
            return;

        if (!_resource.UserData.TryReadAllText(Path, out var text) || text == null)
            return;

        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
                _favorites.Add(trimmed);
        }
    }

    private void Save()
    {
        var content = string.Join("\n", _favorites);
        _resource.UserData.WriteAllText(Path, content);
    }
}
