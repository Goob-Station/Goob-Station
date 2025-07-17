// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using Robust.Shared.Player;

namespace Content.Goobstation.Common.MisandryBox;

public interface ISpiderManager
{
    public void Initialize();

    public void RequestSpider();
    public void AddTemporarySpider(ICommonSession? victim = null);
    public void AddPermanentSpider(ICommonSession? victim = null);
    public void ClearTemporarySpiders();
}
