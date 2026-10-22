/** Undertow: Echoes of Vanessa — motor-bağımsız perde verisi. */

export type ActId = 1 | 2 | 3 | 4
export type Kind = "deck" | "cabin" | "hold" | "hall" | "grave" | "street" | "temple"

export type Room = {
  id: string
  act: ActId
  name: string
  x: number
  z: number
  w: number
  d: number
  floor: number
  floorTo?: number
  ceil: boolean
  kind: Kind
  ambient: number
  safe?: boolean
  openFlag?: string
  landmark?: "mast" | "columns" | "clock" | "whale"
}

export type Blocker = {
  id: string
  act: ActId
  x: number
  z: number
  w: number
  d: number
  until: string
}

export type Item = {
  id: string
  act: ActId
  room: string
  x: number
  z: number
  kind:
    | "log"
    | "oil"
    | "bell"
    | "listen"
    | "crate"
    | "clock"
    | "mirror"
    | "pickup"
    | "examine"
    | "hatch"
    | "exit"
    | "altar"
  prompt: string
  title?: string
  body?: string
  flag?: string
  need?: string[]
  needMsg?: string
  sanity?: number
  oil?: number
  once?: boolean
  puzzle?: "a1" | "a2" | "a2c"
  bell?: number
  slots?: [number, number][]
  goal?: number
  options?: string[]
  answer?: number
}

export type Goal = { flag: string; text: string; x: number; z: number }

export const ACTS: Record<ActId, { name: string; line: string; fog: number; fogColor: number }> = {
  1: {
    name: "Lanetli Kalyon",
    line: "Sis gemiyi yuttu. Vanessa güvertede değil.",
    fog: 0.052,
    fogColor: 0x4e5d62,
  },
  2: {
    name: "Gemi Mezarlığı",
    line: "Enkazlar bir koridor gibi dizilmiş. Yanlış yöne giden yol da sis.",
    fog: 0.04,
    fogColor: 0x3a4a54,
  },
  3: {
    name: "Terk Edilmiş Liman",
    line: "Saatler durmuş. Diz boyu suda Vanessa'nın izi var.",
    fog: 0.036,
    fogColor: 0x3e4b52,
  },
  4: {
    name: "Batık Tapınak",
    line: "Işık aşağı iniyor. Uzakta bir şey nefes alıyor.",
    fog: 0.046,
    fogColor: 0x0e3c44,
  },
}

export const SPAWN: Record<ActId, { x: number; z: number }> = {
  1: { x: 6.2, z: 4.2 },
  2: { x: 6, z: 5 },
  3: { x: 8, z: 6 },
  4: { x: 9, z: 5 },
}

export const A1_SEQ = [0, 2, 1, 2, 4]
export const A1_NOTES = [440, 659.25, 523.25, 659.25, 880]
export const A1_NAMES = ["La", "Mi", "Do", "Mi", "La"]

export const A2_SEQ = [0, 1, 2]
export const A2_NOTES = [440, 523.25, 659.25]
export const A2_NAMES = ["La", "Do", "Mi"]

export const BALANCE = {
  darkDrain: 0.8,
  moonDrain: 0.22,
  seaDrain: 1.2,
  seaDelay: 6,
  safeRegen: 1,
  examineCost: 4,
  musicReward: 15,
  oilBurn: 0.075,
  oilSprint: 0.05,
  startOil: 64,
  oilFlask: 42,
  hysteresis: 3,
}

const R: Room[] = [
  { id: "a1_stern", act: 1, name: "Kıç güverte", x: 0, z: 0, w: 16, d: 18, floor: 0, ceil: false, kind: "deck", ambient: 0.55 },
  { id: "a1_main", act: 1, name: "Ana güverte", x: 0, z: 17, w: 16, d: 26, floor: 0, ceil: false, kind: "deck", ambient: 0.5, landmark: "mast" },
  { id: "a1_fore", act: 1, name: "Pruva", x: 1, z: 40, w: 14, d: 16, floor: 0, ceil: false, kind: "deck", ambient: 0.48 },
  { id: "a1_well", act: 1, name: "Kıç merdiveni", x: 6, z: 10, w: 4, d: 16, floor: 0, floorTo: -4, ceil: false, kind: "hall", ambient: 0.3 },
  { id: "a1_spine", act: 1, name: "Top güvertesi", x: 3, z: 22, w: 7, d: 42, floor: -4, ceil: true, kind: "hold", ambient: 0.07 },
  { id: "a1_crew", act: 1, name: "Tayfa kamarası", x: -6, z: 30, w: 12, d: 12, floor: -4, ceil: true, kind: "cabin", ambient: 0.1 },
  { id: "a1_music", act: 1, name: "Ninni odası", x: -8, z: 46, w: 14, d: 12, floor: -4, ceil: true, kind: "cabin", ambient: 0.12, safe: true },
  { id: "a1_nessa", act: 1, name: "Vanessa'nın kamarası", x: 8, z: 46, w: 12, d: 12, floor: -4, ceil: true, kind: "cabin", ambient: 0.1 },
  { id: "a1_surg", act: 1, name: "Cerrah kamarası", x: -8, z: 62, w: 14, d: 12, floor: -4, ceil: true, kind: "cabin", ambient: 0.06 },
  { id: "a1_stair", act: 1, name: "Pruva ambarı", x: 8, z: 50, w: 4, d: 22, floor: 0, floorTo: -4, ceil: true, kind: "hall", ambient: 0.08, openFlag: "a1_hatch" },
  { id: "a1_cargo", act: 1, name: "Yük ambarı", x: 1, z: 68, w: 16, d: 16, floor: -4, ceil: true, kind: "hold", ambient: 0.08 },
  { id: "a1_bowramp", act: 1, name: "Pruva çıkışı", x: 7, z: 80, w: 4, d: 18, floor: -4, floorTo: 0, ceil: true, kind: "hall", ambient: 0.2 },
  { id: "a1_bow", act: 1, name: "Pruva ucu", x: 1, z: 94, w: 14, d: 16, floor: 0, ceil: false, kind: "deck", ambient: 0.5 },

  { id: "a2_boat", act: 2, name: "Filika iskelesi", x: 0, z: 0, w: 12, d: 16, floor: 0, ceil: false, kind: "grave", ambient: 0.42 },
  { id: "a2_g1", act: 2, name: "İlk geçit", x: 4, z: 15, w: 4, d: 40, floor: 0, ceil: false, kind: "grave", ambient: 0.36 },
  { id: "a2_b", act: 2, name: "Tek Fener enkazı", x: -2, z: 54, w: 16, d: 16, floor: 0, ceil: false, kind: "grave", ambient: 0.34 },
  { id: "a2_g2", act: 2, name: "İkinci geçit", x: 4, z: 69, w: 4, d: 40, floor: 0, ceil: false, kind: "grave", ambient: 0.32 },
  { id: "a2_c", act: 2, name: "Moloz teknesi", x: -2, z: 108, w: 18, d: 16, floor: 0, ceil: false, kind: "grave", ambient: 0.3 },
  { id: "a2_g3", act: 2, name: "Üçüncü geçit", x: 4, z: 123, w: 4, d: 40, floor: 0, ceil: false, kind: "grave", ambient: 0.3 },
  { id: "a2_d", act: 2, name: "Üç Fener enkazı", x: -2, z: 162, w: 16, d: 16, floor: 0, ceil: false, kind: "grave", ambient: 0.33 },
  { id: "a2_g4", act: 2, name: "Karanlık geçit", x: 4, z: 177, w: 4, d: 40, floor: 0, ceil: false, kind: "grave", ambient: 0.22 },
  { id: "a2_e", act: 2, name: "İki Fener ambarı", x: -4, z: 216, w: 20, d: 16, floor: 0, ceil: true, kind: "hold", ambient: 0.08 },
  { id: "a2_g5", act: 2, name: "Son geçit", x: 4, z: 231, w: 4, d: 36, floor: 0, ceil: false, kind: "grave", ambient: 0.3 },
  { id: "a2_hulk", act: 2, name: "Çan teknesi", x: -2, z: 266, w: 18, d: 18, floor: 0, ceil: false, kind: "grave", ambient: 0.28 },
  { id: "a2_gate", act: 2, name: "Sis kapısı", x: 2, z: 280, w: 10, d: 16, floor: 0, ceil: false, kind: "grave", ambient: 0.4 },

  { id: "a3_dock", act: 3, name: "Rıhtım", x: 0, z: 0, w: 18, d: 18, floor: 0, ceil: false, kind: "street", ambient: 0.4 },
  { id: "a3_street", act: 3, name: "Su sokağı", x: 4, z: 16, w: 8, d: 96, floor: 0, ceil: false, kind: "street", ambient: 0.34 },
  { id: "a3_customs", act: 3, name: "Gümrük", x: -12, z: 22, w: 18, d: 14, floor: 0, ceil: true, kind: "cabin", ambient: 0.16 },
  { id: "a3_tavern", act: 3, name: "Taverna", x: 10, z: 24, w: 14, d: 14, floor: 0, ceil: true, kind: "cabin", ambient: 0.14 },
  { id: "a3_ware", act: 3, name: "Antrepo", x: -14, z: 44, w: 20, d: 14, floor: 0, ceil: true, kind: "hold", ambient: 0.1 },
  { id: "a3_chapel", act: 3, name: "Şapel", x: 10, z: 46, w: 16, d: 14, floor: 0, ceil: true, kind: "temple", ambient: 0.18, safe: true },
  { id: "a3_square", act: 3, name: "Meydan", x: 0, z: 62, w: 18, d: 16, floor: 0, ceil: false, kind: "street", ambient: 0.36, landmark: "clock" },
  { id: "a3_house", act: 3, name: "Vanessa'nın evi", x: -14, z: 78, w: 20, d: 14, floor: 0, ceil: true, kind: "cabin", ambient: 0.12 },
  { id: "a3_tower", act: 3, name: "Saat kulesi", x: 3, z: 100, w: 12, d: 16, floor: 0, ceil: true, kind: "hall", ambient: 0.15, landmark: "clock" },
  { id: "a3_sea", act: 3, name: "Deniz duvarı", x: 5, z: 108, w: 8, d: 20, floor: 0, ceil: false, kind: "street", ambient: 0.3, openFlag: "a3_clocks" },

  { id: "a4_nave", act: 4, name: "Narteks", x: 0, z: 0, w: 18, d: 16, floor: 0, ceil: true, kind: "temple", ambient: 0.2, safe: true, landmark: "columns" },
  { id: "a4_hall", act: 4, name: "Yanık hol", x: 5, z: 14, w: 8, d: 28, floor: 0, ceil: true, kind: "temple", ambient: 0.12, landmark: "columns" },
  { id: "a4_m1r", act: 4, name: "Birinci ayna", x: -10, z: 20, w: 18, d: 12, floor: 0, ceil: true, kind: "temple", ambient: 0.1 },
  { id: "a4_m2r", act: 4, name: "İkinci ayna", x: 10, z: 20, w: 16, d: 12, floor: 0, ceil: true, kind: "temple", ambient: 0.1 },
  { id: "a4_cross", act: 4, name: "Haç holü", x: 4, z: 40, w: 10, d: 18, floor: 0, ceil: true, kind: "temple", ambient: 0.12, landmark: "columns" },
  { id: "a4_m3r", act: 4, name: "Üçüncü ayna", x: -10, z: 42, w: 18, d: 12, floor: 0, ceil: true, kind: "temple", ambient: 0.1 },
  { id: "a4_m4r", act: 4, name: "Dördüncü ayna", x: 10, z: 42, w: 16, d: 12, floor: 0, ceil: true, kind: "temple", ambient: 0.1 },
  { id: "a4_gal", act: 4, name: "Balina galerisi", x: 0, z: 56, w: 20, d: 18, floor: 0, ceil: true, kind: "temple", ambient: 0.16, landmark: "whale" },
  { id: "a4_ramp", act: 4, name: "Sunak inişi", x: 6, z: 72, w: 6, d: 16, floor: 0, floorTo: -1.2, ceil: true, kind: "hall", ambient: 0.14 },
  { id: "a4_altar", act: 4, name: "Sunak", x: 2, z: 86, w: 14, d: 16, floor: -1.2, ceil: true, kind: "temple", ambient: 0.22, landmark: "columns" },
]

export const ROOMS: Room[] = R

export const BLOCKERS: Blocker[] = [
  { id: "b_nessa", act: 1, x: 7.4, z: 45.6, w: 3.2, d: 12.6, until: "a1_music" },
  { id: "b_surg", act: 1, x: 2.4, z: 61.6, w: 5.4, d: 12.6, until: "a1_letter" },
  { id: "b_bow", act: 1, x: 6.2, z: 79.2, w: 5.2, d: 2.4, until: "a1_crates" },
  { id: "b_g3", act: 2, x: 3.2, z: 122.2, w: 5.4, d: 2.6, until: "a2_clear" },
  { id: "b_gate", act: 2, x: 2.2, z: 282.4, w: 9.4, d: 2.2, until: "a2_nail" },
]

const clueNeed = ["a3_customs", "a3_tavern", "a3_ware", "a3_chapel", "a3_house"]

export const ITEMS: Item[] = [
  { id: "a1_oil_stern", act: 1, room: "a1_stern", x: 3.2, z: 6.5, kind: "oil", prompt: "Yağ tulumunu al", oil: BALANCE.oilFlask, flag: "a1_oil_stern", once: true },
  {
    id: "a1_log_stern",
    act: 1,
    room: "a1_stern",
    x: 11.5,
    z: 7,
    kind: "log",
    prompt: "Güverte defterini oku",
    title: "Son vardiya",
    body: "Vanessa dümeni bıraktı. Ninni hâlâ ambarda çalıyor. Denize uzun bakma. Feneri söndürme. Pruva kapağı paslı; kanca cerrahın dolabında.",
    flag: "a1_log_stern",
  },
  {
    id: "a1_port",
    act: 1,
    room: "a1_main",
    x: 1.4,
    z: 28,
    kind: "examine",
    prompt: "Portreyi incele",
    title: "İsimsiz portre",
    body: "Yağlıboya bir kadın. Gözleri senin durduğun yere bakmıyor. Henüz. Çerçeve ıslak, imza silinmiş.",
    flag: "a1_portrait",
    sanity: -BALANCE.examineCost,
    once: true,
  },
  {
    id: "a1_oil_spine",
    act: 1,
    room: "a1_spine",
    x: 8.2,
    z: 40,
    kind: "oil",
    prompt: "Yedek yağı al",
    oil: 36,
    flag: "a1_oil_spine",
    once: true,
  },
  {
    id: "a1_log_crew",
    act: 1,
    room: "a1_crew",
    x: 2,
    z: 36,
    kind: "log",
    prompt: "Ranzadaki notu oku",
    title: "Tayfa notu",
    body: "Ninni beş vuruş. Duvar noktaları yalan söylemez: bir, üç, iki, üç, beş. İsimler: La, Mi, Do, Mi, La. Kulakların yetmezse altyazı yeter.",
    flag: "a1_crewnote",
  },
  {
    id: "a1_listen",
    act: 1,
    room: "a1_music",
    x: -2,
    z: 50,
    kind: "listen",
    prompt: "Ninniyi dinle",
    puzzle: "a1",
    title: "Müzik kutusu",
    body: "Kutu aynı beş notayı çalar. Zamanlama yok. Sırayı çanlarda tekrarla.",
  },
  { id: "a1_b0", act: 1, room: "a1_music", x: -5.5, z: 53, kind: "bell", prompt: "İlk çan", puzzle: "a1", bell: 0 },
  { id: "a1_b1", act: 1, room: "a1_music", x: -2.5, z: 53.4, kind: "bell", prompt: "İkinci çan", puzzle: "a1", bell: 1 },
  { id: "a1_b2", act: 1, room: "a1_music", x: 0.4, z: 53, kind: "bell", prompt: "Üçüncü çan", puzzle: "a1", bell: 2 },
  { id: "a1_b3", act: 1, room: "a1_music", x: 3.2, z: 53.3, kind: "bell", prompt: "Dördüncü çan", puzzle: "a1", bell: 3 },
  { id: "a1_b4", act: 1, room: "a1_music", x: 1.2, z: 56, kind: "bell", prompt: "Beşinci çan", puzzle: "a1", bell: 4 },
  {
    id: "a1_letter",
    act: 1,
    room: "a1_nessa",
    x: 14,
    z: 52,
    kind: "log",
    prompt: "Mektubu oku",
    title: "Vanessa'nın mektubu",
    body: "Kanca cerrah dolabında, üst raf. Yük halkaları sancak iç köşede: iki kasa da sağdaki çembere. Kolyem pruvada. Beni denizde arama; beni denizde bırakma.",
    flag: "a1_letter",
  },
  {
    id: "a1_exam_surg",
    act: 1,
    room: "a1_surg",
    x: -4,
    z: 66,
    kind: "examine",
    prompt: "Masayı incele",
    title: "Cerrah masası",
    body: "Kan yok. Yalnızca ıslak bez, kesilmiş bir harita ve tuz. Haritanın kenarında liman saati karalanmış: kasaba durunca gemi durmamış.",
    flag: "a1_table",
    sanity: -BALANCE.examineCost,
    once: true,
  },
  {
    id: "a1_hook",
    act: 1,
    room: "a1_surg",
    x: -2,
    z: 70,
    kind: "pickup",
    prompt: "Kancayı al",
    title: "Paslı kanca",
    body: "Gemi kancası. Pruva kapağının halkasına girer.",
    flag: "a1_hook",
    once: true,
  },
  {
    id: "a1_hatch",
    act: 1,
    room: "a1_fore",
    x: 8,
    z: 48,
    kind: "hatch",
    prompt: "Kapağı kaldır",
    need: ["a1_hook"],
    needMsg: "Kapak paslı. Bir kanca lazım.",
    flag: "a1_hatch",
    once: true,
  },
  {
    id: "a1_oil_cargo",
    act: 1,
    room: "a1_cargo",
    x: 4,
    z: 70,
    kind: "oil",
    prompt: "Ambar yağını al",
    oil: 40,
    flag: "a1_oil_cargo",
    once: true,
  },
  {
    id: "a1_log_cargo",
    act: 1,
    room: "a1_cargo",
    x: 4.2,
    z: 78,
    kind: "log",
    prompt: "Yük işaretini oku",
    title: "Tebeşir işareti",
    body: "İki kasa. İkisi de sancak iç halkada durunca pruva sürgüsü kalkar. Sıkışırsan sıfırlama levyesi kapının yanında.",
    flag: "a1_chalk",
  },
  {
    id: "a1_crate_a",
    act: 1,
    room: "a1_cargo",
    x: 3.2,
    z: 72,
    kind: "crate",
    prompt: "Kasayı it",
    slots: [
      [3.2, 72],
      [3.2, 76],
      [13, 76],
    ],
    goal: 2,
  },
  {
    id: "a1_crate_b",
    act: 1,
    room: "a1_cargo",
    x: 13,
    z: 70,
    kind: "crate",
    prompt: "Kasayı it",
    slots: [
      [13, 70],
      [8, 70],
      [13.2, 73.5],
    ],
    goal: 2,
  },
  {
    id: "a1_reset",
    act: 1,
    room: "a1_cargo",
    x: 8,
    z: 80,
    kind: "examine",
    prompt: "Kasaları sıfırla",
    title: "Levye",
    body: "Kasalar ilk yerlerine döndü.",
    flag: "a1_reset_ping",
  },
  {
    id: "a1_locket",
    act: 1,
    room: "a1_bow",
    x: 8,
    z: 100,
    kind: "examine",
    prompt: "Kolyeyi incele",
    title: "V kolye",
    body: "İç kapakta bir harf: V. Sis inceliyor. Mezarlık seni bekliyor.",
    flag: "a1_locket",
    once: true,
  },
  {
    id: "a1_exit",
    act: 1,
    room: "a1_bow",
    x: 8,
    z: 106,
    kind: "exit",
    prompt: "Sis kapısından geç",
    need: ["a1_crates", "a1_locket"],
    needMsg: "Önce kolye ve yük sürgüsü.",
    flag: "a1_left",
  },

  {
    id: "a2_log_boat",
    act: 2,
    room: "a2_boat",
    x: 3,
    z: 6,
    kind: "log",
    prompt: "Islak haritayı oku",
    title: "Mezarlık haritası",
    body: "Beş tekne, bir çan. Fener sayıları küçükten büyüğe: tek, iki, üç. La, Do, Mi. Moloz kalkmadan üçüncü geçit yok.",
    flag: "a2_map",
  },
  { id: "a2_oil_boat", act: 2, room: "a2_boat", x: 8, z: 8, kind: "oil", prompt: "Filika yağını al", oil: 38, flag: "a2_oil1", once: true },
  {
    id: "a2_t0",
    act: 2,
    room: "a2_b",
    x: 6,
    z: 62,
    kind: "bell",
    prompt: "Tek fener çanı",
    puzzle: "a2c",
    bell: 0,
    title: "Tek fener",
    body: "Bir fener. Nota La.",
    flag: "a2_heard0",
  },
  {
    id: "a2_log_b",
    act: 2,
    room: "a2_b",
    x: 2,
    z: 66,
    kind: "log",
    prompt: "Tekne adını oku",
    title: "Tek Fener",
    body: "Bu tekneye bir fener asılı. Çan teknesinde ilk sıra bunun.",
    flag: "a2_read0",
  },
  {
    id: "a2_crate",
    act: 2,
    room: "a2_c",
    x: 3,
    z: 114,
    kind: "crate",
    prompt: "Molozu it",
    slots: [
      [3, 114],
      [8, 114],
      [12, 118],
    ],
    goal: 2,
  },
  {
    id: "a2_log_c",
    act: 2,
    room: "a2_c",
    x: 10,
    z: 112,
    kind: "log",
    prompt: "Moloz notunu oku",
    title: "Devrilmiş direk",
    body: "Kasa sancak halkaya gidince geçit açılır. Sıkışırsan kasayı yeniden it; tur tamamlanır.",
    flag: "a2_readc",
  },
  {
    id: "a2_t2",
    act: 2,
    room: "a2_d",
    x: 6,
    z: 170,
    kind: "bell",
    prompt: "Üç fener çanı",
    puzzle: "a2c",
    bell: 2,
    body: "Üç fener. Nota Mi.",
    flag: "a2_heard2",
  },
  {
    id: "a2_log_d",
    act: 2,
    room: "a2_d",
    x: 2,
    z: 172,
    kind: "log",
    prompt: "Üç fener yazısını oku",
    title: "Üç Fener",
    body: "Üç fener, son nota. Mi. Önce tek, sonra iki, en son üç.",
    flag: "a2_read2",
  },
  { id: "a2_oil_e", act: 2, room: "a2_e", x: 4, z: 222, kind: "oil", prompt: "Karanlık yağ", oil: 40, flag: "a2_oil2", once: true },
  {
    id: "a2_t1",
    act: 2,
    room: "a2_e",
    x: 8,
    z: 224,
    kind: "bell",
    prompt: "İki fener çanı",
    puzzle: "a2c",
    bell: 1,
    body: "İki fener. Nota Do. Burada kalma.",
    flag: "a2_heard1",
  },
  {
    id: "a2_log_e",
    act: 2,
    room: "a2_e",
    x: 12,
    z: 226,
    kind: "log",
    prompt: "İki fener yazısını oku",
    title: "İki Fener",
    body: "İki fener, orta nota. Do. Denize bakarak buradan çıkma.",
    flag: "a2_read1",
  },
  {
    id: "a2_listen",
    act: 2,
    room: "a2_hulk",
    x: 4,
    z: 272,
    kind: "listen",
    prompt: "Hatırlanan notaları dinle",
    puzzle: "a2",
    need: ["a2_heard0", "a2_heard1", "a2_heard2"],
    needMsg: "Önce üç enkazın çanını bir kez çal.",
  },
  { id: "a2_h0", act: 2, room: "a2_hulk", x: 2, z: 276, kind: "bell", prompt: "La çanı", puzzle: "a2", bell: 0 },
  { id: "a2_h1", act: 2, room: "a2_hulk", x: 7, z: 276, kind: "bell", prompt: "Do çanı", puzzle: "a2", bell: 1 },
  { id: "a2_h2", act: 2, room: "a2_hulk", x: 12, z: 276, kind: "bell", prompt: "Mi çanı", puzzle: "a2", bell: 2 },
  {
    id: "a2_nail",
    act: 2,
    room: "a2_b",
    x: 10,
    z: 60,
    kind: "pickup",
    prompt: "Çiviyi sök",
    title: "Figür çivisi",
    body: "Figürbaşının altındaki çivi gevşedi. Sis kapısı bunu istiyor.",
    flag: "a2_nail",
    need: ["a2_bells"],
    needMsg: "Çivi sıkışmış. Çanlar doğru susunca gevşer.",
    once: true,
  },
  {
    id: "a2_exit",
    act: 2,
    room: "a2_gate",
    x: 6,
    z: 292,
    kind: "exit",
    prompt: "Liman sisinine gir",
    need: ["a2_nail"],
    needMsg: "Kapı çivisiz açılmaz.",
    flag: "a2_left",
  },

  { id: "a3_oil_dock", act: 3, room: "a3_dock", x: 4, z: 8, kind: "oil", prompt: "Rıhtım yağı", oil: 36, flag: "a3_oil1", once: true },
  {
    id: "a3_log_dock",
    act: 3,
    room: "a3_dock",
    x: 12,
    z: 8,
    kind: "log",
    prompt: "İskele levhasını oku",
    title: "Levha",
    body: "Ayak izleri fener ışığında parlar. Beş bina: gümrük, taverna, antrepo, şapel, ev. Sonra kule.",
    flag: "a3_board",
  },
  {
    id: "a3_customs",
    act: 3,
    room: "a3_customs",
    x: -4,
    z: 28,
    kind: "log",
    prompt: "Gümrük defterini oku",
    title: "Sis saati",
    body: "Defter net: sis 02:17'de indi. Gece yarısı diyenler içerideydi, güvertede değil.",
    flag: "a3_customs",
  },
  {
    id: "a3_tavern",
    act: 3,
    room: "a3_tavern",
    x: 16,
    z: 30,
    kind: "log",
    prompt: "Tezgâh notunu oku",
    title: "Taverna yalanı",
    body: "Biri 'sis gece yarısı' yazmış, sonra çizmiş. Doğru saat defterde. Buradaki saat süs.",
    flag: "a3_tavern",
  },
  { id: "a3_oil_w", act: 3, room: "a3_ware", x: -6, z: 50, kind: "oil", prompt: "Antrepo yağı", oil: 38, flag: "a3_oil2", once: true },
  {
    id: "a3_ware",
    act: 3,
    room: "a3_ware",
    x: -2,
    z: 52,
    kind: "examine",
    prompt: "Mühürlü sandığı incele",
    title: "Sandık",
    body: "İçinde silah yok. Islak ayakkabı, bir çocuk eldiveni, tuz. Kapakta şapelin mührü.",
    flag: "a3_ware",
    sanity: -BALANCE.examineCost,
    once: true,
  },
  {
    id: "a3_chapel",
    act: 3,
    room: "a3_chapel",
    x: 16,
    z: 52,
    kind: "log",
    prompt: "Şapel yazısını oku",
    title: "Kasaba saati",
    body: "Kasaba saatleri 11:40'ta durdu. Bu, geminin battığı saat değil. Kulede ikinci kadran bunu ister: kasabanın donduğu an.",
    flag: "a3_chapel",
  },
  {
    id: "a3_statue",
    act: 3,
    room: "a3_square",
    x: 8,
    z: 70,
    kind: "examine",
    prompt: "Heykeli incele",
    title: "Meydan heykeli",
    body: "Yüzü sana dönük değil. Arkanı döndüğünde omzu kayabilir. İtme. Sadece gör.",
    flag: "a3_statue",
    sanity: -2,
    once: true,
  },
  {
    id: "a3_house",
    act: 3,
    room: "a3_house",
    x: -4,
    z: 84,
    kind: "log",
    prompt: "Vanessa'nın saatini oku",
    title: "Cep saati",
    body: "Saat 06:05'te durmuş. Sabah vardiyası. Kulenin üçüncü kadranı bu. Ev boş, yatak yapılmış, pencere denize bakıyor.",
    flag: "a3_house",
  },
  {
    id: "a3_c1",
    act: 3,
    room: "a3_tower",
    x: 6,
    z: 106,
    kind: "clock",
    prompt: "Sis kadranı",
    options: ["00:00", "02:17", "11:40", "06:05"],
    answer: 1,
    need: clueNeed,
    needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku.",
  },
  {
    id: "a3_c2",
    act: 3,
    room: "a3_tower",
    x: 8.5,
    z: 108,
    kind: "clock",
    prompt: "Kasaba kadranı",
    options: ["02:17", "11:40", "06:05", "03:33"],
    answer: 1,
    need: clueNeed,
    needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku.",
  },
  {
    id: "a3_c3",
    act: 3,
    room: "a3_tower",
    x: 11,
    z: 106,
    kind: "clock",
    prompt: "Vardiya kadranı",
    options: ["11:40", "02:17", "06:05", "09:10"],
    answer: 2,
    need: clueNeed,
    needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku.",
  },
  {
    id: "a3_exit",
    act: 3,
    room: "a3_sea",
    x: 8,
    z: 122,
    kind: "exit",
    prompt: "Merdivenlerden in",
    need: ["a3_clocks"],
    needMsg: "Deniz duvarı kapalı.",
    flag: "a3_left",
  },

  {
    id: "a4_chest",
    act: 4,
    room: "a4_nave",
    x: 4,
    z: 6,
    kind: "pickup",
    prompt: "Sandığı aç",
    title: "Prizma",
    body: "Sandıktan soğuk bir cam prizma çıktı. Işın onsuz sunağa varmaz.",
    flag: "a4_prism",
    once: true,
  },
  {
    id: "a4_log_n",
    act: 4,
    room: "a4_nave",
    x: 14,
    z: 8,
    kind: "log",
    prompt: "Kaide yazısını oku",
    title: "Işın",
    body: "Dört ayna, dört çizik. Çiziğin gösterdiği yönde bir kez, iki kez, üç kez, bir kez çevir. Sıra bozulursa ışın söner. Galerideki mercek en son alınır.",
    flag: "a4_raynote",
  },
  { id: "a4_m1", act: 4, room: "a4_m1r", x: -4, z: 26, kind: "mirror", prompt: "Aynayı çevir", answer: 1 },
  { id: "a4_m2", act: 4, room: "a4_m2r", x: 18, z: 26, kind: "mirror", prompt: "Aynayı çevir", answer: 2 },
  { id: "a4_m3", act: 4, room: "a4_m3r", x: -4, z: 48, kind: "mirror", prompt: "Aynayı çevir", answer: 3 },
  { id: "a4_m4", act: 4, room: "a4_m4r", x: 18, z: 48, kind: "mirror", prompt: "Aynayı çevir", answer: 1 },
  { id: "a4_oil", act: 4, room: "a4_gal", x: 4, z: 64, kind: "oil", prompt: "Galeri yağı", oil: 40, flag: "a4_oil", once: true },
  {
    id: "a4_lens",
    act: 4,
    room: "a4_gal",
    x: 14,
    z: 66,
    kind: "pickup",
    prompt: "Merceği al",
    title: "Mercek",
    body: "Cam ısındı. Uzaktaki gölge pencerenin önünden geçiyor. Bakma fazla.",
    flag: "a4_lens",
    need: ["a4_m4"],
    needMsg: "Mercek soğuk. Önce dört ayna ışığı getirsin.",
    once: true,
  },
  {
    id: "a4_whale_note",
    act: 4,
    room: "a4_gal",
    x: 8,
    z: 62,
    kind: "log",
    prompt: "Pencere yazısını oku",
    title: "Pencere",
    body: "O bir düşman değil. İsim bile değil. Işık kesilirse yaklaşır, vurmaz. Feneri tut.",
    flag: "a4_whale_note",
  },
  {
    id: "a4_altar",
    act: 4,
    room: "a4_altar",
    x: 9,
    z: 94,
    kind: "altar",
    prompt: "Kolyeyi ve prizmayı bırak",
    need: ["a4_prism", "a4_lens", "a4_m1", "a4_m2", "a4_m3", "a4_m4", "a1_locket"],
    needMsg: "Sunak eksik: prizma, mercek, dört ayna ve kolye.",
    flag: "a4_ending",
  },
]

export const GOALS: Record<ActId, Goal[]> = {
  1: [
    { flag: "a1_oil_stern", text: "Kıçta yağ tulumunu bul. Feneri açık tut.", x: 3.2, z: 6.5 },
    { flag: "a1_log_stern", text: "Güverte defterini oku.", x: 11.5, z: 7 },
    { flag: "a1_music", text: "Aşağı in. Ninni odasında beş çanı tekrarla.", x: -2, z: 50 },
    { flag: "a1_letter", text: "Vanessa'nın kamarasındaki mektubu oku.", x: 14, z: 52 },
    { flag: "a1_hook", text: "Cerrah kamarasından kancayı al.", x: -2, z: 70 },
    { flag: "a1_hatch", text: "Pruvadaki kapağı kancayla kaldır.", x: 8, z: 48 },
    { flag: "a1_crates", text: "Yük ambarında iki kasayı sancak halkasına it.", x: 13, z: 76 },
    { flag: "a1_locket", text: "Pruvadaki kolyeyi incele.", x: 8, z: 100 },
    { flag: "a1_left", text: "Sis kapısından mezarlığa geç.", x: 8, z: 106 },
  ],
  2: [
    { flag: "a2_map", text: "Filikadaki haritayı oku.", x: 3, z: 6 },
    { flag: "a2_read0", text: "Tek Fener enkazının yazısını oku ve çanı çal.", x: 2, z: 66 },
    { flag: "a2_clear", text: "Moloz teknesinde kasayı halkaya it.", x: 12, z: 118 },
    { flag: "a2_read2", text: "Üç Fener yazısını oku, çanı çal.", x: 2, z: 172 },
    { flag: "a2_read1", text: "Karanlık ambardaki İki Fener yazısını oku, çanı çal.", x: 12, z: 226 },
    { flag: "a2_bells", text: "Çan teknesinde La, Do, Mi sırasını çal.", x: 7, z: 276 },
    { flag: "a2_nail", text: "Tek Fener enkazına dön. Çiviyi sök.", x: 10, z: 60 },
    { flag: "a2_left", text: "Sis kapısından limana gir.", x: 6, z: 292 },
  ],
  3: [
    { flag: "a3_board", text: "Rıhtım levhasını oku. İzleri fenerle takip et.", x: 12, z: 8 },
    { flag: "a3_customs", text: "Gümrük defterinden sis saatini öğren.", x: -4, z: 28 },
    { flag: "a3_tavern", text: "Tavernadaki yalan notu oku.", x: 16, z: 30 },
    { flag: "a3_ware", text: "Antrepodaki sandığı incele.", x: -2, z: 52 },
    { flag: "a3_chapel", text: "Şapelde kasaba saatini oku.", x: 16, z: 52 },
    { flag: "a3_house", text: "Vanessa'nın evindeki cep saatini oku.", x: -4, z: 84 },
    { flag: "a3_clocks", text: "Kulede üç kadranı doğru saatlere getir.", x: 8.5, z: 108 },
    { flag: "a3_left", text: "Deniz duvarından tapınağa in.", x: 8, z: 122 },
  ],
  4: [
    { flag: "a4_prism", text: "Narteksteki sandıktan prizmayı al.", x: 4, z: 6 },
    { flag: "a4_raynote", text: "Kaide yazısını oku.", x: 14, z: 8 },
    { flag: "a4_m1", text: "Birinci aynayı çiziğe göre bir kez çevir.", x: -4, z: 26 },
    { flag: "a4_m2", text: "İkinci aynayı iki kez çevir.", x: 18, z: 26 },
    { flag: "a4_m3", text: "Üçüncü aynayı üç kez çevir.", x: -4, z: 48 },
    { flag: "a4_m4", text: "Dördüncü aynayı bir kez çevir.", x: 18, z: 48 },
    { flag: "a4_lens", text: "Balina galerisinden merceği al. Uzağa fazla bakma.", x: 14, z: 66 },
    { flag: "a4_ending", text: "Sunakta kolyeyi ve prizmayı bırak.", x: 9, z: 94 },
  ],
}

export function roomContains(room: Room, x: number, z: number): boolean {
  return x >= room.x && x <= room.x + room.w && z >= room.z && z <= room.z + room.d
}

export function validateContent(): string[] {
  const errors: string[] = []
  for (const item of ITEMS) {
    const rooms = ROOMS.filter((r) => r.act === item.act)
    if (!rooms.some((r) => roomContains(r, item.x, item.z))) errors.push(`item ${item.id} outside`)
    if (item.slots) {
      for (const [x, z] of item.slots) {
        if (!rooms.some((r) => roomContains(r, x, z))) errors.push(`slot ${item.id} outside`)
      }
    }
  }
  for (const act of [1, 2, 3, 4] as ActId[]) {
    const s = SPAWN[act]
    const open = ROOMS.filter((r) => r.act === act && !r.openFlag)
    if (!open.some((r) => roomContains(r, s.x, s.z))) errors.push(`spawn ${act} outside`)
    for (const g of GOALS[act]) {
      if (!ROOMS.some((r) => r.act === act && roomContains(r, g.x, g.z))) errors.push(`goal ${g.flag} outside`)
    }
  }
  return errors
}
