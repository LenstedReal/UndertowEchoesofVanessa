// @ts-nocheck
import * as THREE from "three"
import { mergeGeometries } from "three/addons/utils/BufferGeometryUtils.js"
import {
  A1_NAMES,
  A1_NOTES,
  A1_SEQ,
  A2_NAMES,
  A2_NOTES,
  A2_SEQ,
  ACTS,
  BALANCE,
  BLOCKERS,
  GOALS,
  ITEMS,
  ROOMS,
  SPAWN,
  roomContains,
  type ActId,
  type Item,
  type Room,
} from "@/game/content"
import { GameAudio } from "@/game/audio"
import {
  clearSave,
  loadSave,
  loadSettings,
  writeSave,
  writeSettings,
  type LogEntry,
  type Quality,
  type SaveData,
  type Settings,
  type Tier,
} from "@/game/save"

export type HudMode = "menu" | "playing" | "paused" | "ending" | "settings" | "log" | "card"

export type Hud = {
  mode: HudMode
  settingsFrom: "menu" | "paused"
  act: ActId
  actName: string
  objective: string
  prompt: string | null
  oil: number
  lanternOn: boolean
  subtitle: string | null
  hasSave: boolean
  reading: { title: string; body: string } | null
  logs: LogEntry[]
  cardTitle: string
  cardLine: string
  quality: Quality
  sens: number
  volume: number
  subtitles: boolean
  motion: boolean
  haptics: boolean
  webgl: boolean
}

export type GameApi = {
  dispose: () => void
  setStick: (x: number, y: number) => void
  setSprint: (v: boolean) => void
  addLook: (dx: number, dy: number) => void
  interact: () => void
  toggleLantern: () => void
  pause: () => void
  resume: () => void
  newGame: () => void
  continueGame: () => void
  back: () => void
  openSettings: (from: "menu" | "paused") => void
  closeSettings: () => void
  setSetting: (patch: Partial<Settings>) => void
  closeReading: () => void
  openLog: () => void
  closeLog: () => void
  quitToMenu: () => void
  dismissCard: () => void
  rescue: () => void
  lock: () => void
}

declare global {
  interface Window {
    __controlsTest?: {
      getYaw: () => number
      getSpeed: () => number
      getPos: () => { x: number; z: number }
      setKeys: (codes: string[]) => void
    }
  }
}

const R = 0.3

function updateTier(sanity: number, prev: Tier): Tier {
  if (prev === "stable") return sanity < 70 - BALANCE.hysteresis ? "uneasy" : "stable"
  if (prev === "uneasy") {
    if (sanity >= 70 + BALANCE.hysteresis) return "stable"
    if (sanity < 45 - BALANCE.hysteresis) return "disturbed"
    return "uneasy"
  }
  if (prev === "disturbed") {
    if (sanity >= 45 + BALANCE.hysteresis) return "uneasy"
    if (sanity < 20 - BALANCE.hysteresis) return "breaking"
    return "disturbed"
  }
  return sanity >= 20 + BALANCE.hysteresis ? "disturbed" : "breaking"
}

function woodTexture() {
  const c = document.createElement("canvas")
  c.width = 512
  c.height = 512
  const g = c.getContext("2d")
  if (!g) return null
  g.fillStyle = "#1c140f"
  g.fillRect(0, 0, 512, 512)
  for (let i = 0; i < 18; i++) {
    const y = i * 28
    const shade = 18 + ((i * 17) % 22)
    g.fillStyle = `rgb(${shade + 16},${shade},${shade - 6})`
    g.fillRect(0, y, 512, 26)
    g.strokeStyle = "rgba(0,0,0,0.45)"
    g.strokeRect(0, y, 512, 26)
    for (let k = 0; k < 8; k++) {
      g.strokeStyle = `rgba(40,28,18,${0.15 + (k % 3) * 0.08})`
      g.beginPath()
      g.moveTo(0, y + 4 + k * 2.4)
      g.lineTo(512, y + 6 + k * 2.1)
      g.stroke()
    }
    g.fillStyle = "rgba(0,0,0,0.35)"
    g.fillRect((i * 47) % 460, y + 8, 3, 3)
  }
  const tex = new THREE.CanvasTexture(c)
  tex.wrapS = THREE.RepeatWrapping
  tex.wrapT = THREE.RepeatWrapping
  tex.repeat.set(2, 2)
  tex.colorSpace = THREE.SRGBColorSpace
  return tex
}

export function createGame(canvas: HTMLCanvasElement, onHud: (h: Hud) => void): GameApi {
  const audio = new GameAudio()
  const settings = loadSettings()
  audio.volume = settings.volume

  const flags = new Set<string>()
  const crates: Record<string, number> = {}
  const clocks: Record<string, number> = {}
  const mirrors: Record<string, number> = {}
  const logs: LogEntry[] = []

  let mode: HudMode = "menu"
  let settingsFrom: "menu" | "paused" = "menu"
  let act: ActId = 1
  let x = SPAWN[1].x
  let z = SPAWN[1].z
  let yaw = Math.PI
  let pitch = -0.08
  let sanity = 100
  let tier: Tier = "stable"
  let oil = BALANCE.startOil
  let lanternOn = true
  let reading: { title: string; body: string } | null = null
  let subtitle: string | null = null
  let subT = 0
  let cardTitle = ""
  let cardLine = ""
  let webgl = true
  let roomName = ""
  let seaT = 0
  let stableHold = 0
  let a1p = 0
  let a2p = 0
  let stepAcc = 0
  let saveAcc = 0
  let distortAcc = 5
  let portraitOff = 0
  let safeX = x
  let safeZ = z
  let safeYaw = yaw
  let horizSpeed = 0
  let stickX = 0
  let stickY = 0
  let sprintHeld = false
  let lookX = 0
  let lookY = 0
  let probe: Set<string> | null = null
  const keys = new Set<string>()
  let menuT = 0
  let warnedWind = false
  let warnedOil = false
  let warnedSea = false

  const renderer = new THREE.WebGLRenderer({ canvas, antialias: false, powerPreference: "high-performance" })
  renderer.setClearColor(0x4e5d62, 1)
  renderer.outputColorSpace = THREE.SRGBColorSpace
  renderer.toneMapping = THREE.ACESFilmicToneMapping
  renderer.toneMappingExposure = 0.78
  const scene = new THREE.Scene()
  const camera = new THREE.PerspectiveCamera(72, 1, 0.08, 90)
  camera.rotation.order = "YXZ"
  const clock = new THREE.Timer()
  clock.connect(document)

  const hemi = new THREE.HemisphereLight(0x86a0c8, 0x0b141a, 0.22)
  const moon = new THREE.DirectionalLight(0x9eb4d4, 0.72)
  moon.position.set(-28, 36, -12)
  const lamp = new THREE.PointLight(0xff9227, 0, 12, 2)
  scene.add(hemi, moon, lamp)

  const fog = new THREE.FogExp2(0x4e5d62, 0.05)
  scene.fog = fog
  scene.background = new THREE.Color(0x4e5d62)

  const map = woodTexture()
  const wood = new THREE.MeshStandardMaterial({ map: map ?? undefined, color: map ? 0xffffff : 0x3a2a22, roughness: 0.86, metalness: 0.04 })
  const wet = new THREE.MeshStandardMaterial({ map: map ?? undefined, color: 0x5c4638, roughness: 0.55, metalness: 0.18 })
  const stone = new THREE.MeshStandardMaterial({ color: 0x1c272c, roughness: 0.92, metalness: 0.04 })
  const iron = new THREE.MeshStandardMaterial({ color: 0x1a2226, roughness: 0.4, metalness: 0.8 })
  const brass = new THREE.MeshStandardMaterial({ color: 0x8a5a28, roughness: 0.35, metalness: 0.75 })
  const ember = new THREE.MeshStandardMaterial({ color: 0xff9227, emissive: 0xff6a00, emissiveIntensity: 0.8, roughness: 0.4 })
  const cloth = new THREE.MeshStandardMaterial({ color: 0x1c2428, roughness: 1, side: THREE.DoubleSide })

  const level = new THREE.Group()
  scene.add(level)
  const roomRoots = new Map<string, THREE.Group>()
  const itemMeshes = new Map<string, THREE.Object3D>()
  const blockerMeshes = new Map<string, THREE.Object3D>()
  let whale: THREE.Object3D | null = null
  let waterMat: THREE.ShaderMaterial | null = null

  const viewScene = new THREE.Scene()
  const viewCam = new THREE.PerspectiveCamera(42, 1, 0.05, 4)
  const lantern = new THREE.Group()
  const cage = new THREE.Mesh(new THREE.CylinderGeometry(0.07, 0.08, 0.16, 8), iron)
  const glass = new THREE.Mesh(
    new THREE.CylinderGeometry(0.055, 0.06, 0.12, 8),
    new THREE.MeshStandardMaterial({ color: 0xffe1b0, transparent: true, opacity: 0.28, roughness: 0.1 }),
  )
  const flame = new THREE.Mesh(new THREE.ConeGeometry(0.02, 0.06, 6), ember)
  flame.position.y = 0.02
  const handle = new THREE.Mesh(new THREE.TorusGeometry(0.05, 0.008, 6, 10), brass)
  handle.position.y = 0.12
  lantern.add(cage, glass, flame, handle)
  lantern.position.set(0.34, -0.34, -0.58)
  viewScene.add(lantern)
  viewScene.add(new THREE.HemisphereLight(0xff9227, 0x1a1008, 0.8))

  const goal = new THREE.Mesh(
    new THREE.SphereGeometry(0.08, 8, 8),
    new THREE.MeshBasicMaterial({ color: 0xff9227, transparent: true, opacity: 0.85 }),
  )
  scene.add(goal)

  const portrait = new THREE.Mesh(
    new THREE.PlaneGeometry(0.8, 1.1),
    new THREE.MeshStandardMaterial({ color: 0x4a3b34, roughness: 0.7 }),
  )
  portrait.position.set(1.15, 1.4, 28)
  portrait.rotation.y = Math.PI / 2
  scene.add(portrait)

  function matFor(room: Room) {
    if (room.kind === "street" || room.kind === "temple") return stone
    if (room.kind === "deck" || room.kind === "grave") return wet
    return wood
  }

  function segInsideOther(px: number, pz: number, self: Room, rooms: Room[]) {
    return rooms.some((r) => r !== self && r.act === self.act && roomContains(r, px, pz))
  }

  function buildAct(next: ActId) {
    for (const child of [...level.children]) {
      level.remove(child)
      child.traverse((obj: THREE.Object3D) => {
        const mesh = obj as THREE.Mesh
        if (mesh.geometry && mesh.userData.ownGeo) mesh.geometry.dispose()
      })
    }
    roomRoots.clear()
    itemMeshes.clear()
    blockerMeshes.clear()
    whale = null
    const rooms = ROOMS.filter((r) => r.act === next)
    for (const room of rooms) {
      const parts: THREE.BufferGeometry[] = []
      const y0 = room.floor
      const y1 = room.floorTo ?? room.floor
      const midY = (y0 + y1) / 2
      const floor = new THREE.BoxGeometry(room.w, 0.28, room.d)
      if (room.floorTo !== undefined) {
        const rise = room.floorTo - room.floor
        floor.rotateX(-Math.atan2(rise, room.d))
      }
      floor.translate(room.x + room.w / 2, midY - 0.14, room.z + room.d / 2)
      parts.push(floor)
      if (room.ceil) {
        const ceil = new THREE.BoxGeometry(room.w, 0.18, room.d)
        ceil.translate(room.x + room.w / 2, Math.min(y0, y1) + 2.7, room.z + room.d / 2)
        parts.push(ceil)
      }
      const height = room.ceil ? 2.6 : 0.72
      const baseY = Math.min(y0, y1) + height / 2
      const edges = [
        { len: room.w, horizontal: true, x: room.x, z: room.z, nx: 0, nz: -1 },
        { len: room.w, horizontal: true, x: room.x, z: room.z + room.d, nx: 0, nz: 1 },
        { len: room.d, horizontal: false, x: room.x, z: room.z, nx: -1, nz: 0 },
        { len: room.d, horizontal: false, x: room.x + room.w, z: room.z, nx: 1, nz: 0 },
      ]
      for (const edge of edges) {
        const n = Math.max(1, Math.round(edge.len / 2))
        for (let i = 0; i < n; i++) {
          const t = (i + 0.5) / n
          const cx = edge.horizontal ? edge.x + edge.len * t : edge.x
          const cz = edge.horizontal ? edge.z : edge.z + edge.len * t
          if (segInsideOther(cx + edge.nx * 0.4, cz + edge.nz * 0.4, room, rooms)) continue
          const w = edge.horizontal ? edge.len / n : 0.18
          const d = edge.horizontal ? 0.18 : edge.len / n
          const wall = new THREE.BoxGeometry(w, height, d)
          wall.translate(cx, baseY, cz)
          parts.push(wall)
        }
      }
      if (room.landmark === "mast") {
        const mast = new THREE.CylinderGeometry(0.18, 0.24, 8, 6)
        mast.translate(room.x + room.w * 0.62, 4, room.z + room.d * 0.45)
        parts.push(mast)
      }
      if (room.landmark === "columns") {
        for (let i = 0; i < 3; i++) {
          const col = new THREE.CylinderGeometry(0.22, 0.26, 2.6, 6)
          col.translate(room.x + 1.2 + i * (room.w - 2.4) / 2, Math.min(y0, y1) + 1.3, room.z + room.d * 0.5)
          parts.push(col)
        }
      }
      const merged = parts.length === 1 ? parts[0] : mergeOrFirst(parts)
      const mesh = new THREE.Mesh(merged, matFor(room))
      mesh.userData.ownGeo = true
      const root = new THREE.Group()
      root.add(mesh)
      if (!room.ceil && (room.kind === "deck" || room.kind === "grave")) {
        const sail = new THREE.Mesh(new THREE.PlaneGeometry(1.6, 2.2), cloth)
        sail.position.set(room.x + room.w * 0.62, 3.2, room.z + room.d * 0.45)
        root.add(sail)
      }
      root.visible = !room.openFlag || flags.has(room.openFlag)
      level.add(root)
      roomRoots.set(room.id, root)
    }

    for (const item of ITEMS.filter((it) => it.act === next)) {
      const mesh = itemMesh(item)
      const p = itemPoint(item)
      mesh.position.set(p.x, p.y, p.z)
      level.add(mesh)
      itemMeshes.set(item.id, mesh)
    }
    for (const b of BLOCKERS.filter((bl) => bl.act === next)) {
      const mesh = new THREE.Mesh(new THREE.BoxGeometry(b.w, 2.2, b.d), iron)
      mesh.position.set(b.x + b.w / 2, 1.1, b.z + b.d / 2)
      mesh.userData.ownGeo = true
      level.add(mesh)
      blockerMeshes.set(b.id, mesh)
    }
    if (next === 4) {
      whale = new THREE.Mesh(new THREE.SphereGeometry(1, 10, 8), new THREE.MeshStandardMaterial({ color: 0x102026, roughness: 0.6 }))
      whale.scale.set(6.5, 1.5, 2.2)
      whale.position.set(28, 2.2, 64)
      scene.add(whale)
    } else if (whale) {
      scene.remove(whale)
      whale = null
    }

    const info = ACTS[next]
    fog.color.setHex(info.fogColor)
    fog.density = info.fog
    ;(scene.background as THREE.Color).setHex(info.fogColor)
    renderer.setClearColor(info.fogColor, 1)
    if (!waterMat) {
      waterMat = new THREE.ShaderMaterial({
        transparent: true,
        uniforms: {
          uTime: { value: 0 },
          uA: { value: new THREE.Color("#0b141a") },
          uB: { value: new THREE.Color("#16343a") },
          uC: { value: new THREE.Color("#18c5d2") },
        },
        vertexShader: `uniform float uTime; varying float vH; void main(){ vec3 p=position; float w=sin(p.x*0.25+uTime*0.6)*0.08+cos(p.z*0.18+uTime*0.4)*0.1; p.y+=w; vH=w; gl_Position=projectionMatrix*modelViewMatrix*vec4(p,1.0); }`,
        fragmentShader: `precision highp float; uniform vec3 uA; uniform vec3 uB; uniform vec3 uC; uniform float uTime; varying float vH; void main(){ float g=smoothstep(0.97,1.0,sin(vH*40.0+uTime)); vec3 c=mix(uA,uB,0.5+vH*2.0); c=mix(c,uC,g*0.45); gl_FragColor=vec4(c,0.88); }`,
      })
      const water = new THREE.Mesh(new THREE.PlaneGeometry(180, 180, 24, 24), waterMat)
      water.rotation.x = -Math.PI / 2
      water.position.y = -1.25
      scene.add(water)
    }
    const water = scene.children.find((c: THREE.Object3D) => (c as THREE.Mesh).material === waterMat) as THREE.Mesh | undefined
    if (water) water.position.y = next === 3 ? 0.22 : next === 4 ? -0.4 : -1.25
    portrait.visible = next === 1
    dressAct(next)
    applyQuality()
    refreshMeshes()
  }

  function dressAct(next: ActId) {
    const rig = new THREE.Group()
    const hullMat = new THREE.MeshStandardMaterial({ color: 0x140e0b, roughness: 0.92 })
    const spar = new THREE.MeshStandardMaterial({ color: 0x2a2118, roughness: 0.8 })
    if (next === 1) {
      const hull = new THREE.Mesh(new THREE.BoxGeometry(20, 4.2, 118), hullMat)
      hull.position.set(8, -2.4, 52)
      rig.add(hull)
      const bow = new THREE.Mesh(new THREE.ConeGeometry(6, 14, 4), hullMat)
      bow.rotation.x = Math.PI / 2
      bow.position.set(8, -0.4, 112)
      rig.add(bow)
      for (const mz of [30, 102]) {
        const mast = new THREE.Mesh(new THREE.CylinderGeometry(0.14, 0.26, 16, 8), spar)
        mast.position.set(8, 7.2, mz)
        const yard = new THREE.Mesh(new THREE.CylinderGeometry(0.05, 0.05, 8, 6), spar)
        yard.rotation.z = Math.PI / 2
        yard.position.set(8, 11, mz)
        const sail = new THREE.Mesh(new THREE.PlaneGeometry(6.2, 4.2), cloth)
        sail.position.set(8, 8.4, mz + 0.2)
        rig.add(mast, yard, sail)
      }
    }
    const moonDisc = new THREE.Mesh(
      new THREE.SphereGeometry(5.5, 20, 16),
      new THREE.MeshBasicMaterial({ color: 0xd5e2f2 }),
    )
    moonDisc.position.set(-36, 22, next === 1 ? 8 : 40)
    rig.add(moonDisc)
    level.add(rig)
  }

  function mergeOrFirst(parts: THREE.BufferGeometry[]) {
    if (parts.length === 1) return parts[0]!
    const merged = mergeGeometries(parts, false) as THREE.BufferGeometry | null
    if (!merged) return parts[0]!
    for (const part of parts) part.dispose()
    return merged
  }

  function itemMesh(item: Item) {
    let mesh: THREE.Mesh
    if (item.kind === "oil") mesh = new THREE.Mesh(new THREE.CylinderGeometry(0.1, 0.12, 0.28, 8), ember)
    else if (item.kind === "bell") mesh = new THREE.Mesh(new THREE.ConeGeometry(0.12, 0.22, 8), brass)
    else if (item.kind === "crate") mesh = new THREE.Mesh(new THREE.BoxGeometry(0.85, 0.7, 0.85), wood)
    else if (item.kind === "clock") mesh = new THREE.Mesh(new THREE.CylinderGeometry(0.28, 0.28, 0.08, 12), brass)
    else if (item.kind === "mirror") mesh = new THREE.Mesh(new THREE.BoxGeometry(0.7, 1.2, 0.06), iron)
    else if (item.kind === "exit" || item.kind === "altar") mesh = new THREE.Mesh(new THREE.TorusGeometry(0.45, 0.05, 6, 12), ember)
    else mesh = new THREE.Mesh(new THREE.BoxGeometry(0.34, 0.08, 0.42), new THREE.MeshStandardMaterial({ color: 0xd7c3a2, roughness: 0.8 }))
    mesh.userData.ownGeo = true
    return mesh
  }

  function itemPoint(item: Item) {
    const slot = item.kind === "crate" ? item.slots?.[crates[item.id] ?? 0] : undefined
    const ix = slot ? slot[0] : item.x
    const iz = slot ? slot[1] : item.z
    const room = pickRoom(ix, iz, true)
    const y = (room ? floorOf(room, iz) : 0) + (item.kind === "bell" || item.kind === "mirror" ? 1.2 : 0.45)
    return { x: ix, y, z: iz }
  }

  function openRooms(includeClosed = false) {
    return ROOMS.filter((r) => r.act === act && (includeClosed || !r.openFlag || flags.has(r.openFlag)))
  }

  function pickRoom(px: number, pz: number, includeClosed = false): Room | null {
    let best: Room | null = null
    let area = Infinity
    for (const room of openRooms(includeClosed)) {
      if (!roomContains(room, px, pz)) continue
      const a = room.w * room.d
      if (a < area) {
        area = a
        best = room
      }
    }
    return best
  }

  function floorOf(room: Room, pz: number) {
    if (room.floorTo === undefined) return room.floor
    const t = Math.min(1, Math.max(0, (pz - room.z) / room.d))
    return room.floor + (room.floorTo - room.floor) * t
  }

  function blockerHit(px: number, pz: number) {
    for (const b of BLOCKERS) {
      if (b.act !== act || flags.has(b.until)) continue
      if (px > b.x - R && px < b.x + b.w + R && pz > b.z - R && pz < b.z + b.d + R) return true
    }
    for (const item of ITEMS) {
      if (item.act !== act || item.kind !== "crate" || !item.slots) continue
      const p = itemPoint(item)
      const dx = px - p.x
      const dz = pz - p.z
      if (dx * dx + dz * dz < 0.72 * 0.72) return true
    }
    return false
  }

  function solid(px: number, pz: number) {
    const samples: [number, number][] = [
      [0, 0],
      [R, 0],
      [-R, 0],
      [0, R],
      [0, -R],
    ]
    for (const [dx, dz] of samples) {
      if (!pickRoom(px + dx, pz + dz)) return true
    }
    return blockerHit(px, pz)
  }

  function refreshMeshes() {
    for (const room of ROOMS) {
      const root = roomRoots.get(room.id)
      if (root) root.visible = room.act === act && (!room.openFlag || flags.has(room.openFlag))
    }
    for (const b of BLOCKERS) {
      const mesh = blockerMeshes.get(b.id)
      if (mesh) mesh.visible = b.act === act && !flags.has(b.until)
    }
    for (const item of ITEMS) {
      const mesh = itemMeshes.get(item.id)
      if (!mesh) continue
      const room = ROOMS.find((r) => r.id === item.room)
      const hidden =
        (item.once && item.flag && flags.has(item.flag) && (item.kind === "oil" || item.kind === "pickup" || item.kind === "hatch")) ||
        (room?.openFlag && !flags.has(room.openFlag))
      mesh.visible = !hidden && item.act === act
      if (item.kind === "crate" || item.kind === "mirror") {
        const p = itemPoint(item)
        mesh.position.set(p.x, p.y, p.z)
        if (item.kind === "mirror") mesh.rotation.y = (mirrors[item.id] ?? 0) * (Math.PI / 2)
      }
    }
  }

  function syncDerived() {
    const a = crates.a1_crate_a ?? 0
    const b = crates.a1_crate_b ?? 0
    if (a === 2 && b === 2) flags.add("a1_crates")
    else flags.delete("a1_crates")
    if ((crates.a2_crate ?? 0) === 2) flags.add("a2_clear")
    else flags.delete("a2_clear")
    const clockIds = ["a3_c1", "a3_c2", "a3_c3"]
    const clocksOk = clockIds.every((id) => {
      const item = ITEMS.find((it) => it.id === id)
      return item && (clocks[id] ?? 0) === item.answer
    })
    if (clocksOk) flags.add("a3_clocks")
    else flags.delete("a3_clocks")
    let prev = true
    for (const id of ["a4_m1", "a4_m2", "a4_m3", "a4_m4"]) {
      const item = ITEMS.find((it) => it.id === id)
      const rot = mirrors[id] ?? 0
      if (prev && item && rot === item.answer) flags.add(id)
      else flags.delete(id)
      prev = flags.has(id)
    }
    refreshMeshes()
  }

  function goalNow() {
    for (const g of GOALS[act]) if (!flags.has(g.flag)) return g
    return GOALS[act][GOALS[act].length - 1]!
  }

  function say(text: string) {
    subtitle = text
    subT = 5.5
  }

  function remember(item: Item) {
    if (!item.title || !item.body) return
    if (!logs.some((l) => l.id === item.id)) logs.unshift({ id: item.id, title: item.title, body: item.body })
    reading = { title: item.title, body: item.body }
  }

  function playSeq(puzzle: "a1" | "a2") {
    const notes = puzzle === "a1" ? A1_NOTES : A2_NOTES
    const seq = puzzle === "a1" ? A1_SEQ : A2_SEQ
    const names = puzzle === "a1" ? A1_NAMES : A2_NAMES
    seq.forEach((n, i) => {
      window.setTimeout(() => audio.tone(notes[n]!, 0.42), i * 480)
    })
    if (settings.subtitles) say(seq.map((n) => names[n]).join(" · "))
  }

  function onBell(item: Item) {
    if (item.puzzle === "a2c") {
      audio.tone(A2_NOTES[item.bell ?? 0]!, 0.5)
      if (item.flag) flags.add(item.flag)
      if (settings.subtitles && item.body) say(item.body)
      return
    }
    const puzzle = item.puzzle === "a2" ? "a2" : "a1"
    const seq = puzzle === "a1" ? A1_SEQ : A2_SEQ
    const notes = puzzle === "a1" ? A1_NOTES : A2_NOTES
    const names = puzzle === "a1" ? A1_NAMES : A2_NAMES
    const bell = item.bell ?? 0
    audio.tone(notes[bell]!, 0.4)
    let progress = puzzle === "a1" ? a1p : a2p
    if (seq[progress] === bell) progress += 1
    else progress = seq[0] === bell ? 1 : 0
    if (progress >= seq.length) {
      progress = 0
      if (puzzle === "a1") {
        flags.add("a1_music")
        sanity = Math.min(100, sanity + BALANCE.musicReward)
        say("Ninni tamam. Vanessa'nın kapısı aralandı.")
        audio.chime()
      } else {
        flags.add("a2_bells")
        sanity = Math.min(100, sanity + BALANCE.musicReward)
        say("Çanlar durdu. Tek Fener'deki çivi gevşer.")
        audio.chime()
      }
    } else if (settings.subtitles) say(names[bell] ?? "")
    else if (progress === 0) audio.wrong()
    if (puzzle === "a1") a1p = progress
    else a2p = progress
    syncDerived()
  }

  function interact() {
    if (mode === "card") {
      mode = "playing"
      emit()
      return
    }
    if (reading) {
      reading = null
      emit()
      return
    }
    if (mode !== "playing") return
    const fx = -Math.sin(yaw)
    const fz = -Math.cos(yaw)
    let best: Item | null = null
    let bestD = 2.25
    for (const item of ITEMS) {
      if (item.act !== act) continue
      const room = ROOMS.find((r) => r.id === item.room)
      if (room?.openFlag && !flags.has(room.openFlag)) continue
      if (item.once && item.flag && flags.has(item.flag) && (item.kind === "oil" || item.kind === "pickup" || item.kind === "hatch")) continue
      const p = itemPoint(item)
      const dx = p.x - x
      const dz = p.z - z
      const dist = Math.hypot(dx, dz)
      if (dist > bestD) continue
      const dot = (dx / (dist || 1)) * fx + (dz / (dist || 1)) * fz
      if (dot < 0.15 && dist > 1.1) continue
      best = item
      bestD = dist
    }
    if (!best) return
    if (best.need && !best.need.every((f) => flags.has(f))) {
      say(best.needMsg ?? "Henüz değil.")
      audio.wrong()
      emit()
      return
    }
    if (best.id === "a1_reset") {
      crates.a1_crate_a = 0
      crates.a1_crate_b = 0
      syncDerived()
      say("Kasalar ilk yerlerine döndü.")
      emit()
      return
    }
    if (best.kind === "bell") {
      onBell(best)
      emit()
      return
    }
    if (best.kind === "listen" && best.puzzle) {
      playSeq(best.puzzle === "a2" ? "a2" : "a1")
      emit()
      return
    }
    if (best.kind === "crate" && best.slots) {
      const cur = crates[best.id] ?? 0
      const next = (cur + 1) % best.slots.length
      const slot = best.slots[next]!
      const dx = slot[0] - x
      const dz = slot[1] - z
      if (dx * dx + dz * dz < 0.9) {
        say("Kasa seni ezer. Bir adım çekil.")
        emit()
        return
      }
      crates[best.id] = next
      syncDerived()
      audio.creak()
      say(flags.has(best.act === 1 ? "a1_crates" : "a2_clear") ? "Halka oturdu. Yol açıldı." : "Kasa kaydı.")
      emit()
      return
    }
    if (best.kind === "clock" && best.options) {
      clocks[best.id] = ((clocks[best.id] ?? 0) + 1) % best.options.length
      syncDerived()
      say(`${best.prompt}: ${best.options[clocks[best.id] ?? 0]}`)
      if (flags.has("a3_clocks")) say("Üç saat birden doğruldu. Deniz duvarı açıldı.")
      audio.tone(320, 0.15, "square", 0.04)
      emit()
      return
    }
    if (best.kind === "mirror") {
      mirrors[best.id] = ((mirrors[best.id] ?? 0) + 1) % 4
      syncDerived()
      const turns = mirrors[best.id] ?? 0
      say(flags.has(best.id) ? "Işın bu aynada tutundu." : `Ayna ${turns} çeyrek döndü.`)
      audio.tone(480, 0.12, "triangle", 0.04)
      emit()
      return
    }
    if (best.kind === "hatch") {
      flags.add("a1_hatch")
      say("Kapak kalktı. Ambar merdiveni açıldı.")
      audio.creak()
      syncDerived()
      emit()
      return
    }
    if (best.kind === "altar") {
      flags.add("a4_ending")
      mode = "ending"
      say("Ninni susuyor.")
      audio.chime()
      persist()
      emit()
      return
    }
    if (best.kind === "exit") {
      if (act < 4) {
        flags.add(best.flag ?? "")
        act = (act + 1) as ActId
        const spawn = SPAWN[act]
        x = spawn.x
        z = spawn.z
        yaw = Math.PI
        pitch = -0.06
        safeX = x
        safeZ = z
        buildAct(act)
        cardTitle = `Perde ${"I II III IV".split(" ")[act - 1]}`
        cardLine = `${ACTS[act].name}. ${ACTS[act].line}`
        mode = "card"
        persist()
        emit()
      }
      return
    }
    if (best.flag) flags.add(best.flag)
    if (best.oil) oil = Math.min(100, oil + best.oil)
    if (typeof best.sanity === "number") sanity = Math.min(100, Math.max(0, sanity + best.sanity))
    if (best.body) remember(best)
    else if (best.title) say(best.title)
    if (best.kind === "oil") say("Yağ eklendi. Fitil düzeliyor.")
    audio.tone(best.kind === "oil" ? 180 : 240, 0.12, "triangle", 0.04)
    syncDerived()
    emit()
  }

  function held(code: string) {
    return (probe ?? keys).has(code)
  }

  function persist() {
    const data: SaveData = {
      version: 1,
      act,
      x,
      z,
      yaw,
      pitch,
      sanity,
      tier,
      oil,
      lanternOn,
      flags: [...flags],
      crates,
      clocks,
      mirrors,
      logs,
    }
    writeSave(data)
  }

  function applySave(data: SaveData) {
    act = data.act
    x = data.x
    z = data.z
    yaw = data.yaw
    pitch = data.pitch
    sanity = data.sanity
    tier = data.tier
    oil = data.oil
    lanternOn = data.lanternOn
    flags.clear()
    data.flags.forEach((f) => flags.add(f))
    Object.keys(crates).forEach((k) => delete crates[k])
    Object.assign(crates, data.crates)
    Object.keys(clocks).forEach((k) => delete clocks[k])
    Object.assign(clocks, data.clocks)
    Object.keys(mirrors).forEach((k) => delete mirrors[k])
    Object.assign(mirrors, data.mirrors)
    logs.splice(0, logs.length, ...data.logs)
    safeX = x
    safeZ = z
    buildAct(act)
    syncDerived()
  }

  function resetWorld() {
    flags.clear()
    for (const k of Object.keys(crates)) delete crates[k]
    for (const k of Object.keys(clocks)) delete clocks[k]
    for (const k of Object.keys(mirrors)) delete mirrors[k]
    logs.splice(0, logs.length)
    act = 1
    sanity = 100
    tier = "stable"
    oil = BALANCE.startOil
    lanternOn = true
    a1p = 0
    a2p = 0
    seaT = 0
    x = SPAWN[1].x
    z = SPAWN[1].z
    yaw = Math.PI
    pitch = -0.08
    safeX = x
    safeZ = z
    reading = null
    clearSave()
    buildAct(1)
  }

  function nearestPrompt() {
    if (mode !== "playing" || reading) return null
    const fx = -Math.sin(yaw)
    const fz = -Math.cos(yaw)
    let label: string | null = null
    let best = 2.25
    for (const item of ITEMS) {
      if (item.act !== act) continue
      const room = ROOMS.find((r) => r.id === item.room)
      if (room?.openFlag && !flags.has(room.openFlag)) continue
      const p = itemPoint(item)
      const dx = p.x - x
      const dz = p.z - z
      const dist = Math.hypot(dx, dz)
      if (dist > best) continue
      const dot = (dx / (dist || 1)) * fx + (dz / (dist || 1)) * fz
      if (dot < 0.15 && dist > 1.1) continue
      label = item.prompt
      best = dist
    }
    return label
  }

  function emit() {
    const g = goalNow()
    onHud({
      mode,
      settingsFrom,
      act,
      actName: ACTS[act].name,
      objective: g?.text ?? "",
      prompt: nearestPrompt(),
      oil,
      lanternOn,
      subtitle: settings.subtitles ? subtitle : null,
      hasSave: loadSave() !== null,
      reading,
      logs: [...logs],
      cardTitle,
      cardLine,
      quality: settings.quality,
      sens: settings.sens,
      volume: settings.volume,
      subtitles: settings.subtitles,
      motion: settings.motion,
      haptics: settings.haptics,
      webgl,
    })
  }

  function applyQuality() {
    const cap = settings.quality === "low" ? 1 : settings.quality === "high" ? 1.75 : 1.35
    const pr = Math.min(window.devicePixelRatio || 1, cap)
    renderer.setPixelRatio(pr)
    resize()
  }

  function resize() {
    const w = canvas.clientWidth || window.innerWidth
    const h = canvas.clientHeight || window.innerHeight
    if (w < 2 || h < 2) return
    renderer.setSize(w, h, false)
    camera.aspect = w / h
    camera.updateProjectionMatrix()
    viewCam.aspect = w / h
    viewCam.updateProjectionMatrix()
  }

  const onResize = () => resize()
  window.addEventListener("resize", onResize)
  const onKeyDown = (e: KeyboardEvent) => {
    if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return
    keys.add(e.code)
    if (e.code === "KeyE") interact()
    if (e.code === "KeyF") toggleLantern()
    if (e.code === "Escape") {
      if (mode === "playing") pause()
      else if (mode === "paused") resume()
    }
  }
  const onKeyUp = (e: KeyboardEvent) => keys.delete(e.code)
  const onBlur = () => keys.clear()
  const onMove = (e: MouseEvent) => {
    if (document.pointerLockElement !== canvas) return
    addLook(e.movementX, e.movementY)
  }
  window.addEventListener("keydown", onKeyDown)
  window.addEventListener("keyup", onKeyUp)
  window.addEventListener("blur", onBlur)
  window.addEventListener("mousemove", onMove)

  function toggleLantern() {
    if (mode !== "playing") return
    if (!lanternOn && oil <= 0) {
      say("Yağ yok. Fitil tutuşmuyor.")
      return
    }
    lanternOn = !lanternOn
    audio.tone(lanternOn ? 220 : 90, 0.08, "triangle", 0.04)
    emit()
  }

  function pause() {
    if (mode !== "playing") return
    mode = "paused"
    document.exitPointerLock?.()
    persist()
    emit()
  }
  function resume() {
    if (mode !== "paused" && mode !== "log" && mode !== "settings") return
    mode = "playing"
    emit()
  }

  function addLook(dx: number, dy: number) {
    lookX += dx
    lookY += dy
  }

  function sim(dt: number) {
    menuT += dt
    if (mode === "menu") {
      const s = SPAWN[1]
      x = s.x
      z = s.z + 1.2
      yaw = Math.PI + Math.sin(menuT * 0.25) * 0.18
      pitch = -0.12
      lanternOn = true
    } else if (mode === "playing" && !reading) {
      const sens = 0.00215 * settings.sens
      yaw -= lookX * sens
      pitch -= lookY * sens
      const limit = Math.PI / 2 - 0.08
      pitch = Math.max(-limit, Math.min(limit, pitch))
      lookX = 0
      lookY = 0
      let ix = stickX
      let iz = stickY
      if (held("KeyA") || held("ArrowLeft")) ix -= 1
      if (held("KeyD") || held("ArrowRight")) ix += 1
      if (held("KeyW") || held("ArrowUp")) iz += 1
      if (held("KeyS") || held("ArrowDown")) iz -= 1
      const len = Math.hypot(ix, iz)
      if (len > 1) {
        ix /= len
        iz /= len
      }
      const sprint = (sprintHeld || held("ShiftLeft") || held("ShiftRight")) && len > 0.2
      const speed = sprint ? 3.35 : 2.05
      const fx = -Math.sin(yaw)
      const fz = -Math.cos(yaw)
      const rx = Math.cos(yaw)
      const rz = -Math.sin(yaw)
      const mx = (fx * iz + rx * ix) * speed * dt
      const mz = (fz * iz + rz * ix) * speed * dt
      horizSpeed = Math.hypot(mx, mz) / Math.max(dt, 0.0001)
      if (!solid(x + mx, z)) x += mx
      if (!solid(x, z + mz)) z += mz
      if (!pickRoom(x, z)) {
        x = safeX
        z = safeZ
      }
      const room = pickRoom(x, z)
      if (room && room.name !== roomName) {
        roomName = room.name
        say(room.name)
      }
      const indoors = !!room?.ceil
      const dark = !lanternOn || oil <= 0
      let delta = 0
      if (room?.safe && lanternOn && oil > 0) delta += BALANCE.safeRegen
      else if (dark && room && room.ambient < 0.25 && !room.safe) delta -= BALANCE.darkDrain
      else if (dark) delta -= BALANCE.moonDrain
      const openDeck = room && (room.kind === "deck" || room.kind === "grave" || room.id === "a4_gal")
      if (openDeck && horizSpeed < 0.35 && Math.abs(pitch) < 0.42) seaT += dt
      else seaT = Math.max(0, seaT - dt * 2)
      if (seaT > BALANCE.seaDelay) {
        delta -= BALANCE.seaDrain
        if (!warnedSea) {
          warnedSea = true
          say("Su seni çağırıyor. Bakışını çek.")
        }
      }
      sanity = Math.max(0, Math.min(100, sanity + delta * dt))
      if (delta < 0) stableHold = 0
      else if (tier === "stable") stableHold += dt
      tier = updateTier(sanity, tier)
      if (lanternOn && oil > 0) {
        oil = Math.max(0, oil - (BALANCE.oilBurn + (sprint ? BALANCE.oilSprint : 0)) * dt)
        if (oil <= 0) {
          lanternOn = false
          if (!warnedOil) {
            warnedOil = true
            say("Yağ bitti. Karanlık ağırlaşıyor.")
          }
        }
      }
      if (sprint && indoors === false && !warnedWind && lanternOn) {
        warnedWind = true
        say("Rüzgâr alevi yatırıyor. Koşunca yağ da gider.")
      }
      if (horizSpeed > 0.6) {
        stepAcc += dt
        if (stepAcc > (sprint ? 0.34 : 0.5)) {
          stepAcc = 0
          audio.step()
        }
      }
      saveAcc += dt
      if (saveAcc > 8) {
        saveAcc = 0
        safeX = x
        safeZ = z
        safeYaw = yaw
        persist()
      }
      distortAcc -= dt
      if (distortAcc <= 0) {
        distortAcc = 5 + Math.random() * 3
        const camF = new THREE.Vector3(-Math.sin(yaw), 0, -Math.cos(yaw))
        const toP = new THREE.Vector3(1.15 - x, 0, 28 - z).normalize()
        const looking = camF.dot(toP) > 0.45
        if (!looking && tier !== "stable") {
          const mag = tier === "uneasy" ? 0.08 : tier === "disturbed" ? 0.28 : 0.45
          portraitOff = Math.max(-mag, Math.min(mag, portraitOff + (Math.random() > 0.5 ? mag : -mag)))
        }
        if (tier === "stable" && stableHold > 10) portraitOff *= 0.5
      }
    } else {
      lookX = 0
      lookY = 0
      horizSpeed = 0
    }
    if (subT > 0) {
      subT -= dt
      if (subT <= 0) subtitle = null
    }

    const room = pickRoom(x, z)
    const eye = (room ? floorOf(room, z) : 0) + 1.58
    const bob = settings.motion && horizSpeed > 0.4 ? Math.sin(menuT * 8) * 0.025 : 0
    camera.position.set(x, eye + bob, z)
    camera.rotation.y = yaw
    camera.rotation.x = pitch
    const fov = 72 + (settings.motion && tier === "breaking" ? Math.sin(menuT) * 1.2 : 0)
    if (Math.abs(camera.fov - fov) > 0.05) {
      camera.fov = fov
      camera.updateProjectionMatrix()
    }
    const fx = -Math.sin(yaw)
    const fz = -Math.cos(yaw)
    const rx = Math.cos(yaw)
    const rz = -Math.sin(yaw)
    const flick = lanternOn && oil > 0 ? 0.82 + Math.sin(menuT * (sprintHeld ? 28 : 13)) * (sprintHeld ? 0.12 : 0.04) : 0
    lamp.intensity = lanternOn && oil > 0 ? (2.4 + 4.2 * (oil / 100)) * (0.75 + flick) : 0
    lamp.position.set(x + fx * 0.45 + rx * 0.22, eye - 0.25, z + fz * 0.45 + rz * 0.22)
    hemi.intensity = room?.ceil ? (room.safe && lanternOn ? 0.28 : 0.07) : 0.4
    moon.intensity = room?.ceil ? 0.04 : 0.62
    flame.visible = lanternOn && oil > 0
    flame.scale.y = 0.7 + flick
    portrait.rotation.y = Math.PI / 2 + portraitOff
    const g = goalNow()
    if (g && mode === "playing") {
      const gr = pickRoom(g.x, g.z, true)
      goal.position.set(g.x, (gr ? floorOf(gr, g.z) : 0) + 1.15, g.z)
      goal.visible = true
    } else goal.visible = false
    if (whale) {
      whale.position.x = 26 + Math.sin(menuT * 0.15) * 4
      whale.position.z = 62 + Math.cos(menuT * 0.1) * 3
    }
    if (waterMat) waterMat.uniforms.uTime!.value = menuT
    audio.setWind(room?.ceil ? 0.15 : 0.7)
    audio.setDrone(tier === "stable" ? 0 : tier === "uneasy" ? 0.3 : tier === "disturbed" ? 0.6 : 1)
    renderer.autoClear = false
    renderer.clear()
    renderer.render(scene, camera)
    renderer.clearDepth()
    renderer.render(viewScene, viewCam)
  }

  let raf = 0
  let hudAcc = 0
  const loop = () => {
    raf = requestAnimationFrame(loop)
    clock.update()
    const dt = Math.min(clock.getDelta(), 0.05)
    sim(dt)
    hudAcc += dt
    if (hudAcc > 0.2) {
      hudAcc = 0
      emit()
    }
  }

  buildAct(1)
  resize()
  loop()
  emit()

  window.__controlsTest = {
    getYaw: () => yaw,
    getSpeed: () => horizSpeed,
    getPos: () => ({ x, z }),
    setKeys: (codes: string[]) => {
      probe = codes.length ? new Set(codes) : null
    },
  }

  return {
    dispose: () => {
      cancelAnimationFrame(raf)
      clock.disconnect()
      window.removeEventListener("resize", onResize)
      window.removeEventListener("keydown", onKeyDown)
      window.removeEventListener("keyup", onKeyUp)
      window.removeEventListener("blur", onBlur)
      window.removeEventListener("mousemove", onMove)
      audio.dispose()
      renderer.dispose()
      delete window.__controlsTest
    },
    setStick: (sx, sy) => {
      stickX = sx
      stickY = sy
    },
    setSprint: (v) => {
      sprintHeld = v
    },
    addLook,
    interact: () => {
      audio.unlock()
      interact()
    },
    toggleLantern: () => {
      audio.unlock()
      toggleLantern()
    },
    pause,
    resume,
    newGame: () => {
      audio.unlock()
      resetWorld()
      mode = "playing"
      say("Kıç güverte. Fener sende. Yağ az.")
      emit()
    },
    continueGame: () => {
      audio.unlock()
      const data = loadSave()
      if (!data) return
      applySave(data)
      mode = "playing"
      emit()
    },
    back: () => {
      if (reading) {
        reading = null
        emit()
        return
      }
      if (mode === "playing") pause()
      else if (mode === "paused" || mode === "log" || mode === "card") {
        mode = mode === "paused" ? "menu" : "playing"
        if (mode === "menu") document.exitPointerLock?.()
        emit()
      } else if (mode === "settings") {
        mode = settingsFrom === "paused" ? "paused" : "menu"
        emit()
      }
    },
    openSettings: (from) => {
      settingsFrom = from
      mode = "settings"
      emit()
    },
    closeSettings: () => {
      mode = settingsFrom === "paused" ? "paused" : "menu"
      emit()
    },
    setSetting: (patch) => {
      Object.assign(settings, patch)
      writeSettings(settings)
      audio.setVolume(settings.volume)
      applyQuality()
      emit()
    },
    closeReading: () => {
      reading = null
      emit()
    },
    openLog: () => {
      mode = "log"
      emit()
    },
    closeLog: () => {
      mode = settingsFrom === "paused" || loadSave() ? "paused" : "menu"
      if (mode === "paused" || flags.size) mode = mode === "menu" && flags.size ? "paused" : mode
      mode = "paused"
      if (!loadSave() && flags.size === 0) mode = "menu"
      emit()
    },
    quitToMenu: () => {
      persist()
      mode = "menu"
      document.exitPointerLock?.()
      emit()
    },
    dismissCard: () => {
      mode = "playing"
      emit()
    },
    rescue: () => {
      x = safeX
      z = safeZ
      yaw = safeYaw
      say("Son güvenli noktaya döndün.")
      emit()
    },
    lock: () => {
      if (matchMedia("(pointer: coarse)").matches) return
      canvas.requestPointerLock?.()
    },
  }
}
