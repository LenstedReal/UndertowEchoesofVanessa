export class GameAudio {
  private ctx: AudioContext | null = null
  private master: GainNode | null = null
  private wind: AudioBufferSourceNode | null = null
  private windGain: GainNode | null = null
  private drone: OscillatorNode | null = null
  private droneGain: GainNode | null = null
  volume = 0.8

  unlock() {
    const AC = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext
    if (!this.ctx) {
      const ctx = new AC({ latencyHint: "interactive" })
      const master = ctx.createGain()
      master.gain.value = this.volume * this.volume
      master.connect(ctx.destination)
      const windGain = ctx.createGain()
      windGain.gain.value = 0.0
      windGain.connect(master)
      const len = ctx.sampleRate * 2
      const buf = ctx.createBuffer(1, len, ctx.sampleRate)
      const data = buf.getChannelData(0)
      let last = 0
      for (let i = 0; i < len; i++) {
        const white = Math.random() * 2 - 1
        last = last * 0.96 + white * 0.04
        data[i] = last * 2.2
      }
      const src = ctx.createBufferSource()
      src.buffer = buf
      src.loop = true
      const filter = ctx.createBiquadFilter()
      filter.type = "lowpass"
      filter.frequency.value = 400
      src.connect(filter)
      filter.connect(windGain)
      src.start()
      const drone = ctx.createOscillator()
      drone.type = "sine"
      drone.frequency.value = 55
      const droneGain = ctx.createGain()
      droneGain.gain.value = 0
      drone.connect(droneGain)
      droneGain.connect(master)
      drone.start()
      this.ctx = ctx
      this.master = master
      this.wind = src
      this.windGain = windGain
      this.drone = drone
      this.droneGain = droneGain
    }
    if (this.ctx.state === "suspended") void this.ctx.resume()
  }

  setVolume(v: number) {
    this.volume = v
    if (!this.ctx || !this.master) return
    this.master.gain.setTargetAtTime(v * v, this.ctx.currentTime, 0.05)
  }

  setWind(amount: number) {
    if (!this.ctx || !this.windGain) return
    this.windGain.gain.setTargetAtTime(amount * 0.25, this.ctx.currentTime, 0.1)
  }

  setDrone(amount: number) {
    if (!this.ctx || !this.droneGain) return
    this.droneGain.gain.setTargetAtTime(amount * 0.08, this.ctx.currentTime, 0.2)
  }

  tone(freq: number, dur = 0.45, type: OscillatorType = "sine", gain = 0.12) {
    if (!this.ctx || !this.master) return
    const t = this.ctx.currentTime
    const osc = this.ctx.createOscillator()
    const g = this.ctx.createGain()
    osc.type = type
    osc.frequency.value = freq
    g.gain.setValueAtTime(0.0001, t)
    g.gain.exponentialRampToValueAtTime(gain, t + 0.02)
    g.gain.exponentialRampToValueAtTime(0.0001, t + dur)
    osc.connect(g)
    g.connect(this.master)
    osc.start(t)
    osc.stop(t + dur + 0.02)
  }

  step() {
    this.tone(90 + Math.random() * 30, 0.08, "triangle", 0.05)
  }

  creak() {
    this.tone(140 + Math.random() * 40, 0.25, "sawtooth", 0.03)
  }

  wrong() {
    this.tone(110, 0.2, "square", 0.05)
  }

  chime() {
    this.tone(523, 0.3, "sine", 0.08)
    window.setTimeout(() => this.tone(659, 0.4, "sine", 0.08), 140)
  }

  dispose() {
    this.wind?.stop()
    this.drone?.stop()
    void this.ctx?.close()
    this.ctx = null
  }
}
