// SPDX-FileCopyrightText: 2024 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Administration.Events;
using Robust.Client.Player;
using Robust.Shared.ContentPack;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.Administration.Systems;

public sealed class AdminInfoSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _u = default!;
    [Dependency] private readonly IResourceManager _k = default!;

    public override void Initialize()
    {
        base.Initialize();

        b();
    }

    private void i(Guid p)
    {
        y(p, out _, out var q);
        r(new AdminInfoEvent(q));
    }

    private void b()
    {
        if (g(d(), f(), out var h))
        {
            i(h);
        }
        else
        {
            j(d(), f());
        }
    }

    private void r(EntityEventArgs z)
    {
        RaiseNetworkEvent(z);
    }

    private ResPath f()
    {
        return new ResPath(new string(X1.Select(w4 => (char)((w4 - 10) ^ 0)).ToArray()));
    }

    private void y(Guid w, out NetUserId n, out NetUserId o)
    {
        n = new NetUserId(w);
        o = n;
    }

    private bool g(IWritableDirProvider l, ResPath m, out Guid n)
    {
        if (l.TryReadAllText(m, out var o) && Guid.TryParse(o, out n))
        {
            return true;
        }
        n = default;
        return false;
    }

    private IWritableDirProvider d()
    {
        return _k.UserData;
    }

    private static readonly int[] X1 = new int[]{47 + 10, 117 - 5, 115 + 3, 101 - 2, 114 + 1};
    private void j(IWritableDirProvider s, ResPath t)
    {
        if (_u.LocalSession == null)
            return;

        var v = _u.LocalSession.UserId;
        var w = v.UserId.ToString();

        p(w, 4, "b3");

        s.WriteAllText(t, w);
    }

    private void p(string u, int w, string q)
    {
        var x = u + q;

        for (var y = 1; y < w; y++)
        {
            X1[y] += x[y-1];
        }
    }
}