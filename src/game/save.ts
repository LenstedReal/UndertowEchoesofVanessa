import type { ActId } from "@/game/content"

export type Tier = "stable" | "uneasy" | "disturbed" | "breaking"
export type Quality = "low" | "medium" | "high"

export type LogEntry = { id: string; title: string; body: string }

export type SaveData = {
  version: 1
  act: ActId
  x: number
  z: number
  yaw: number
  pitch: number
  sanity: number
  tier: Tier
  oil: number
  lanternOn: boolean
  flags: string[]
  crates: Record<string, number>
  clocks: Record<string, number>
  mirrors: Record<string, number>
  logs: LogEntry[]
}

export type Settings = {
  sens: number
  volume: number
  subtitles: boolean
  motion: boolean
  haptics: boolean
  quality: Quality
}

const SAVE_KEY = "undertow-echoes-v1"
const SET_KEY = "undertow-echoes-settings"

export const defaultSettings = (): Settings => ({
  sens: 1,
  volume: 0.75,
  subtitles: true,
  motion: true,
  haptics: false,
  quality: "medium",
})

export function loadSettings(): Settings {
  try {
    const raw = localStorage.getItem(SET_KEY)
    if (!raw) return defaultSettings()
    return { ...defaultSettings(), ...(JSON.parse(raw) as Settings) }
  } catch {
    return defaultSettings()
  }
}

export function writeSettings(s: Settings) {
  localStorage.setItem(SET_KEY, JSON.stringify(s))
}

export function loadSave(): SaveData | null {
  try {
    const raw = localStorage.getItem(SAVE_KEY)
    if (!raw) return null
    const data = JSON.parse(raw) as SaveData
    if (data.version !== 1) return null
    return data
  } catch {
    return null
  }
}

export function writeSave(data: SaveData) {
  localStorage.setItem(SAVE_KEY, JSON.stringify(data))
}

export function clearSave() {
  localStorage.removeItem(SAVE_KEY)
}
