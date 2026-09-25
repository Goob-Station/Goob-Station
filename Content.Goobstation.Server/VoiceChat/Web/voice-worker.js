"use strict";

const CLOSE_INVALID_TOKEN = 4001;
const CLOSE_REPLACED = 4002;
const MAX_BUFFERED_BYTES = 64 * 1024;
const RECONNECT_DELAYS = [1000, 2000, 5000, 10000];

let candidates = [];
let token = "";
let socket = null;
let ready = false;
let candidateIndex = 0;
let preferredCandidate = -1;
let reconnectAttempt = 0;
let reconnectTimer = 0;
let stopped = true;
let sequence = 0;
let state = null;
let audioPort = null;

self.onmessage = event => {
    const message = event.data;
    if (message.type === "audio-port") {
        audioPort = message.port;
        audioPort.onmessage = frame => sendFrame(frame.data.flags, frame.data.payload);
        pushTransmit();
    } else if (message.type === "send") {
        if (socket && ready)
            socket.send(message.text);
    } else if (message.type === "start") {
        candidates = message.candidates;
        token = message.token;
        stopped = false;
        reconnectAttempt = 0;
        connect();
    }
};

function pushTransmit() {
    if (!audioPort)
        return;

    audioPort.postMessage({
        type: "config",
        allowed: ready && !!state && state.inGame && !state.muted,
        pushToTalk: !!(state && state.pushToTalk),
    });
}

function connect() {
    clearTimeout(reconnectTimer);
    reconnectTimer = 0;
    if (stopped)
        return;

    if (preferredCandidate >= 0)
        candidateIndex = preferredCandidate;

    const index = candidateIndex;
    let opened = false;
    const ws = new WebSocket(candidates[index]);
    ws.binaryType = "arraybuffer";
    socket = ws;
    ready = false;
    self.postMessage({ type: "connecting" });

    ws.onopen = () => {
        opened = true;
        ws.send(JSON.stringify({ t: "auth", token }));
    };

    ws.onmessage = event => {
        if (typeof event.data !== "string")
            return;

        let message;
        try {
            message = JSON.parse(event.data);
        } catch {
            return;
        }

        if (message.t === "auth_ok") {
            ready = true;
            preferredCandidate = index;
            reconnectAttempt = 0;
            pushTransmit();
            self.postMessage({ type: "ready" });
        } else if (message.t === "state") {
            state = message;
            pushTransmit();
            self.postMessage({ type: "state", state });
        }
    };

    ws.onclose = event => {
        if (socket !== ws)
            return;

        socket = null;
        ready = false;
        state = null;
        pushTransmit();

        if (event.code === CLOSE_INVALID_TOKEN || event.code === CLOSE_REPLACED) {
            stopped = true;
            self.postMessage({ type: "stopped", code: event.code });
            return;
        }

        if (!opened && preferredCandidate < 0 && index + 1 < candidates.length) {
            candidateIndex = index + 1;
            connect();
            return;
        }

        if (!opened && preferredCandidate < 0)
            candidateIndex = 0;

        const delay = RECONNECT_DELAYS[Math.min(reconnectAttempt, RECONNECT_DELAYS.length - 1)];
        reconnectAttempt++;
        reconnectTimer = setTimeout(connect, delay);
        self.postMessage({ type: "retrying", delay });
    };
}

function sendFrame(flags, payload) {
    if (!socket || !ready || socket.bufferedAmount > MAX_BUFFERED_BYTES)
        return;

    const bytes = new Uint8Array(3 + payload.byteLength);
    bytes[0] = sequence & 0xff;
    bytes[1] = (sequence >> 8) & 0xff;
    bytes[2] = flags;
    bytes.set(new Uint8Array(payload), 3);
    sequence = (sequence + 1) & 0xffff;
    socket.send(bytes.buffer);
}
