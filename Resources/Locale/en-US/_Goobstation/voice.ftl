ui-options-hear-self = Hear yourself in voice chat
ui-options-voice-chat-volume = Voice Chat Volume
ui-options-function-voice-push-to-talk = Voice Push-to-Talk (Local)
ui-options-function-voice-push-to-talk-radio = Voice Push-to-Talk (Radio)

ui-escape-voice-chat = Voice Chat

cmd-voicechat-desc = Shows the voice chat guide with your link and code
cmd-voicechat-help = Usage: voicechat
cmd-voicechat-unavailable = Voice chat isn't enabled on this server.

voice-link-connected = Your mic is connected.
voice-link-disconnected = Your mic isn't connected yet.
voice-link-computer-title = On this computer
voice-link-computer-text = Opens the voice page in your browser.
voice-link-phone-title = On your phone
voice-link-phone-text = Go to this address, then type in the code:
voice-link-expiry = Leave the page open while you play. The code only works until you leave the server.
voice-link-loading = Getting your link...
voice-link-open = Open in browser
voice-link-copy = Copy link
voice-link-close = Close

voice-guide-title = Voice Chat
voice-guide-unbound = (no key set)
voice-guide-intro = Voice chat on this server works like talking in person. Your voice comes from your character, and only people close enough to hear them will hear you.
voice-guide-talking-title = Talking
voice-guide-talking =
    The further away someone is, the quieter you get. Anything in the way, like a wall, a door or a window, muffles you.

    Talk louder than you normally do and you'll shout, which can be heard further away. Whispers can only be heard by people a few tiles away.
    Any situations in which you normally wouldn't be able to type in chat in SS14, like when you are muted, dead, etc, you will not be able to speak out loud.

    Hold [bold]{$local}[/bold] to talk. If you'd rather not use PTT, switch to voice activation on the voice chat page.
voice-guide-radio-title = Radio
voice-guide-radio =
    Hold [bold]{$radio}[/bold] to talk over the radio. Anyone nearby can still hear you as if you are speaking normally.

    The radio button in chat is used to select your channel. If it's set to Local, [bold]{$radio}[/bold] uses the first channel on your headset. The same menu lets you mute channels you don't care about and turn the radio volume down.

    On voice activation, picking a channel keeps you on the radio every time you speak, so set it back to Local when you're done. Handheld radios and intercoms with their mic switched on will pick you up too if you talk near them.
voice-guide-station-title = Around the station
voice-guide-station =
    Phones and holopads pick up anyone nearby. Megaphones allow you to be heard from several tiles away. If you have access, a communications console lets you broadcast to the whole station.

    The AI hears and talks through its cameras. Ghosts can talk to each other, but living players can't hear them.
voice-guide-controls-title = Controls
voice-guide-controls =
    The mic and headphone icons next to the chat box mute your mic or deafen you. Right-click someone to mute them or turn them up or down.

    Voice volume is in Options under Audio. You can rebind the push-to-talk keys under Controls.
voice-guide-rules-title = Rules
voice-guide-rules =
    Voice chat has the same rules as text chat. With two exceptions:
    1. Keep your microphone at a reasonable level. If you are intentonally micspamming/earraping, you will be muted or banned.
    2. Only the clown can use soundboards.
    Have fun!
voice-guide-connect-title = Connect your mic

cmd-voicemute-desc = Toggles whether a player's voice chat is muted.
cmd-voicemute-help = Usage: voicemute <username>
cmd-voicemute-hint = <username>
cmd-voicemute-muted = Muted voice chat for {$player}.
cmd-voicemute-unmuted = Unmuted voice chat for {$player}.

comms-console-menu-voice-broadcast-button = Voice Broadcast
comms-console-menu-voice-broadcast-button-tooltip = Speak to the whole station over voice chat for a short time.
comms-console-menu-voice-broadcast-stop = End Voice Broadcast ({$seconds}s)
comms-console-menu-voice-broadcast-busy = Voice Broadcast in Progress
comms-console-menu-voice-broadcast-cooldown = Voice Broadcast ({$seconds}s)

voice-broadcast-denied = Access denied.
voice-broadcast-not-connected = Open voice chat first: Esc, Voice Chat.
voice-broadcast-started = You're broadcasting to the whole station for {$seconds} seconds.
voice-broadcast-ended = Your voice broadcast has ended.

voice-radio-button-off = Local
voice-radio-button-tooltip = Voice chat radio: pick the channel you talk on, which channels you hear, and radio volume.

voice-verb-category = Voice chat
voice-verb-mute = Mute
voice-verb-unmute = Unmute
voice-verb-volume = Volume {$percent}%

voice-prompt-later = Not now
voice-prompt-never = Don't ask again

voice-speaker-unknown = Unknown
voice-route-radio = Radio
voice-route-telephone = Phone
voice-route-holopad = Holopad
voice-route-camera = Camera
voice-route-broadcast = Broadcast
voice-speaker-self = You
voice-self-blocked = Can't speak

voice-radio-popup-talk = Talk
voice-radio-popup-hear = Hear
voice-radio-popup-volume = Volume
voice-radio-popup-volume-value = {$percent}%

voice-loudness-shout = Shouting
voice-loudness-whisper = Whispering
voice-loudness-megaphone = Megaphone

admin-player-actions-window-voice-logs = Voice Logs
cmd-voicelogs-desc = Opens the voice chat log viewer.
cmd-voicelogs-help = Usage: voicelogs [username]
cmd-voicelogs-hint = [username]
voice-logs-verb = Voice Logs
voice-logs-title = Voice Logs
voice-logs-round = Round
voice-logs-round-entry = Round {$id} · {$length}
voice-logs-round-current = Round {$id} (current) · {$length}
voice-logs-player = Player
voice-logs-add = Add
voice-logs-refresh = Refresh
voice-logs-play = Play
voice-logs-pause = Pause
voice-logs-fit = Fit
voice-logs-loading = Loading…
voice-logs-empty = Pick a player and press Add to stack their voice track here.
voice-logs-mute = Mute this track
voice-logs-solo = Solo this track
voice-logs-remove = Remove this track
voice-logs-lobby = Lobby
voice-logs-blocked = {$name} (blocked)

voice-god-verb = Voice of God
voice-god-verb-stop = Stop Voice of God
voice-god-started = You are now speaking to {$target} as the Voice of God. Nobody else can hear you.
voice-god-started-offline = You are now speaking to {$target} as the Voice of God, but your voice page isn't open. Open it from Esc > Voice Chat.
voice-god-hear-self = You'll hear yourself as the voice too, with the same glow and light on your screen.
voice-god-stopped = You are no longer speaking as the Voice of God.
voice-god-name = A Voice
voice-god-log-name = Voice of God to {$target}
voice-route-god = Divine
voice-self-god = Voice of God
cmd-voiceofgod-desc = Speak as the Voice of God to one player, everyone nearby, a department, or the whole station. Run with no arguments to stop.
cmd-voiceofgod-help = Usage: voiceofgod <username> [hearSelf] | voiceofgod radius <tiles> [hearSelf] | voiceofgod department <id> [hearSelf] | voiceofgod station [hearSelf]
cmd-voiceofgod-hint = <username> | radius | department | station
cmd-voiceofgod-radius-hint = <tiles>
cmd-voiceofgod-department-hint = <department>
cmd-voiceofgod-hear-self-hint = [hearSelf]
cmd-voiceofgod-bad-hear-self = hearSelf has to be true or false.
cmd-voiceofgod-bad-radius = Give a radius in tiles between 1 and {$max}.
cmd-voiceofgod-bad-department = That department doesn't exist.
voice-god-target-radius = everyone within {$radius} tiles of you
voice-god-target-department = the {$department} department
voice-god-target-station = the whole station
voice-god-cue = A voice echoes in your head...

voice-mic-offline = Voice chat isn't open. Click to open it.
voice-mic-ready = Microphone on. Click to mute.
voice-mic-live = You're live. Click to mute.
voice-mic-blocked = Your character can't speak right now.
voice-mic-muted = Microphone muted. Click to unmute.
voice-mic-deafened = Deafened. Click to undeafen and unmute.
voice-deafen-off = Deafen: silence all voice chat and mute your microphone.
voice-deafen-on = Deafened. Click to undeafen.
voice-radio-popup-mute-all = Mute all
voice-radio-popup-unmute-all = Unmute all
