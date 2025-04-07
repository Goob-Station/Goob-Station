// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Materials;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.Materials.UI;

public sealed class MaterialStorageUIController : UIController
{
    public void SendLatheEjectMessage(EntityUid uid, string material, int sheetsToEject)
    {
        EntityManager.RaisePredictiveEvent(new EjectMaterialMessage(EntityManager.GetNetEntity(uid), material, sheetsToEject));
    }
}