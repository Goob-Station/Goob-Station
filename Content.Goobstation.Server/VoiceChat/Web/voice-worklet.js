const OUTPUT_RATE = 16000;
const FRAME_SAMPLES = 320;
const FRAME_BYTES = 3 + FRAME_SAMPLES / 2;
const PREROLL_FRAMES = 5;
const VAD_HANGOVER_FRAMES = 20;
const LEVEL_REPORT_FRAMES = 3;
const FLAG_TRANSMISSION_START = 1;

const INDEX_TABLE = [-1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8];
const STEP_TABLE = [
    7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
    19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
    50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
    130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
    337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
    876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
    2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
    5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
    15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767,
];

class Resampler {
    constructor(inputRate, outputRate) {
        this.passthrough = inputRate === outputRate;
        this.step = inputRate / outputRate;
        this.buffer = new Float32Array(8192);
        this.length = 0;
        this.position = 0;

        if (this.passthrough)
            return;

        const cutoff = Math.min(0.5, 0.45 / this.step);
        this.halfWidth = Math.ceil(8 / (2 * cutoff));
        this.resolution = 64;
        this.table = new Float32Array(this.halfWidth * this.resolution + 2);
        for (let i = 0; i < this.table.length; i++) {
            const x = i / this.resolution;
            if (x >= this.halfWidth)
                continue;

            const arg = 2 * cutoff * x;
            const sinc = arg === 0 ? 1 : Math.sin(Math.PI * arg) / (Math.PI * arg);
            const u = x / this.halfWidth;
            const window = 0.42 + 0.5 * Math.cos(Math.PI * u) + 0.08 * Math.cos(2 * Math.PI * u);
            this.table[i] = 2 * cutoff * sinc * window;
        }

        this.position = this.halfWidth;
    }

    kernel(x) {
        const ax = Math.abs(x) * this.resolution;
        const index = ax | 0;
        if (index + 1 >= this.table.length)
            return 0;

        const frac = ax - index;
        return this.table[index] + (this.table[index + 1] - this.table[index]) * frac;
    }

    push(input, emit) {
        if (this.passthrough) {
            for (let i = 0; i < input.length; i++)
                emit(input[i]);
            return;
        }

        if (this.length + input.length > this.buffer.length) {
            const grown = new Float32Array((this.length + input.length) * 2);
            grown.set(this.buffer.subarray(0, this.length));
            this.buffer = grown;
        }

        this.buffer.set(input, this.length);
        this.length += input.length;

        while (this.position + this.halfWidth < this.length) {
            const center = Math.floor(this.position);
            const frac = this.position - center;
            let sum = 0;
            for (let k = -this.halfWidth + 1; k <= this.halfWidth; k++)
                sum += this.buffer[center + k] * this.kernel(k - frac);

            emit(sum);
            this.position += this.step;
        }

        const discard = Math.floor(this.position) - this.halfWidth;
        if (discard > 0) {
            this.buffer.copyWithin(0, discard, this.length);
            this.length -= discard;
            this.position -= discard;
        }
    }
}

class AdpcmEncoder {
    constructor() {
        this.predictor = 0;
        this.index = 0;
    }

    encode(samples) {
        const out = new Uint8Array(FRAME_BYTES);
        out[0] = this.predictor & 0xff;
        out[1] = (this.predictor >> 8) & 0xff;
        out[2] = this.index;

        for (let i = 0; i < samples.length; i++) {
            let step = STEP_TABLE[this.index];
            let diff = samples[i] - this.predictor;
            let nibble = 0;
            if (diff < 0) {
                nibble = 8;
                diff = -diff;
            }

            let delta = step >> 3;
            if (diff >= step) {
                nibble |= 4;
                diff -= step;
                delta += step;
            }
            step >>= 1;
            if (diff >= step) {
                nibble |= 2;
                diff -= step;
                delta += step;
            }
            step >>= 1;
            if (diff >= step) {
                nibble |= 1;
                delta += step;
            }

            this.predictor += (nibble & 8) ? -delta : delta;
            if (this.predictor > 32767)
                this.predictor = 32767;
            else if (this.predictor < -32768)
                this.predictor = -32768;

            this.index += INDEX_TABLE[nibble];
            if (this.index < 0)
                this.index = 0;
            else if (this.index > 88)
                this.index = 88;

            const byte = 3 + (i >> 1);
            out[byte] |= (i & 1) === 0 ? nibble : nibble << 4;
        }

        return out;
    }
}

class VoiceCaptureProcessor extends AudioWorkletProcessor {
    constructor() {
        super();
        this.resampler = new Resampler(sampleRate, OUTPUT_RATE);
        this.encoder = new AdpcmEncoder();
        this.frame = new Float32Array(FRAME_SAMPLES);
        this.pcm = new Int16Array(FRAME_SAMPLES);
        this.frameLength = 0;
        this.preroll = [];
        this.transmitting = false;
        this.vadOpen = false;
        this.vadHang = 0;
        this.levelCounter = 0;
        this.peakDb = -100;
        this.mode = "ptt";
        this.threshold = -45;
        this.gain = 1;
        this.muted = false;
        this.allowed = false;
        this.pushToTalk = false;
        this.emit = this.emit.bind(this);
        this.port.onmessage = event => this.configure(event.data);
    }

    configure(message) {
        if (message.type !== "config")
            return;

        for (const key of ["mode", "threshold", "gain", "muted", "allowed", "pushToTalk"]) {
            if (key in message)
                this[key] = message[key];
        }
    }

    emit(sample) {
        this.frame[this.frameLength++] = sample;
        if (this.frameLength === FRAME_SAMPLES) {
            this.frameLength = 0;
            this.processFrame();
        }
    }

    processFrame() {
        let energy = 0;
        for (let i = 0; i < FRAME_SAMPLES; i++) {
            let value = this.frame[i] * this.gain;
            if (value > 1)
                value = 1;
            else if (value < -1)
                value = -1;

            energy += value * value;
            this.pcm[i] = Math.round(value * 32767);
        }

        const rms = Math.sqrt(energy / FRAME_SAMPLES);
        const db = rms > 0 ? 20 * Math.log10(rms) : -100;
        this.peakDb = Math.max(this.peakDb, db);

        if (db >= this.threshold) {
            this.vadOpen = true;
            this.vadHang = VAD_HANGOVER_FRAMES;
        } else if (this.vadHang > 0) {
            this.vadHang--;
        } else {
            this.vadOpen = false;
        }

        const encoded = this.encoder.encode(this.pcm);
        const wanted = this.mode === "vad" ? this.vadOpen : this.pushToTalk;
        const transmitting = wanted && this.allowed && !this.muted;

        if (transmitting) {
            if (!this.transmitting) {
                let first = true;
                for (const frame of this.preroll) {
                    this.send(frame, first ? FLAG_TRANSMISSION_START : 0);
                    first = false;
                }
                this.preroll = [];
                this.send(encoded, first ? FLAG_TRANSMISSION_START : 0);
            } else {
                this.send(encoded, 0);
            }
        } else {
            this.preroll.push(encoded);
            if (this.preroll.length > PREROLL_FRAMES)
                this.preroll.shift();
        }

        if (transmitting !== this.transmitting) {
            this.transmitting = transmitting;
            this.reportLevel();
        }

        if (++this.levelCounter >= LEVEL_REPORT_FRAMES)
            this.reportLevel();
    }

    reportLevel() {
        this.levelCounter = 0;
        this.port.postMessage({
            type: "level",
            db: this.peakDb,
            transmitting: this.transmitting,
            voice: this.vadOpen,
        });
        this.peakDb = -100;
    }

    send(payload, flags) {
        this.port.postMessage({ type: "frame", flags, payload: payload.buffer }, [payload.buffer]);
    }

    process(inputs) {
        const input = inputs[0];
        if (input && input.length > 0 && input[0].length > 0)
            this.resampler.push(input[0], this.emit);

        return true;
    }
}

registerProcessor("voice-capture", VoiceCaptureProcessor);
