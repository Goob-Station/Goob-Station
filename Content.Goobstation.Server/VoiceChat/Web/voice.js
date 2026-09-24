"use strict";

const CONFIG = Object.assign({ wsUrl: "", wsPort: 0 }, readConfig());
const TOKEN_KEY = "voice-token";
const SETTINGS_KEY = "voice-settings";
const CLOSE_INVALID_TOKEN = 4001;
const CLOSE_REPLACED = 4002;
const METER_FLOOR_DB = -70;

const DEFAULT_SETTINGS = {
    mode: "ptt",
    threshold: -45,
    gain: 1,
    muted: false,
    effect: "none",
    deviceId: "",
    noiseSuppression: true,
    echoCancellation: true,
    autoGainControl: true,
};

const el = id => document.getElementById(id);
const ui = {
    status: el("status"),
    statusDetail: el("status-detail"),
    start: el("start"),
    retry: el("retry"),
    controls: el("controls"),
    indicatorLabel: el("indicator-label"),
    meterFill: el("meter-fill"),
    meterThreshold: el("meter-threshold"),
    modePtt: el("mode-ptt"),
    modeVad: el("mode-vad"),
    thresholdRow: el("threshold-row"),
    threshold: el("threshold"),
    thresholdValue: el("threshold-value"),
    gain: el("gain"),
    gainValue: el("gain-value"),
    mute: el("mute"),
    device: el("device"),
    effectRow: el("effect-row"),
    effect: el("effect"),
    noiseSuppression: el("noise-suppression"),
    echoCancellation: el("echo-cancellation"),
    autoGainControl: el("auto-gain-control"),
};

const settings = loadSettings();
const token = takeToken();

let worker = null;
let socketReady = false;
let retrying = false;
let stopped = false;
let serverState = null;
let audio = null;
let lastLevel = { db: -100, transmitting: false, voice: false };

function readConfig() {
    try {
        return JSON.parse(document.getElementById("voice-config").textContent) || {};
    } catch {
        return {};
    }
}

function loadSettings() {
    try {
        const stored = JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}");
        return Object.assign({}, DEFAULT_SETTINGS, stored);
    } catch {
        return Object.assign({}, DEFAULT_SETTINGS);
    }
}

function saveSettings() {
    try {
        localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
    } catch {
    }
}

function takeToken() {
    const fromHash = decodeURIComponent(location.hash.slice(1));
    if (fromHash) {
        try {
            localStorage.setItem(TOKEN_KEY, fromHash);
        } catch {
        }
        history.replaceState(null, "", location.pathname + location.search);
        return fromHash;
    }

    try {
        return localStorage.getItem(TOKEN_KEY) || "";
    } catch {
        return "";
    }
}

function forgetToken() {
    try {
        localStorage.removeItem(TOKEN_KEY);
    } catch {
    }
}

function setStatus(text, detail, tone) {
    if (ui.status.textContent !== text)
        ui.status.textContent = text;
    if (ui.statusDetail.textContent !== (detail || ""))
        ui.statusDetail.textContent = detail || "";
    document.body.dataset.tone = tone || "neutral";
}

function socketCandidates() {
    if (CONFIG.wsUrl)
        return [CONFIG.wsUrl];

    const sameOrigin = new URL("ws", location.href);
    sameOrigin.protocol = location.protocol === "https:" ? "wss:" : "ws:";
    const list = [sameOrigin.href];

    if (location.protocol === "http:" && CONFIG.wsPort)
        list.push(`ws://${location.hostname}:${CONFIG.wsPort}/voice/ws`);

    return list;
}

function startConnection() {
    stopped = false;
    retrying = false;
    worker.postMessage({ type: "start", candidates: socketCandidates(), token });
}

function onWorkerMessage(event) {
    const message = event.data;
    switch (message.type) {
        case "connecting":
            socketReady = false;
            break;
        case "ready":
            socketReady = true;
            retrying = false;
            sendEffect();
            break;
        case "state":
            serverState = message.state;
            break;
        case "retrying":
            socketReady = false;
            serverState = null;
            retrying = true;
            setStatus("Can't reach the voice server.", `Retrying in ${Math.round(message.delay / 1000)}s.`, "error");
            return;
        case "stopped":
            socketReady = false;
            serverState = null;
            stopped = true;
            if (message.code === CLOSE_INVALID_TOKEN) {
                forgetToken();
                setStatus("Link expired.", "Get a new one in game: Esc, Voice Chat.", "error");
            }
            else if (message.code === CLOSE_REPLACED)
                setStatus("Opened in another tab.", "", "error");
            ui.retry.hidden = message.code !== CLOSE_REPLACED;
            return;
    }

    render();
}

function sendEffect() {
    if (worker && socketReady)
        worker.postMessage({ type: "send", text: JSON.stringify({ t: "effect", effect: settings.effect }) });
}

function pushConfig() {
    if (!audio)
        return;

    audio.node.port.postMessage({
        type: "config",
        mode: settings.mode,
        threshold: settings.threshold,
        gain: settings.gain,
        muted: settings.muted,
    });
}

async function startAudio() {
    const constraints = {
        audio: {
            channelCount: 1,
            noiseSuppression: settings.noiseSuppression,
            echoCancellation: settings.echoCancellation,
            autoGainControl: settings.autoGainControl,
        },
    };

    if (settings.deviceId)
        constraints.audio.deviceId = { exact: settings.deviceId };

    let stream;
    try {
        stream = await navigator.mediaDevices.getUserMedia(constraints);
    } catch (error) {
        if (settings.deviceId && error.name === "OverconstrainedError") {
            settings.deviceId = "";
            saveSettings();
            return startAudio();
        }
        throw error;
    }

    const context = audio ? audio.context : new AudioContext();
    if (!audio)
        await context.audioWorklet.addModule("voice-worklet.js");

    await context.resume();

    const source = context.createMediaStreamSource(stream);
    const node = audio ? audio.node : new AudioWorkletNode(context, "voice-capture", {
        numberOfInputs: 1,
        numberOfOutputs: 1,
        outputChannelCount: [1],
        channelCount: 1,
        channelCountMode: "explicit",
    });

    if (!audio) {
        const sink = context.createGain();
        sink.gain.value = 0;
        node.connect(sink).connect(context.destination);
        node.port.onmessage = event => {
            if (event.data.type === "level") {
                lastLevel = event.data;
                renderLevel();
            }
        };

        const channel = new MessageChannel();
        node.port.postMessage({ type: "net-port", port: channel.port1 }, [channel.port1]);
        worker.postMessage({ type: "audio-port", port: channel.port2 }, [channel.port2]);
    }

    source.connect(node);
    audio = { context, node, source, stream };
    pushConfig();
    await refreshDevices();
}

async function restartAudio() {
    if (!audio)
        return;

    audio.source.disconnect();
    audio.stream.getTracks().forEach(track => track.stop());
    try {
        await startAudio();
    } catch (error) {
        reportMicError(error);
    }
}

async function refreshDevices() {
    const devices = (await navigator.mediaDevices.enumerateDevices()).filter(device => device.kind === "audioinput");
    const activeId = audio && audio.stream.getAudioTracks()[0]?.getSettings().deviceId;
    ui.device.replaceChildren();

    for (const [index, device] of devices.entries()) {
        const option = document.createElement("option");
        option.value = device.deviceId;
        option.textContent = device.label || `Microphone ${index + 1}`;
        option.selected = device.deviceId === (settings.deviceId || activeId);
        ui.device.append(option);
    }
}

function reportMicError(error) {
    const denied = error && (error.name === "NotAllowedError" || error.name === "SecurityError");
    const missing = error && error.name === "NotFoundError";
    if (denied)
        setStatus("Microphone blocked.", "Allow it from the address bar, then press Start.", "error");
    else if (missing)
        setStatus("No microphone found.", "", "error");
    else
        setStatus("Couldn't start the microphone.", error && error.message ? error.message : String(error), "error");

    ui.start.hidden = false;
}

function renderLevel() {
    const range = -METER_FLOOR_DB;
    const clamp = value => Math.max(0, Math.min(1, (value - METER_FLOOR_DB) / range));
    ui.meterFill.style.transform = `scaleX(${clamp(lastLevel.db)})`;
    ui.meterThreshold.style.left = `${clamp(settings.threshold) * 100}%`;
    render();
}

function setText(element, text) {
    if (element.textContent !== text)
        element.textContent = text;
}

function syncSlider(input) {
    const min = Number(input.min);
    const max = Number(input.max);
    input.parentElement.style.setProperty("--value", String((Number(input.value) - min) / (max - min)));
}

function render() {
    const transmitting = !!lastLevel.transmitting;
    document.body.classList.toggle("transmitting", transmitting);
    ui.thresholdRow.hidden = settings.mode !== "vad";
    ui.effectRow.hidden = !(serverState && serverState.voiceChanger);
    ui.meterThreshold.hidden = settings.mode !== "vad";
    setText(ui.mute, settings.muted ? "Unmute" : "Mute");
    ui.mute.classList.toggle("negative", settings.muted);
    ui.mute.setAttribute("aria-pressed", String(settings.muted));

    if (!audio)
        setText(ui.indicatorLabel, "Microphone off");
    else if (settings.muted)
        setText(ui.indicatorLabel, "Muted");
    else if (transmitting)
        setText(ui.indicatorLabel, "Transmitting");
    else if (settings.mode === "ptt")
        setText(ui.indicatorLabel, "Hold push-to-talk in game");
    else
        setText(ui.indicatorLabel, "Listening");

    if (stopped || !audio)
        return;

    if (!socketReady) {
        if (!retrying)
            setStatus("Connecting…", "", "neutral");
        return;
    }

    const state = serverState;
    const name = state && state.name ? `Connected as ${state.name}` : "Connected";
    if (!state)
        setStatus(name, "", "neutral");
    else if (state.muted)
        setStatus("Muted by an admin.", "", "error");
    else if (state.lobby)
        setStatus(name, state.canSpeak ? "In the lobby. Everyone in the lobby hears you." : "Lobby voice is off.", "neutral");
    else if (!state.inGame)
        setStatus(name, "Join the round to talk.", "neutral");
    else if (!state.canSpeak)
        setStatus(name, "Your character can't speak right now.", "warn");
    else if (state.broadcasting)
        setStatus(name, "Broadcasting to the whole station.", "warn");
    else if (state.radio)
        setStatus(name, `Also speaking on ${state.radio} radio.`, "neutral");
    else
        setStatus(name, "", "neutral");
}

function bindControls() {
    ui.modePtt.checked = settings.mode === "ptt";
    ui.modeVad.checked = settings.mode === "vad";
    ui.threshold.value = settings.threshold;
    ui.gain.value = Math.round(settings.gain * 100);
    syncSlider(ui.threshold);
    syncSlider(ui.gain);
    ui.noiseSuppression.checked = settings.noiseSuppression;
    ui.echoCancellation.checked = settings.echoCancellation;
    ui.autoGainControl.checked = settings.autoGainControl;
    ui.thresholdValue.textContent = `${settings.threshold} dB`;
    ui.gainValue.textContent = `${Math.round(settings.gain * 100)}%`;

    const onModeChange = () => {
        settings.mode = ui.modeVad.checked ? "vad" : "ptt";
        saveSettings();
        pushConfig();
        renderLevel();
    };
    ui.modePtt.addEventListener("change", onModeChange);
    ui.modeVad.addEventListener("change", onModeChange);

    ui.threshold.addEventListener("input", () => {
        syncSlider(ui.threshold);
        settings.threshold = Number(ui.threshold.value);
        ui.thresholdValue.textContent = `${settings.threshold} dB`;
        saveSettings();
        pushConfig();
        renderLevel();
    });

    ui.gain.addEventListener("input", () => {
        syncSlider(ui.gain);
        settings.gain = Number(ui.gain.value) / 100;
        ui.gainValue.textContent = `${ui.gain.value}%`;
        saveSettings();
        pushConfig();
    });

    ui.mute.addEventListener("click", () => {
        settings.muted = !settings.muted;
        saveSettings();
        pushConfig();
        render();
    });

    ui.effect.value = settings.effect;
    ui.effect.addEventListener("change", () => {
        settings.effect = ui.effect.value;
        saveSettings();
        sendEffect();
    });

    ui.device.addEventListener("change", () => {
        settings.deviceId = ui.device.value;
        saveSettings();
        restartAudio();
    });

    for (const [key, input] of [
        ["noiseSuppression", ui.noiseSuppression],
        ["echoCancellation", ui.echoCancellation],
        ["autoGainControl", ui.autoGainControl],
    ]) {
        input.addEventListener("change", () => {
            settings[key] = input.checked;
            saveSettings();
            restartAudio();
        });
    }

    ui.start.addEventListener("click", async () => {
        ui.start.hidden = true;
        setStatus("Starting microphone…", "", "neutral");
        try {
            await startAudio();
        } catch (error) {
            reportMicError(error);
            return;
        }

        ui.controls.hidden = false;
        startConnection();
        render();
    });

    ui.retry.addEventListener("click", () => {
        ui.retry.hidden = true;
        startConnection();
        render();
    });

    if (navigator.mediaDevices)
        navigator.mediaDevices.addEventListener("devicechange", () => audio && refreshDevices());
}

function init() {
    bindControls();
    renderLevel();

    if (!window.isSecureContext || !navigator.mediaDevices || !window.AudioWorkletNode) {
        setStatus("Microphone needs HTTPS.", "This page has to be served over HTTPS or from localhost.", "error");
        ui.start.hidden = true;
        return;
    }

    if (!token) {
        setStatus("Open this page from the game.", "In game: Esc, Voice Chat.", "error");
        ui.start.hidden = true;
        return;
    }

    worker = new Worker("voice-worker.js");
    worker.onmessage = onWorkerMessage;
    setStatus("Ready", "", "neutral");
}

init();
