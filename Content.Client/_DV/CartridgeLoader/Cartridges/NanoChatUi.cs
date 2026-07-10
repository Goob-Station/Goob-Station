// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Fragments;
using Content.Client._Pirate.CartridgeLoader.Cartridges; // Pirate: pda fix
using Content.Shared.CartridgeLoader;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client._DV.CartridgeLoader.Cartridges;

public sealed partial class NanoChatUi : UIFragment
{
    private NanoChatUiFragmentPirate? _fragment; // Pirate: pda fix

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoChatUiFragmentPirate(); // Pirate: pda fix

        _fragment.OnMessageSent += message => // Pirate: camera (nanochat gallery)
        {
            SendNanoChatUiMessage(message, userInterface); // Pirate: camera (nanochat gallery)
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is NanoChatUiState cast)
            _fragment?.UpdateState(cast);
    }

    private static void SendNanoChatUiMessage(NanoChatUiMessageEvent nanoChatMessage, // Pirate: camera (nanochat gallery)
        BoundUserInterface userInterface)
    {
        var message = new CartridgeUiMessage(nanoChatMessage);
        userInterface.SendMessage(message);
    }
}
