import { useEffect, useRef, useState, type ReactNode } from "react"
import { BookOpen, Flame, Hand, Pause, Settings, Wind } from "lucide-react"
import type { GameApi, Hud } from "@/game/engine"
import act1 from "@/assets/act1.jpg"

const initial: Hud = {
  mode: "menu",
  settingsFrom: "menu",
  act: 1,
  actName: "Lanetli Kalyon",
  objective: "",
  prompt: null,
  oil: 64,
  lanternOn: true,
  subtitle: null,
  hasSave: false,
  reading: null,
  logs: [],
  cardTitle: "",
  cardLine: "",
  quality: "medium",
  sens: 1,
  volume: 0.75,
  subtitles: true,
  motion: true,
  haptics: false,
  webgl: true,
}

export function HorrorGame() {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const api = useRef<GameApi | null>(null)
  const [hud, setHud] = useState<Hud>(initial)
  const [knob, setKnob] = useState({ x: 0, y: 0 })
  const stick = useRef<{ id: number; x: number; y: number } | null>(null)
  const look = useRef<{ id: number; x: number; y: number } | null>(null)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return
    let dead = false
    let local: GameApi | null = null
    void import("@/game/engine").then(({ createGame }) => {
      if (dead || !canvasRef.current) return
      try {
        local = createGame(canvasRef.current, setHud)
        api.current = local
      } catch (err) {
        setHud((h) => ({ ...h, webgl: false, subtitle: err instanceof Error ? err.message : "3D başlatılamadı" }))
      }
    })
    const onPop = () => {
      api.current?.back()
      history.pushState({ undertow: 1 }, "")
    }
    history.pushState({ undertow: 1 }, "")
    window.addEventListener("popstate", onPop)
    return () => {
      dead = true
      window.removeEventListener("popstate", onPop)
      local?.dispose()
      api.current = null
    }
  }, [])

  const playing = hud.mode === "playing"

  return (
    <main className="fixed inset-0 overflow-hidden bg-abyss font-sans text-foam">
      <img src={act1} alt="" className={`pointer-events-none absolute inset-0 h-full w-full object-cover ${hud.webgl ? "opacity-0" : "opacity-80"}`} />
      <canvas ref={canvasRef} className="absolute inset-0 h-full w-full touch-none" aria-label="Undertow oyun sahnesi" />
      <div className="grain pointer-events-none absolute inset-0" />
      {playing && (
        <div
          className="absolute inset-0 z-10 touch-none"
          onPointerDown={(e) => {
            if (e.pointerType === "mouse" && e.button === 0) api.current?.lock()
            look.current = { id: e.pointerId, x: e.clientX, y: e.clientY }
            e.currentTarget.setPointerCapture(e.pointerId)
          }}
          onPointerMove={(e) => {
            if (!look.current || look.current.id !== e.pointerId) return
            const dx = e.clientX - look.current.x
            const dy = e.clientY - look.current.y
            look.current = { id: e.pointerId, x: e.clientX, y: e.clientY }
            if (document.pointerLockElement) return
            api.current?.addLook(dx, dy)
          }}
          onPointerUp={(e) => {
            if (look.current?.id === e.pointerId) look.current = null
          }}
        />
      )}

      {hud.mode === "menu" && (
        <section className="safe-pad absolute inset-0 z-30 flex flex-col justify-between bg-linear-to-r from-ink/88 via-ink/35 to-transparent p-5 md:max-w-xl">
          <div>
            <p className="text-xs tracking-[0.28em] text-moon uppercase">Echoes of Vanessa</p>
            <h1 className="hero-title mt-2 text-foam">Undertow</h1>
            <p className="mt-3 max-w-sm text-sm leading-relaxed text-fog">
              Dört perde. Fener, sis, Vanessa’nın ninnisi. Zıplama korkusu yok; karanlık ve yanlış hatıra var.
            </p>
          </div>
          <div className="flex flex-col gap-2">
            <button type="button" className="min-h-12 rounded-full bg-lantern px-5 font-medium text-ink" onClick={() => api.current?.newGame()}>
              Yeni oyun
            </button>
            {hud.hasSave && (
              <button type="button" className="min-h-12 rounded-full border border-fog/50 bg-abyss/80 px-5" onClick={() => api.current?.continueGame()}>
                Devam et
              </button>
            )}
            <div className="flex gap-2">
              <button type="button" className="min-h-12 flex-1 rounded-full border border-fog/40 px-4" onClick={() => api.current?.openSettings("menu")}>
                <Settings className="mr-2 inline size-4" aria-hidden />
                Ayarlar
              </button>
              <button type="button" className="min-h-12 flex-1 rounded-full border border-fog/40 px-4" onClick={() => api.current?.openLog()}>
                <BookOpen className="mr-2 inline size-4" aria-hidden />
                Günlük
              </button>
            </div>
            <p className="text-xs leading-relaxed text-fog">
              Sol başparmak yürü. Sağda sürükle, bak. Fener yağ yakar. Denize uzun bakma.
            </p>
            <p className="text-[0.7rem] tracking-wide text-moon">© Lenstedreal · All Rights Reserved.</p>
          </div>
        </section>
      )}

      {playing && (
        <div className="pointer-events-none absolute inset-x-0 top-0 z-20 p-4 pt-[max(0.75rem,env(safe-area-inset-top))]">
          <p className="text-[0.65rem] tracking-[0.22em] text-moon uppercase">
            Perde {hud.act} · {hud.actName}
          </p>
          <p className="mt-1 max-w-md text-sm text-foam">{hud.objective}</p>
        </div>
      )}

      {hud.subtitle && playing && (
        <p className="pointer-events-none absolute inset-x-6 bottom-36 z-20 text-center text-sm text-foam md:bottom-28">{hud.subtitle}</p>
      )}
      {hud.prompt && playing && !hud.reading && (
        <p className="pointer-events-none absolute inset-x-0 bottom-28 z-20 text-center text-sm text-lantern md:bottom-20">{hud.prompt}</p>
      )}

      {playing && (
        <>
          <div
            className="absolute bottom-4 left-4 z-30 size-28 touch-none rounded-full border border-moon/40 bg-ink/45 safe-pad"
            onPointerDown={(e) => {
              e.stopPropagation()
              stick.current = { id: e.pointerId, x: e.clientX, y: e.clientY }
              e.currentTarget.setPointerCapture(e.pointerId)
            }}
            onPointerMove={(e) => {
              if (!stick.current || stick.current.id !== e.pointerId) return
              const dx = e.clientX - stick.current.x
              const dy = e.clientY - stick.current.y
              const mag = Math.hypot(dx, dy) || 1
              const cap = 42
              const k = Math.min(cap, mag) / mag
              setKnob({ x: dx * k, y: dy * k })
              api.current?.setStick(Math.max(-1, Math.min(1, dx / cap)), Math.max(-1, Math.min(1, -dy / cap)))
            }}
            onPointerUp={(e) => {
              if (stick.current?.id !== e.pointerId) return
              stick.current = null
              setKnob({ x: 0, y: 0 })
              api.current?.setStick(0, 0)
            }}
            aria-label="Hareket"
          >
            <span
              className="absolute top-1/2 left-1/2 size-12 -translate-x-1/2 -translate-y-1/2 rounded-full bg-moon/80"
              style={{ marginLeft: knob.x, marginTop: knob.y }}
            />
          </div>
          <div className="absolute right-3 bottom-4 z-30 flex items-end gap-2">
            <button
              type="button"
              className="grid size-14 place-items-center rounded-full border border-fog/40 bg-ink/70"
              aria-label="Koş"
              onPointerDown={(e) => {
                e.stopPropagation()
                api.current?.setSprint(true)
              }}
              onPointerUp={() => api.current?.setSprint(false)}
              onPointerLeave={() => api.current?.setSprint(false)}
            >
              <Wind className="size-5" />
            </button>
            <button
              type="button"
              className="grid size-16 place-items-center rounded-full bg-lantern text-ink"
              onClick={() => api.current?.interact()}
              aria-label="Etkileşim"
            >
              <Hand className="size-6" />
            </button>
            <button
              type="button"
              className={`grid size-14 place-items-center rounded-full border ${hud.lanternOn ? "border-lantern bg-lantern/20 text-lantern" : "border-fog/40 bg-ink/70"}`}
              onClick={() => api.current?.toggleLantern()}
              aria-label="Fener"
            >
              <Flame className="size-5" />
            </button>
            <button
              type="button"
              className="grid size-12 place-items-center rounded-full border border-fog/40 bg-ink/70"
              onClick={() => api.current?.pause()}
              aria-label="Duraklat"
            >
              <Pause className="size-4" />
            </button>
          </div>
          <div className="absolute top-4 right-4 z-20 h-16 w-2 overflow-hidden rounded-full bg-ink/70" aria-label="Yağ">
            <span className="absolute bottom-0 w-full bg-lantern" style={{ height: `${hud.oil}%` }} />
          </div>
        </>
      )}

      {hud.reading && (
        <section className="absolute inset-x-0 bottom-0 z-40 max-h-[55%] overflow-auto rounded-t-3xl bg-ink/95 p-5 safe-pad">
          <h2 className="font-display text-2xl text-lantern">{hud.reading.title}</h2>
          <p className="mt-3 text-sm leading-relaxed text-foam">{hud.reading.body}</p>
          <button type="button" className="mt-4 min-h-12 rounded-full bg-lantern px-5 text-ink" onClick={() => api.current?.closeReading()}>
            Kapat
          </button>
        </section>
      )}

      {hud.mode === "paused" && (
        <Panel title="Duraklatıldı">
          <button type="button" className="btn-main" onClick={() => api.current?.resume()}>
            Devam
          </button>
          <button type="button" className="btn-ghost" onClick={() => api.current?.rescue()}>
            Son güvenli nokta
          </button>
          <button type="button" className="btn-ghost" onClick={() => api.current?.openLog()}>
            Günlük
          </button>
          <button type="button" className="btn-ghost" onClick={() => api.current?.openSettings("paused")}>
            Ayarlar
          </button>
          <button type="button" className="btn-ghost" onClick={() => api.current?.quitToMenu()}>
            Ana menü
          </button>
        </Panel>
      )}

      {hud.mode === "settings" && (
        <Panel title="Ayarlar">
          <label className="block text-sm text-fog">
            Bakış hassasiyeti
            <input
              className="mt-1 w-full"
              type="range"
              min={0.45}
              max={2}
              step={0.05}
              value={hud.sens}
              onChange={(e) => api.current?.setSetting({ sens: Number(e.target.value) })}
            />
          </label>
          <label className="mt-3 block text-sm text-fog">
            Ses
            <input
              className="mt-1 w-full"
              type="range"
              min={0}
              max={1}
              step={0.05}
              value={hud.volume}
              onChange={(e) => api.current?.setSetting({ volume: Number(e.target.value) })}
            />
          </label>
          <div className="mt-3 flex flex-wrap gap-2">
            {(["low", "medium", "high"] as const).map((q) => (
              <button key={q} type="button" className={`btn-ghost ${hud.quality === q ? "border-lantern text-lantern" : ""}`} onClick={() => api.current?.setSetting({ quality: q })}>
                {q === "low" ? "Düşük" : q === "medium" ? "Orta" : "Yüksek"}
              </button>
            ))}
          </div>
          <label className="mt-3 flex items-center gap-2 text-sm">
            <input type="checkbox" checked={hud.subtitles} onChange={(e) => api.current?.setSetting({ subtitles: e.target.checked })} />
            Altyazı
          </label>
          <label className="mt-2 flex items-center gap-2 text-sm">
            <input type="checkbox" checked={!hud.motion} onChange={(e) => api.current?.setSetting({ motion: !e.target.checked })} />
            Hareket azalt
          </label>
          <button type="button" className="btn-main mt-4" onClick={() => api.current?.closeSettings()}>
            Kapat
          </button>
        </Panel>
      )}

      {hud.mode === "log" && (
        <Panel title="Günlük">
          <div className="max-h-64 space-y-3 overflow-auto">
            {hud.logs.length === 0 && <p className="text-sm text-fog">Henüz yazı yok.</p>}
            {hud.logs.map((l) => (
              <article key={l.id}>
                <h3 className="text-lantern">{l.title}</h3>
                <p className="text-sm leading-relaxed text-fog">{l.body}</p>
              </article>
            ))}
          </div>
          <button type="button" className="btn-main mt-4" onClick={() => api.current?.closeLog()}>
            Kapat
          </button>
        </Panel>
      )}

      {hud.mode === "card" && (
        <section className="absolute inset-0 z-40 flex flex-col items-center justify-center bg-ink/80 p-6 text-center">
          <p className="text-xs tracking-[0.28em] text-moon uppercase">{hud.cardTitle}</p>
          <h2 className="hero-title mt-3">{hud.actName}</h2>
          <p className="mt-4 max-w-md text-sm leading-relaxed text-fog">{hud.cardLine}</p>
          <button type="button" className="btn-main mt-6" onClick={() => api.current?.dismissCard()}>
            Devam
          </button>
        </section>
      )}

      {hud.mode === "ending" && (
        <section className="absolute inset-0 z-40 flex flex-col justify-end bg-ink/88 p-6 safe-pad">
          <p className="text-xs tracking-[0.28em] text-moon uppercase">Son</p>
          <h2 className="hero-title mt-2">Vanessa</h2>
          <p className="mt-4 max-w-xl text-sm leading-relaxed text-foam">
            Ninni bitti. Kolye sunağa bırakıldı. Kasaba saati 11:40’ta donmuştu; gemi 02:17’de sis aldı, vardiyası 06:05’te kapanmıştı. Vanessa güvertede kaldı ki diğerleri filikaya binsin. Uzaktaki gölge geçti, vurmadı. Fener bir kez daha düz yandı.
          </p>
          <p className="mt-3 text-sm text-fog">Mezarlık susuyor. Su seni bırakıyor.</p>
          <div className="mt-5 flex gap-2">
            <button type="button" className="btn-main" onClick={() => api.current?.newGame()}>
              Baştan
            </button>
            <button type="button" className="btn-ghost" onClick={() => api.current?.quitToMenu()}>
              Menü
            </button>
          </div>
          <p className="mt-4 text-[0.7rem] text-moon">© Lenstedreal · All Rights Reserved.</p>
        </section>
      )}

      {!hud.webgl && <p className="absolute bottom-4 left-4 z-50 text-sm text-lantern">Bu cihazda 3D görüntü açılamadı.</p>}
    </main>
  )
}

function Panel({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="absolute inset-0 z-40 flex items-end justify-center bg-ink/75 p-4 safe-pad md:items-center">
      <div className="w-full max-w-md rounded-3xl border border-fog/30 bg-abyss/95 p-5">
        <h2 className="font-display text-3xl text-foam">{title}</h2>
        <div className="mt-4 flex flex-col gap-2">{children}</div>
      </div>
    </section>
  )
}
