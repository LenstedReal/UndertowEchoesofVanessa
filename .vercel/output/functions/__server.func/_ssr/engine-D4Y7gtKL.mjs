import { C as Timer, S as SphereGeometry, T as Vector3, _ as PointLight, a as Color, b as Scene, c as DirectionalLight, d as HemisphereLight, f as Mesh, g as PlaneGeometry, h as PerspectiveCamera, i as CanvasTexture, l as FogExp2, m as MeshStandardMaterial, n as WebGLRenderer, o as ConeGeometry, p as MeshBasicMaterial, r as BoxGeometry, s as CylinderGeometry, t as mergeGeometries, u as Group, v as RepeatWrapping, w as TorusGeometry, x as ShaderMaterial, y as SRGBColorSpace } from "../_libs/three.mjs";
//#region node_modules/.nitro/vite/services/ssr/assets/engine-D4Y7gtKL.js
var ACTS = {
	1: {
		name: "Lanetli Kalyon",
		line: "Sis gemiyi yuttu. Vanessa güvertede değil.",
		fog: .052,
		fogColor: 5135714
	},
	2: {
		name: "Gemi Mezarlığı",
		line: "Enkazlar bir koridor gibi dizilmiş. Yanlış yöne giden yol da sis.",
		fog: .04,
		fogColor: 3820116
	},
	3: {
		name: "Terk Edilmiş Liman",
		line: "Saatler durmuş. Diz boyu suda Vanessa'nın izi var.",
		fog: .036,
		fogColor: 4082514
	},
	4: {
		name: "Batık Tapınak",
		line: "Işık aşağı iniyor. Uzakta bir şey nefes alıyor.",
		fog: .046,
		fogColor: 932932
	}
};
var SPAWN = {
	1: {
		x: 6.2,
		z: 4.2
	},
	2: {
		x: 6,
		z: 5
	},
	3: {
		x: 8,
		z: 6
	},
	4: {
		x: 9,
		z: 5
	}
};
var A1_SEQ = [
	0,
	2,
	1,
	2,
	4
];
var A1_NOTES = [
	440,
	659.25,
	523.25,
	659.25,
	880
];
var A1_NAMES = [
	"La",
	"Mi",
	"Do",
	"Mi",
	"La"
];
var A2_SEQ = [
	0,
	1,
	2
];
var A2_NOTES = [
	440,
	523.25,
	659.25
];
var A2_NAMES = [
	"La",
	"Do",
	"Mi"
];
var BALANCE = {
	darkDrain: .8,
	moonDrain: .22,
	seaDrain: 1.2,
	seaDelay: 6,
	safeRegen: 1,
	examineCost: 4,
	musicReward: 15,
	oilBurn: .075,
	oilSprint: .05,
	startOil: 64,
	oilFlask: 42,
	hysteresis: 3
};
var ROOMS = [
	{
		id: "a1_stern",
		act: 1,
		name: "Kıç güverte",
		x: 0,
		z: 0,
		w: 16,
		d: 18,
		floor: 0,
		ceil: false,
		kind: "deck",
		ambient: .55
	},
	{
		id: "a1_main",
		act: 1,
		name: "Ana güverte",
		x: 0,
		z: 17,
		w: 16,
		d: 26,
		floor: 0,
		ceil: false,
		kind: "deck",
		ambient: .5,
		landmark: "mast"
	},
	{
		id: "a1_fore",
		act: 1,
		name: "Pruva",
		x: 1,
		z: 40,
		w: 14,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "deck",
		ambient: .48
	},
	{
		id: "a1_well",
		act: 1,
		name: "Kıç merdiveni",
		x: 6,
		z: 10,
		w: 4,
		d: 16,
		floor: 0,
		floorTo: -4,
		ceil: false,
		kind: "hall",
		ambient: .3
	},
	{
		id: "a1_spine",
		act: 1,
		name: "Top güvertesi",
		x: 3,
		z: 22,
		w: 7,
		d: 42,
		floor: -4,
		ceil: true,
		kind: "hold",
		ambient: .07
	},
	{
		id: "a1_crew",
		act: 1,
		name: "Tayfa kamarası",
		x: -6,
		z: 30,
		w: 12,
		d: 12,
		floor: -4,
		ceil: true,
		kind: "cabin",
		ambient: .1
	},
	{
		id: "a1_music",
		act: 1,
		name: "Ninni odası",
		x: -8,
		z: 46,
		w: 14,
		d: 12,
		floor: -4,
		ceil: true,
		kind: "cabin",
		ambient: .12,
		safe: true
	},
	{
		id: "a1_nessa",
		act: 1,
		name: "Vanessa'nın kamarası",
		x: 8,
		z: 46,
		w: 12,
		d: 12,
		floor: -4,
		ceil: true,
		kind: "cabin",
		ambient: .1
	},
	{
		id: "a1_surg",
		act: 1,
		name: "Cerrah kamarası",
		x: -8,
		z: 62,
		w: 14,
		d: 12,
		floor: -4,
		ceil: true,
		kind: "cabin",
		ambient: .06
	},
	{
		id: "a1_stair",
		act: 1,
		name: "Pruva ambarı",
		x: 8,
		z: 50,
		w: 4,
		d: 22,
		floor: 0,
		floorTo: -4,
		ceil: true,
		kind: "hall",
		ambient: .08,
		openFlag: "a1_hatch"
	},
	{
		id: "a1_cargo",
		act: 1,
		name: "Yük ambarı",
		x: 1,
		z: 68,
		w: 16,
		d: 16,
		floor: -4,
		ceil: true,
		kind: "hold",
		ambient: .08
	},
	{
		id: "a1_bowramp",
		act: 1,
		name: "Pruva çıkışı",
		x: 7,
		z: 80,
		w: 4,
		d: 18,
		floor: -4,
		floorTo: 0,
		ceil: true,
		kind: "hall",
		ambient: .2
	},
	{
		id: "a1_bow",
		act: 1,
		name: "Pruva ucu",
		x: 1,
		z: 94,
		w: 14,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "deck",
		ambient: .5
	},
	{
		id: "a2_boat",
		act: 2,
		name: "Filika iskelesi",
		x: 0,
		z: 0,
		w: 12,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .42
	},
	{
		id: "a2_g1",
		act: 2,
		name: "İlk geçit",
		x: 4,
		z: 15,
		w: 4,
		d: 40,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .36
	},
	{
		id: "a2_b",
		act: 2,
		name: "Tek Fener enkazı",
		x: -2,
		z: 54,
		w: 16,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .34
	},
	{
		id: "a2_g2",
		act: 2,
		name: "İkinci geçit",
		x: 4,
		z: 69,
		w: 4,
		d: 40,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .32
	},
	{
		id: "a2_c",
		act: 2,
		name: "Moloz teknesi",
		x: -2,
		z: 108,
		w: 18,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .3
	},
	{
		id: "a2_g3",
		act: 2,
		name: "Üçüncü geçit",
		x: 4,
		z: 123,
		w: 4,
		d: 40,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .3
	},
	{
		id: "a2_d",
		act: 2,
		name: "Üç Fener enkazı",
		x: -2,
		z: 162,
		w: 16,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .33
	},
	{
		id: "a2_g4",
		act: 2,
		name: "Karanlık geçit",
		x: 4,
		z: 177,
		w: 4,
		d: 40,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .22
	},
	{
		id: "a2_e",
		act: 2,
		name: "İki Fener ambarı",
		x: -4,
		z: 216,
		w: 20,
		d: 16,
		floor: 0,
		ceil: true,
		kind: "hold",
		ambient: .08
	},
	{
		id: "a2_g5",
		act: 2,
		name: "Son geçit",
		x: 4,
		z: 231,
		w: 4,
		d: 36,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .3
	},
	{
		id: "a2_hulk",
		act: 2,
		name: "Çan teknesi",
		x: -2,
		z: 266,
		w: 18,
		d: 18,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .28
	},
	{
		id: "a2_gate",
		act: 2,
		name: "Sis kapısı",
		x: 2,
		z: 280,
		w: 10,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "grave",
		ambient: .4
	},
	{
		id: "a3_dock",
		act: 3,
		name: "Rıhtım",
		x: 0,
		z: 0,
		w: 18,
		d: 18,
		floor: 0,
		ceil: false,
		kind: "street",
		ambient: .4
	},
	{
		id: "a3_street",
		act: 3,
		name: "Su sokağı",
		x: 4,
		z: 16,
		w: 8,
		d: 96,
		floor: 0,
		ceil: false,
		kind: "street",
		ambient: .34
	},
	{
		id: "a3_customs",
		act: 3,
		name: "Gümrük",
		x: -12,
		z: 22,
		w: 18,
		d: 14,
		floor: 0,
		ceil: true,
		kind: "cabin",
		ambient: .16
	},
	{
		id: "a3_tavern",
		act: 3,
		name: "Taverna",
		x: 10,
		z: 24,
		w: 14,
		d: 14,
		floor: 0,
		ceil: true,
		kind: "cabin",
		ambient: .14
	},
	{
		id: "a3_ware",
		act: 3,
		name: "Antrepo",
		x: -14,
		z: 44,
		w: 20,
		d: 14,
		floor: 0,
		ceil: true,
		kind: "hold",
		ambient: .1
	},
	{
		id: "a3_chapel",
		act: 3,
		name: "Şapel",
		x: 10,
		z: 46,
		w: 16,
		d: 14,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .18,
		safe: true
	},
	{
		id: "a3_square",
		act: 3,
		name: "Meydan",
		x: 0,
		z: 62,
		w: 18,
		d: 16,
		floor: 0,
		ceil: false,
		kind: "street",
		ambient: .36,
		landmark: "clock"
	},
	{
		id: "a3_house",
		act: 3,
		name: "Vanessa'nın evi",
		x: -14,
		z: 78,
		w: 20,
		d: 14,
		floor: 0,
		ceil: true,
		kind: "cabin",
		ambient: .12
	},
	{
		id: "a3_tower",
		act: 3,
		name: "Saat kulesi",
		x: 3,
		z: 100,
		w: 12,
		d: 16,
		floor: 0,
		ceil: true,
		kind: "hall",
		ambient: .15,
		landmark: "clock"
	},
	{
		id: "a3_sea",
		act: 3,
		name: "Deniz duvarı",
		x: 5,
		z: 108,
		w: 8,
		d: 20,
		floor: 0,
		ceil: false,
		kind: "street",
		ambient: .3,
		openFlag: "a3_clocks"
	},
	{
		id: "a4_nave",
		act: 4,
		name: "Narteks",
		x: 0,
		z: 0,
		w: 18,
		d: 16,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .2,
		safe: true,
		landmark: "columns"
	},
	{
		id: "a4_hall",
		act: 4,
		name: "Yanık hol",
		x: 5,
		z: 14,
		w: 8,
		d: 28,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .12,
		landmark: "columns"
	},
	{
		id: "a4_m1r",
		act: 4,
		name: "Birinci ayna",
		x: -10,
		z: 20,
		w: 18,
		d: 12,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .1
	},
	{
		id: "a4_m2r",
		act: 4,
		name: "İkinci ayna",
		x: 10,
		z: 20,
		w: 16,
		d: 12,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .1
	},
	{
		id: "a4_cross",
		act: 4,
		name: "Haç holü",
		x: 4,
		z: 40,
		w: 10,
		d: 18,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .12,
		landmark: "columns"
	},
	{
		id: "a4_m3r",
		act: 4,
		name: "Üçüncü ayna",
		x: -10,
		z: 42,
		w: 18,
		d: 12,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .1
	},
	{
		id: "a4_m4r",
		act: 4,
		name: "Dördüncü ayna",
		x: 10,
		z: 42,
		w: 16,
		d: 12,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .1
	},
	{
		id: "a4_gal",
		act: 4,
		name: "Balina galerisi",
		x: 0,
		z: 56,
		w: 20,
		d: 18,
		floor: 0,
		ceil: true,
		kind: "temple",
		ambient: .16,
		landmark: "whale"
	},
	{
		id: "a4_ramp",
		act: 4,
		name: "Sunak inişi",
		x: 6,
		z: 72,
		w: 6,
		d: 16,
		floor: 0,
		floorTo: -1.2,
		ceil: true,
		kind: "hall",
		ambient: .14
	},
	{
		id: "a4_altar",
		act: 4,
		name: "Sunak",
		x: 2,
		z: 86,
		w: 14,
		d: 16,
		floor: -1.2,
		ceil: true,
		kind: "temple",
		ambient: .22,
		landmark: "columns"
	}
];
var BLOCKERS = [
	{
		id: "b_nessa",
		act: 1,
		x: 7.4,
		z: 45.6,
		w: 3.2,
		d: 12.6,
		until: "a1_music"
	},
	{
		id: "b_surg",
		act: 1,
		x: 2.4,
		z: 61.6,
		w: 5.4,
		d: 12.6,
		until: "a1_letter"
	},
	{
		id: "b_bow",
		act: 1,
		x: 6.2,
		z: 79.2,
		w: 5.2,
		d: 2.4,
		until: "a1_crates"
	},
	{
		id: "b_g3",
		act: 2,
		x: 3.2,
		z: 122.2,
		w: 5.4,
		d: 2.6,
		until: "a2_clear"
	},
	{
		id: "b_gate",
		act: 2,
		x: 2.2,
		z: 282.4,
		w: 9.4,
		d: 2.2,
		until: "a2_nail"
	}
];
var clueNeed = [
	"a3_customs",
	"a3_tavern",
	"a3_ware",
	"a3_chapel",
	"a3_house"
];
var ITEMS = [
	{
		id: "a1_oil_stern",
		act: 1,
		room: "a1_stern",
		x: 3.2,
		z: 6.5,
		kind: "oil",
		prompt: "Yağ tulumunu al",
		oil: BALANCE.oilFlask,
		flag: "a1_oil_stern",
		once: true
	},
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
		flag: "a1_log_stern"
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
		once: true
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
		once: true
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
		flag: "a1_crewnote"
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
		body: "Kutu aynı beş notayı çalar. Zamanlama yok. Sırayı çanlarda tekrarla."
	},
	{
		id: "a1_b0",
		act: 1,
		room: "a1_music",
		x: -5.5,
		z: 53,
		kind: "bell",
		prompt: "İlk çan",
		puzzle: "a1",
		bell: 0
	},
	{
		id: "a1_b1",
		act: 1,
		room: "a1_music",
		x: -2.5,
		z: 53.4,
		kind: "bell",
		prompt: "İkinci çan",
		puzzle: "a1",
		bell: 1
	},
	{
		id: "a1_b2",
		act: 1,
		room: "a1_music",
		x: .4,
		z: 53,
		kind: "bell",
		prompt: "Üçüncü çan",
		puzzle: "a1",
		bell: 2
	},
	{
		id: "a1_b3",
		act: 1,
		room: "a1_music",
		x: 3.2,
		z: 53.3,
		kind: "bell",
		prompt: "Dördüncü çan",
		puzzle: "a1",
		bell: 3
	},
	{
		id: "a1_b4",
		act: 1,
		room: "a1_music",
		x: 1.2,
		z: 56,
		kind: "bell",
		prompt: "Beşinci çan",
		puzzle: "a1",
		bell: 4
	},
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
		flag: "a1_letter"
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
		once: true
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
		once: true
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
		once: true
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
		once: true
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
		flag: "a1_chalk"
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
			[13, 76]
		],
		goal: 2
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
			[13.2, 73.5]
		],
		goal: 2
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
		flag: "a1_reset_ping"
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
		once: true
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
		flag: "a1_left"
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
		flag: "a2_map"
	},
	{
		id: "a2_oil_boat",
		act: 2,
		room: "a2_boat",
		x: 8,
		z: 8,
		kind: "oil",
		prompt: "Filika yağını al",
		oil: 38,
		flag: "a2_oil1",
		once: true
	},
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
		flag: "a2_heard0"
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
		flag: "a2_read0"
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
			[12, 118]
		],
		goal: 2
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
		flag: "a2_readc"
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
		flag: "a2_heard2"
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
		flag: "a2_read2"
	},
	{
		id: "a2_oil_e",
		act: 2,
		room: "a2_e",
		x: 4,
		z: 222,
		kind: "oil",
		prompt: "Karanlık yağ",
		oil: 40,
		flag: "a2_oil2",
		once: true
	},
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
		flag: "a2_heard1"
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
		flag: "a2_read1"
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
		need: [
			"a2_heard0",
			"a2_heard1",
			"a2_heard2"
		],
		needMsg: "Önce üç enkazın çanını bir kez çal."
	},
	{
		id: "a2_h0",
		act: 2,
		room: "a2_hulk",
		x: 2,
		z: 276,
		kind: "bell",
		prompt: "La çanı",
		puzzle: "a2",
		bell: 0
	},
	{
		id: "a2_h1",
		act: 2,
		room: "a2_hulk",
		x: 7,
		z: 276,
		kind: "bell",
		prompt: "Do çanı",
		puzzle: "a2",
		bell: 1
	},
	{
		id: "a2_h2",
		act: 2,
		room: "a2_hulk",
		x: 12,
		z: 276,
		kind: "bell",
		prompt: "Mi çanı",
		puzzle: "a2",
		bell: 2
	},
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
		once: true
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
		flag: "a2_left"
	},
	{
		id: "a3_oil_dock",
		act: 3,
		room: "a3_dock",
		x: 4,
		z: 8,
		kind: "oil",
		prompt: "Rıhtım yağı",
		oil: 36,
		flag: "a3_oil1",
		once: true
	},
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
		flag: "a3_board"
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
		flag: "a3_customs"
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
		flag: "a3_tavern"
	},
	{
		id: "a3_oil_w",
		act: 3,
		room: "a3_ware",
		x: -6,
		z: 50,
		kind: "oil",
		prompt: "Antrepo yağı",
		oil: 38,
		flag: "a3_oil2",
		once: true
	},
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
		once: true
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
		flag: "a3_chapel"
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
		once: true
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
		flag: "a3_house"
	},
	{
		id: "a3_c1",
		act: 3,
		room: "a3_tower",
		x: 6,
		z: 106,
		kind: "clock",
		prompt: "Sis kadranı",
		options: [
			"00:00",
			"02:17",
			"11:40",
			"06:05"
		],
		answer: 1,
		need: clueNeed,
		needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku."
	},
	{
		id: "a3_c2",
		act: 3,
		room: "a3_tower",
		x: 8.5,
		z: 108,
		kind: "clock",
		prompt: "Kasaba kadranı",
		options: [
			"02:17",
			"11:40",
			"06:05",
			"03:33"
		],
		answer: 1,
		need: clueNeed,
		needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku."
	},
	{
		id: "a3_c3",
		act: 3,
		room: "a3_tower",
		x: 11,
		z: 106,
		kind: "clock",
		prompt: "Vardiya kadranı",
		options: [
			"11:40",
			"02:17",
			"06:05",
			"09:10"
		],
		answer: 2,
		need: clueNeed,
		needMsg: "Kadran kilitli. Önce beş binadaki yazıları oku."
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
		flag: "a3_left"
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
		once: true
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
		flag: "a4_raynote"
	},
	{
		id: "a4_m1",
		act: 4,
		room: "a4_m1r",
		x: -4,
		z: 26,
		kind: "mirror",
		prompt: "Aynayı çevir",
		answer: 1
	},
	{
		id: "a4_m2",
		act: 4,
		room: "a4_m2r",
		x: 18,
		z: 26,
		kind: "mirror",
		prompt: "Aynayı çevir",
		answer: 2
	},
	{
		id: "a4_m3",
		act: 4,
		room: "a4_m3r",
		x: -4,
		z: 48,
		kind: "mirror",
		prompt: "Aynayı çevir",
		answer: 3
	},
	{
		id: "a4_m4",
		act: 4,
		room: "a4_m4r",
		x: 18,
		z: 48,
		kind: "mirror",
		prompt: "Aynayı çevir",
		answer: 1
	},
	{
		id: "a4_oil",
		act: 4,
		room: "a4_gal",
		x: 4,
		z: 64,
		kind: "oil",
		prompt: "Galeri yağı",
		oil: 40,
		flag: "a4_oil",
		once: true
	},
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
		once: true
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
		flag: "a4_whale_note"
	},
	{
		id: "a4_altar",
		act: 4,
		room: "a4_altar",
		x: 9,
		z: 94,
		kind: "altar",
		prompt: "Kolyeyi ve prizmayı bırak",
		need: [
			"a4_prism",
			"a4_lens",
			"a4_m1",
			"a4_m2",
			"a4_m3",
			"a4_m4",
			"a1_locket"
		],
		needMsg: "Sunak eksik: prizma, mercek, dört ayna ve kolye.",
		flag: "a4_ending"
	}
];
var GOALS = {
	1: [
		{
			flag: "a1_oil_stern",
			text: "Kıçta yağ tulumunu bul. Feneri açık tut.",
			x: 3.2,
			z: 6.5
		},
		{
			flag: "a1_log_stern",
			text: "Güverte defterini oku.",
			x: 11.5,
			z: 7
		},
		{
			flag: "a1_music",
			text: "Aşağı in. Ninni odasında beş çanı tekrarla.",
			x: -2,
			z: 50
		},
		{
			flag: "a1_letter",
			text: "Vanessa'nın kamarasındaki mektubu oku.",
			x: 14,
			z: 52
		},
		{
			flag: "a1_hook",
			text: "Cerrah kamarasından kancayı al.",
			x: -2,
			z: 70
		},
		{
			flag: "a1_hatch",
			text: "Pruvadaki kapağı kancayla kaldır.",
			x: 8,
			z: 48
		},
		{
			flag: "a1_crates",
			text: "Yük ambarında iki kasayı sancak halkasına it.",
			x: 13,
			z: 76
		},
		{
			flag: "a1_locket",
			text: "Pruvadaki kolyeyi incele.",
			x: 8,
			z: 100
		},
		{
			flag: "a1_left",
			text: "Sis kapısından mezarlığa geç.",
			x: 8,
			z: 106
		}
	],
	2: [
		{
			flag: "a2_map",
			text: "Filikadaki haritayı oku.",
			x: 3,
			z: 6
		},
		{
			flag: "a2_read0",
			text: "Tek Fener enkazının yazısını oku ve çanı çal.",
			x: 2,
			z: 66
		},
		{
			flag: "a2_clear",
			text: "Moloz teknesinde kasayı halkaya it.",
			x: 12,
			z: 118
		},
		{
			flag: "a2_read2",
			text: "Üç Fener yazısını oku, çanı çal.",
			x: 2,
			z: 172
		},
		{
			flag: "a2_read1",
			text: "Karanlık ambardaki İki Fener yazısını oku, çanı çal.",
			x: 12,
			z: 226
		},
		{
			flag: "a2_bells",
			text: "Çan teknesinde La, Do, Mi sırasını çal.",
			x: 7,
			z: 276
		},
		{
			flag: "a2_nail",
			text: "Tek Fener enkazına dön. Çiviyi sök.",
			x: 10,
			z: 60
		},
		{
			flag: "a2_left",
			text: "Sis kapısından limana gir.",
			x: 6,
			z: 292
		}
	],
	3: [
		{
			flag: "a3_board",
			text: "Rıhtım levhasını oku. İzleri fenerle takip et.",
			x: 12,
			z: 8
		},
		{
			flag: "a3_customs",
			text: "Gümrük defterinden sis saatini öğren.",
			x: -4,
			z: 28
		},
		{
			flag: "a3_tavern",
			text: "Tavernadaki yalan notu oku.",
			x: 16,
			z: 30
		},
		{
			flag: "a3_ware",
			text: "Antrepodaki sandığı incele.",
			x: -2,
			z: 52
		},
		{
			flag: "a3_chapel",
			text: "Şapelde kasaba saatini oku.",
			x: 16,
			z: 52
		},
		{
			flag: "a3_house",
			text: "Vanessa'nın evindeki cep saatini oku.",
			x: -4,
			z: 84
		},
		{
			flag: "a3_clocks",
			text: "Kulede üç kadranı doğru saatlere getir.",
			x: 8.5,
			z: 108
		},
		{
			flag: "a3_left",
			text: "Deniz duvarından tapınağa in.",
			x: 8,
			z: 122
		}
	],
	4: [
		{
			flag: "a4_prism",
			text: "Narteksteki sandıktan prizmayı al.",
			x: 4,
			z: 6
		},
		{
			flag: "a4_raynote",
			text: "Kaide yazısını oku.",
			x: 14,
			z: 8
		},
		{
			flag: "a4_m1",
			text: "Birinci aynayı çiziğe göre bir kez çevir.",
			x: -4,
			z: 26
		},
		{
			flag: "a4_m2",
			text: "İkinci aynayı iki kez çevir.",
			x: 18,
			z: 26
		},
		{
			flag: "a4_m3",
			text: "Üçüncü aynayı üç kez çevir.",
			x: -4,
			z: 48
		},
		{
			flag: "a4_m4",
			text: "Dördüncü aynayı bir kez çevir.",
			x: 18,
			z: 48
		},
		{
			flag: "a4_lens",
			text: "Balina galerisinden merceği al. Uzağa fazla bakma.",
			x: 14,
			z: 66
		},
		{
			flag: "a4_ending",
			text: "Sunakta kolyeyi ve prizmayı bırak.",
			x: 9,
			z: 94
		}
	]
};
function roomContains(room, x, z) {
	return x >= room.x && x <= room.x + room.w && z >= room.z && z <= room.z + room.d;
}
var GameAudio = class {
	ctx = null;
	master = null;
	wind = null;
	windGain = null;
	drone = null;
	droneGain = null;
	volume = .8;
	unlock() {
		const AC = window.AudioContext || window.webkitAudioContext;
		if (!this.ctx) {
			const ctx = new AC({ latencyHint: "interactive" });
			const master = ctx.createGain();
			master.gain.value = this.volume * this.volume;
			master.connect(ctx.destination);
			const windGain = ctx.createGain();
			windGain.gain.value = 0;
			windGain.connect(master);
			const len = ctx.sampleRate * 2;
			const buf = ctx.createBuffer(1, len, ctx.sampleRate);
			const data = buf.getChannelData(0);
			let last = 0;
			for (let i = 0; i < len; i++) {
				const white = Math.random() * 2 - 1;
				last = last * .96 + white * .04;
				data[i] = last * 2.2;
			}
			const src = ctx.createBufferSource();
			src.buffer = buf;
			src.loop = true;
			const filter = ctx.createBiquadFilter();
			filter.type = "lowpass";
			filter.frequency.value = 400;
			src.connect(filter);
			filter.connect(windGain);
			src.start();
			const drone = ctx.createOscillator();
			drone.type = "sine";
			drone.frequency.value = 55;
			const droneGain = ctx.createGain();
			droneGain.gain.value = 0;
			drone.connect(droneGain);
			droneGain.connect(master);
			drone.start();
			this.ctx = ctx;
			this.master = master;
			this.wind = src;
			this.windGain = windGain;
			this.drone = drone;
			this.droneGain = droneGain;
		}
		if (this.ctx.state === "suspended") this.ctx.resume();
	}
	setVolume(v) {
		this.volume = v;
		if (!this.ctx || !this.master) return;
		this.master.gain.setTargetAtTime(v * v, this.ctx.currentTime, .05);
	}
	setWind(amount) {
		if (!this.ctx || !this.windGain) return;
		this.windGain.gain.setTargetAtTime(amount * .25, this.ctx.currentTime, .1);
	}
	setDrone(amount) {
		if (!this.ctx || !this.droneGain) return;
		this.droneGain.gain.setTargetAtTime(amount * .08, this.ctx.currentTime, .2);
	}
	tone(freq, dur = .45, type = "sine", gain = .12) {
		if (!this.ctx || !this.master) return;
		const t = this.ctx.currentTime;
		const osc = this.ctx.createOscillator();
		const g = this.ctx.createGain();
		osc.type = type;
		osc.frequency.value = freq;
		g.gain.setValueAtTime(1e-4, t);
		g.gain.exponentialRampToValueAtTime(gain, t + .02);
		g.gain.exponentialRampToValueAtTime(1e-4, t + dur);
		osc.connect(g);
		g.connect(this.master);
		osc.start(t);
		osc.stop(t + dur + .02);
	}
	step() {
		this.tone(90 + Math.random() * 30, .08, "triangle", .05);
	}
	creak() {
		this.tone(140 + Math.random() * 40, .25, "sawtooth", .03);
	}
	wrong() {
		this.tone(110, .2, "square", .05);
	}
	chime() {
		this.tone(523, .3, "sine", .08);
		window.setTimeout(() => this.tone(659, .4, "sine", .08), 140);
	}
	dispose() {
		this.wind?.stop();
		this.drone?.stop();
		this.ctx?.close();
		this.ctx = null;
	}
};
var SAVE_KEY = "undertow-echoes-v1";
var SET_KEY = "undertow-echoes-settings";
var defaultSettings = () => ({
	sens: 1,
	volume: .75,
	subtitles: true,
	motion: true,
	haptics: false,
	quality: "medium"
});
function loadSettings() {
	try {
		const raw = localStorage.getItem(SET_KEY);
		if (!raw) return defaultSettings();
		return {
			...defaultSettings(),
			...JSON.parse(raw)
		};
	} catch {
		return defaultSettings();
	}
}
function writeSettings(s) {
	localStorage.setItem(SET_KEY, JSON.stringify(s));
}
function loadSave() {
	try {
		const raw = localStorage.getItem(SAVE_KEY);
		if (!raw) return null;
		const data = JSON.parse(raw);
		if (data.version !== 1) return null;
		return data;
	} catch {
		return null;
	}
}
function writeSave(data) {
	localStorage.setItem(SAVE_KEY, JSON.stringify(data));
}
function clearSave() {
	localStorage.removeItem(SAVE_KEY);
}
var R = .3;
function updateTier(sanity, prev) {
	if (prev === "stable") return sanity < 70 - BALANCE.hysteresis ? "uneasy" : "stable";
	if (prev === "uneasy") {
		if (sanity >= 70 + BALANCE.hysteresis) return "stable";
		if (sanity < 45 - BALANCE.hysteresis) return "disturbed";
		return "uneasy";
	}
	if (prev === "disturbed") {
		if (sanity >= 45 + BALANCE.hysteresis) return "uneasy";
		if (sanity < 20 - BALANCE.hysteresis) return "breaking";
		return "disturbed";
	}
	return sanity >= 20 + BALANCE.hysteresis ? "disturbed" : "breaking";
}
function woodTexture() {
	const c = document.createElement("canvas");
	c.width = 256;
	c.height = 256;
	const g = c.getContext("2d");
	if (!g) return null;
	g.fillStyle = "#1a120e";
	g.fillRect(0, 0, 256, 256);
	for (let i = 0; i < 16; i++) {
		const y = i * 16;
		g.fillStyle = i % 2 ? "#2a1c14" : "#140e0b";
		g.fillRect(0, y, 256, 15);
		g.fillStyle = "#3a3028";
		g.fillRect(18, y + 6, 2, 2);
		g.fillRect(210, y + 7, 2, 2);
	}
	const tex = new CanvasTexture(c);
	tex.wrapS = RepeatWrapping;
	tex.wrapT = RepeatWrapping;
	tex.colorSpace = SRGBColorSpace;
	return tex;
}
function createGame(canvas, onHud) {
	const audio = new GameAudio();
	const settings = loadSettings();
	audio.volume = settings.volume;
	const flags = /* @__PURE__ */ new Set();
	const crates = {};
	const clocks = {};
	const mirrors = {};
	const logs = [];
	let mode = "menu";
	let settingsFrom = "menu";
	let act = 1;
	let x = SPAWN[1].x;
	let z = SPAWN[1].z;
	let yaw = Math.PI;
	let pitch = -.08;
	let sanity = 100;
	let tier = "stable";
	let oil = BALANCE.startOil;
	let lanternOn = true;
	let reading = null;
	let subtitle = null;
	let subT = 0;
	let cardTitle = "";
	let cardLine = "";
	let webgl = true;
	let roomName = "";
	let seaT = 0;
	let stableHold = 0;
	let a1p = 0;
	let a2p = 0;
	let stepAcc = 0;
	let saveAcc = 0;
	let distortAcc = 5;
	let portraitOff = 0;
	let safeX = x;
	let safeZ = z;
	let safeYaw = yaw;
	let horizSpeed = 0;
	let stickX = 0;
	let stickY = 0;
	let sprintHeld = false;
	let lookX = 0;
	let lookY = 0;
	let probe = null;
	const keys = /* @__PURE__ */ new Set();
	let menuT = 0;
	let warnedWind = false;
	let warnedOil = false;
	let warnedSea = false;
	const renderer = new WebGLRenderer({
		canvas,
		antialias: false,
		powerPreference: "high-performance"
	});
	renderer.setClearColor(5135714, 1);
	renderer.outputColorSpace = SRGBColorSpace;
	const scene = new Scene();
	const camera = new PerspectiveCamera(72, 1, .08, 90);
	camera.rotation.order = "YXZ";
	const clock = new Timer();
	clock.connect(document);
	const hemi = new HemisphereLight(8822984, 726042, .38);
	const moon = new DirectionalLight(8822984, .55);
	moon.position.set(-8, 18, -6);
	const lamp = new PointLight(16749095, 0, 12, 2);
	scene.add(hemi, moon, lamp);
	const fog = new FogExp2(5135714, .05);
	scene.fog = fog;
	scene.background = new Color(5135714);
	const map = woodTexture();
	const wood = new MeshStandardMaterial({
		map: map ?? void 0,
		color: map ? 16777215 : 3811874,
		roughness: .86,
		metalness: .04
	});
	const wet = new MeshStandardMaterial({
		map: map ?? void 0,
		color: 13153956,
		roughness: .42,
		metalness: .12
	});
	const stone = new MeshStandardMaterial({
		color: 2371638,
		roughness: .9,
		metalness: .05
	});
	const iron = new MeshStandardMaterial({
		color: 2896952,
		roughness: .45,
		metalness: .72
	});
	const brass = new MeshStandardMaterial({
		color: 9067048,
		roughness: .35,
		metalness: .75
	});
	const ember = new MeshStandardMaterial({
		color: 16749095,
		emissive: 16738816,
		emissiveIntensity: .8,
		roughness: .4
	});
	const cloth = new MeshStandardMaterial({
		color: 1844264,
		roughness: 1,
		side: 2
	});
	const level = new Group();
	scene.add(level);
	const roomRoots = /* @__PURE__ */ new Map();
	const itemMeshes = /* @__PURE__ */ new Map();
	const blockerMeshes = /* @__PURE__ */ new Map();
	let whale = null;
	let waterMat = null;
	const viewScene = new Scene();
	const viewCam = new PerspectiveCamera(42, 1, .05, 4);
	const lantern = new Group();
	const cage = new Mesh(new CylinderGeometry(.07, .08, .16, 8), iron);
	const glass = new Mesh(new CylinderGeometry(.055, .06, .12, 8), new MeshStandardMaterial({
		color: 16769456,
		transparent: true,
		opacity: .28,
		roughness: .1
	}));
	const flame = new Mesh(new ConeGeometry(.02, .06, 6), ember);
	flame.position.y = .02;
	const handle = new Mesh(new TorusGeometry(.05, .008, 6, 10), brass);
	handle.position.y = .12;
	lantern.add(cage, glass, flame, handle);
	lantern.position.set(.34, -.34, -.58);
	viewScene.add(lantern);
	viewScene.add(new HemisphereLight(16749095, 1708040, .8));
	const goal = new Mesh(new SphereGeometry(.08, 8, 8), new MeshBasicMaterial({
		color: 16749095,
		transparent: true,
		opacity: .85
	}));
	scene.add(goal);
	const portrait = new Mesh(new PlaneGeometry(.8, 1.1), new MeshStandardMaterial({
		color: 4864820,
		roughness: .7
	}));
	portrait.position.set(1.15, 1.4, 28);
	portrait.rotation.y = Math.PI / 2;
	scene.add(portrait);
	function matFor(room) {
		if (room.kind === "street" || room.kind === "temple") return stone;
		if (room.kind === "deck" || room.kind === "grave") return wet;
		return wood;
	}
	function segInsideOther(px, pz, self, rooms) {
		return rooms.some((r) => r !== self && r.act === self.act && roomContains(r, px, pz));
	}
	function buildAct(next) {
		for (const child of [...level.children]) {
			level.remove(child);
			child.traverse((obj) => {
				const mesh = obj;
				if (mesh.geometry && mesh.userData.ownGeo) mesh.geometry.dispose();
			});
		}
		roomRoots.clear();
		itemMeshes.clear();
		blockerMeshes.clear();
		whale = null;
		const rooms = ROOMS.filter((r) => r.act === next);
		for (const room of rooms) {
			const parts = [];
			const y0 = room.floor;
			const y1 = room.floorTo ?? room.floor;
			const midY = (y0 + y1) / 2;
			const floor = new BoxGeometry(room.w, .28, room.d);
			if (room.floorTo !== void 0) {
				const rise = room.floorTo - room.floor;
				floor.rotateX(-Math.atan2(rise, room.d));
			}
			floor.translate(room.x + room.w / 2, midY - .14, room.z + room.d / 2);
			parts.push(floor);
			if (room.ceil) {
				const ceil = new BoxGeometry(room.w, .18, room.d);
				ceil.translate(room.x + room.w / 2, Math.min(y0, y1) + 2.7, room.z + room.d / 2);
				parts.push(ceil);
			}
			const height = room.ceil ? 2.6 : 1.05;
			const baseY = Math.min(y0, y1) + height / 2;
			const edges = [
				{
					len: room.w,
					horizontal: true,
					x: room.x,
					z: room.z,
					nx: 0,
					nz: -1
				},
				{
					len: room.w,
					horizontal: true,
					x: room.x,
					z: room.z + room.d,
					nx: 0,
					nz: 1
				},
				{
					len: room.d,
					horizontal: false,
					x: room.x,
					z: room.z,
					nx: -1,
					nz: 0
				},
				{
					len: room.d,
					horizontal: false,
					x: room.x + room.w,
					z: room.z,
					nx: 1,
					nz: 0
				}
			];
			for (const edge of edges) {
				const n = Math.max(1, Math.round(edge.len / 2));
				for (let i = 0; i < n; i++) {
					const t = (i + .5) / n;
					const cx = edge.horizontal ? edge.x + edge.len * t : edge.x;
					const cz = edge.horizontal ? edge.z : edge.z + edge.len * t;
					if (segInsideOther(cx + edge.nx * .4, cz + edge.nz * .4, room, rooms)) continue;
					const w = edge.horizontal ? edge.len / n : .18;
					const d = edge.horizontal ? .18 : edge.len / n;
					const wall = new BoxGeometry(w, height, d);
					wall.translate(cx, baseY, cz);
					parts.push(wall);
				}
			}
			if (room.landmark === "mast") {
				const mast = new CylinderGeometry(.18, .24, 8, 6);
				mast.translate(room.x + room.w * .62, 4, room.z + room.d * .45);
				parts.push(mast);
			}
			if (room.landmark === "columns") for (let i = 0; i < 3; i++) {
				const col = new CylinderGeometry(.22, .26, 2.6, 6);
				col.translate(room.x + 1.2 + i * (room.w - 2.4) / 2, Math.min(y0, y1) + 1.3, room.z + room.d * .5);
				parts.push(col);
			}
			const merged = parts.length === 1 ? parts[0] : mergeOrFirst(parts);
			const mesh = new Mesh(merged, matFor(room));
			mesh.userData.ownGeo = true;
			const root = new Group();
			root.add(mesh);
			if (!room.ceil && (room.kind === "deck" || room.kind === "grave")) {
				const sail = new Mesh(new PlaneGeometry(1.6, 2.2), cloth);
				sail.position.set(room.x + room.w * .62, 3.2, room.z + room.d * .45);
				root.add(sail);
			}
			root.visible = !room.openFlag || flags.has(room.openFlag);
			level.add(root);
			roomRoots.set(room.id, root);
		}
		for (const item of ITEMS.filter((it) => it.act === next)) {
			const mesh = itemMesh(item);
			const p = itemPoint(item);
			mesh.position.set(p.x, p.y, p.z);
			level.add(mesh);
			itemMeshes.set(item.id, mesh);
		}
		for (const b of BLOCKERS.filter((bl) => bl.act === next)) {
			const mesh = new Mesh(new BoxGeometry(b.w, 2.2, b.d), iron);
			mesh.position.set(b.x + b.w / 2, 1.1, b.z + b.d / 2);
			mesh.userData.ownGeo = true;
			level.add(mesh);
			blockerMeshes.set(b.id, mesh);
		}
		if (next === 4) {
			whale = new Mesh(new SphereGeometry(1, 10, 8), new MeshStandardMaterial({
				color: 1056806,
				roughness: .6
			}));
			whale.scale.set(6.5, 1.5, 2.2);
			whale.position.set(28, 2.2, 64);
			scene.add(whale);
		} else if (whale) {
			scene.remove(whale);
			whale = null;
		}
		const info = ACTS[next];
		fog.color.setHex(info.fogColor);
		fog.density = info.fog;
		scene.background.setHex(info.fogColor);
		renderer.setClearColor(info.fogColor, 1);
		if (!waterMat) {
			waterMat = new ShaderMaterial({
				transparent: true,
				uniforms: {
					uTime: { value: 0 },
					uA: { value: new Color("#0b141a") },
					uB: { value: new Color("#16343a") },
					uC: { value: new Color("#18c5d2") }
				},
				vertexShader: `uniform float uTime; varying float vH; void main(){ vec3 p=position; float w=sin(p.x*0.25+uTime*0.6)*0.08+cos(p.z*0.18+uTime*0.4)*0.1; p.y+=w; vH=w; gl_Position=projectionMatrix*modelViewMatrix*vec4(p,1.0); }`,
				fragmentShader: `precision highp float; uniform vec3 uA; uniform vec3 uB; uniform vec3 uC; uniform float uTime; varying float vH; void main(){ float g=smoothstep(0.97,1.0,sin(vH*40.0+uTime)); vec3 c=mix(uA,uB,0.5+vH*2.0); c=mix(c,uC,g*0.45); gl_FragColor=vec4(c,0.88); }`
			});
			const water = new Mesh(new PlaneGeometry(180, 180, 24, 24), waterMat);
			water.rotation.x = -Math.PI / 2;
			water.position.y = -1.25;
			scene.add(water);
		}
		const water = scene.children.find((c) => c.material === waterMat);
		if (water) water.position.y = next === 3 ? .22 : next === 4 ? -.4 : -1.25;
		portrait.visible = next === 1;
		applyQuality();
		refreshMeshes();
	}
	function mergeOrFirst(parts) {
		if (parts.length === 1) return parts[0];
		const merged = mergeGeometries(parts, false);
		if (!merged) return parts[0];
		for (const part of parts) part.dispose();
		return merged;
	}
	function itemMesh(item) {
		let mesh;
		if (item.kind === "oil") mesh = new Mesh(new CylinderGeometry(.1, .12, .28, 8), ember);
		else if (item.kind === "bell") mesh = new Mesh(new ConeGeometry(.12, .22, 8), brass);
		else if (item.kind === "crate") mesh = new Mesh(new BoxGeometry(.85, .7, .85), wood);
		else if (item.kind === "clock") mesh = new Mesh(new CylinderGeometry(.28, .28, .08, 12), brass);
		else if (item.kind === "mirror") mesh = new Mesh(new BoxGeometry(.7, 1.2, .06), iron);
		else if (item.kind === "exit" || item.kind === "altar") mesh = new Mesh(new TorusGeometry(.45, .05, 6, 12), ember);
		else mesh = new Mesh(new BoxGeometry(.34, .08, .42), new MeshStandardMaterial({
			color: 14140322,
			roughness: .8
		}));
		mesh.userData.ownGeo = true;
		return mesh;
	}
	function itemPoint(item) {
		const slot = item.kind === "crate" ? item.slots?.[crates[item.id] ?? 0] : void 0;
		const ix = slot ? slot[0] : item.x;
		const iz = slot ? slot[1] : item.z;
		const room = pickRoom(ix, iz, true);
		return {
			x: ix,
			y: (room ? floorOf(room, iz) : 0) + (item.kind === "bell" || item.kind === "mirror" ? 1.2 : .45),
			z: iz
		};
	}
	function openRooms(includeClosed = false) {
		return ROOMS.filter((r) => r.act === act && (includeClosed || !r.openFlag || flags.has(r.openFlag)));
	}
	function pickRoom(px, pz, includeClosed = false) {
		let best = null;
		let area = Infinity;
		for (const room of openRooms(includeClosed)) {
			if (!roomContains(room, px, pz)) continue;
			const a = room.w * room.d;
			if (a < area) {
				area = a;
				best = room;
			}
		}
		return best;
	}
	function floorOf(room, pz) {
		if (room.floorTo === void 0) return room.floor;
		const t = Math.min(1, Math.max(0, (pz - room.z) / room.d));
		return room.floor + (room.floorTo - room.floor) * t;
	}
	function blockerHit(px, pz) {
		for (const b of BLOCKERS) {
			if (b.act !== act || flags.has(b.until)) continue;
			if (px > b.x - R && px < b.x + b.w + R && pz > b.z - R && pz < b.z + b.d + R) return true;
		}
		for (const item of ITEMS) {
			if (item.act !== act || item.kind !== "crate" || !item.slots) continue;
			const p = itemPoint(item);
			const dx = px - p.x;
			const dz = pz - p.z;
			if (dx * dx + dz * dz < .5184) return true;
		}
		return false;
	}
	function solid(px, pz) {
		const samples = [
			[0, 0],
			[R, 0],
			[-.3, 0],
			[0, R],
			[0, -.3]
		];
		for (const [dx, dz] of samples) if (!pickRoom(px + dx, pz + dz)) return true;
		return blockerHit(px, pz);
	}
	function refreshMeshes() {
		for (const room of ROOMS) {
			const root = roomRoots.get(room.id);
			if (root) root.visible = room.act === act && (!room.openFlag || flags.has(room.openFlag));
		}
		for (const b of BLOCKERS) {
			const mesh = blockerMeshes.get(b.id);
			if (mesh) mesh.visible = b.act === act && !flags.has(b.until);
		}
		for (const item of ITEMS) {
			const mesh = itemMeshes.get(item.id);
			if (!mesh) continue;
			const room = ROOMS.find((r) => r.id === item.room);
			mesh.visible = !(item.once && item.flag && flags.has(item.flag) && (item.kind === "oil" || item.kind === "pickup" || item.kind === "hatch") || room?.openFlag && !flags.has(room.openFlag)) && item.act === act;
			if (item.kind === "crate" || item.kind === "mirror") {
				const p = itemPoint(item);
				mesh.position.set(p.x, p.y, p.z);
				if (item.kind === "mirror") mesh.rotation.y = (mirrors[item.id] ?? 0) * (Math.PI / 2);
			}
		}
	}
	function syncDerived() {
		const a = crates.a1_crate_a ?? 0;
		const b = crates.a1_crate_b ?? 0;
		if (a === 2 && b === 2) flags.add("a1_crates");
		else flags.delete("a1_crates");
		if ((crates.a2_crate ?? 0) === 2) flags.add("a2_clear");
		else flags.delete("a2_clear");
		if ([
			"a3_c1",
			"a3_c2",
			"a3_c3"
		].every((id) => {
			const item = ITEMS.find((it) => it.id === id);
			return item && (clocks[id] ?? 0) === item.answer;
		})) flags.add("a3_clocks");
		else flags.delete("a3_clocks");
		let prev = true;
		for (const id of [
			"a4_m1",
			"a4_m2",
			"a4_m3",
			"a4_m4"
		]) {
			const item = ITEMS.find((it) => it.id === id);
			const rot = mirrors[id] ?? 0;
			if (prev && item && rot === item.answer) flags.add(id);
			else flags.delete(id);
			prev = flags.has(id);
		}
		refreshMeshes();
	}
	function goalNow() {
		for (const g of GOALS[act]) if (!flags.has(g.flag)) return g;
		return GOALS[act][GOALS[act].length - 1];
	}
	function say(text) {
		subtitle = text;
		subT = 5.5;
	}
	function remember(item) {
		if (!item.title || !item.body) return;
		if (!logs.some((l) => l.id === item.id)) logs.unshift({
			id: item.id,
			title: item.title,
			body: item.body
		});
		reading = {
			title: item.title,
			body: item.body
		};
	}
	function playSeq(puzzle) {
		const notes = puzzle === "a1" ? A1_NOTES : A2_NOTES;
		const seq = puzzle === "a1" ? A1_SEQ : A2_SEQ;
		const names = puzzle === "a1" ? A1_NAMES : A2_NAMES;
		seq.forEach((n, i) => {
			window.setTimeout(() => audio.tone(notes[n], .42), i * 480);
		});
		if (settings.subtitles) say(seq.map((n) => names[n]).join(" · "));
	}
	function onBell(item) {
		if (item.puzzle === "a2c") {
			audio.tone(A2_NOTES[item.bell ?? 0], .5);
			if (item.flag) flags.add(item.flag);
			if (settings.subtitles && item.body) say(item.body);
			return;
		}
		const puzzle = item.puzzle === "a2" ? "a2" : "a1";
		const seq = puzzle === "a1" ? A1_SEQ : A2_SEQ;
		const notes = puzzle === "a1" ? A1_NOTES : A2_NOTES;
		const names = puzzle === "a1" ? A1_NAMES : A2_NAMES;
		const bell = item.bell ?? 0;
		audio.tone(notes[bell], .4);
		let progress = puzzle === "a1" ? a1p : a2p;
		if (seq[progress] === bell) progress += 1;
		else progress = seq[0] === bell ? 1 : 0;
		if (progress >= seq.length) {
			progress = 0;
			if (puzzle === "a1") {
				flags.add("a1_music");
				sanity = Math.min(100, sanity + BALANCE.musicReward);
				say("Ninni tamam. Vanessa'nın kapısı aralandı.");
				audio.chime();
			} else {
				flags.add("a2_bells");
				sanity = Math.min(100, sanity + BALANCE.musicReward);
				say("Çanlar durdu. Tek Fener'deki çivi gevşer.");
				audio.chime();
			}
		} else if (settings.subtitles) say(names[bell] ?? "");
		else if (progress === 0) audio.wrong();
		if (puzzle === "a1") a1p = progress;
		else a2p = progress;
		syncDerived();
	}
	function interact() {
		if (mode === "card") {
			mode = "playing";
			emit();
			return;
		}
		if (reading) {
			reading = null;
			emit();
			return;
		}
		if (mode !== "playing") return;
		const fx = -Math.sin(yaw);
		const fz = -Math.cos(yaw);
		let best = null;
		let bestD = 2.25;
		for (const item of ITEMS) {
			if (item.act !== act) continue;
			const room = ROOMS.find((r) => r.id === item.room);
			if (room?.openFlag && !flags.has(room.openFlag)) continue;
			if (item.once && item.flag && flags.has(item.flag) && (item.kind === "oil" || item.kind === "pickup" || item.kind === "hatch")) continue;
			const p = itemPoint(item);
			const dx = p.x - x;
			const dz = p.z - z;
			const dist = Math.hypot(dx, dz);
			if (dist > bestD) continue;
			if (dx / (dist || 1) * fx + dz / (dist || 1) * fz < .15 && dist > 1.1) continue;
			best = item;
			bestD = dist;
		}
		if (!best) return;
		if (best.need && !best.need.every((f) => flags.has(f))) {
			say(best.needMsg ?? "Henüz değil.");
			audio.wrong();
			emit();
			return;
		}
		if (best.id === "a1_reset") {
			crates.a1_crate_a = 0;
			crates.a1_crate_b = 0;
			syncDerived();
			say("Kasalar ilk yerlerine döndü.");
			emit();
			return;
		}
		if (best.kind === "bell") {
			onBell(best);
			emit();
			return;
		}
		if (best.kind === "listen" && best.puzzle) {
			playSeq(best.puzzle === "a2" ? "a2" : "a1");
			emit();
			return;
		}
		if (best.kind === "crate" && best.slots) {
			const next = ((crates[best.id] ?? 0) + 1) % best.slots.length;
			const slot = best.slots[next];
			const dx = slot[0] - x;
			const dz = slot[1] - z;
			if (dx * dx + dz * dz < .9) {
				say("Kasa seni ezer. Bir adım çekil.");
				emit();
				return;
			}
			crates[best.id] = next;
			syncDerived();
			audio.creak();
			say(flags.has(best.act === 1 ? "a1_crates" : "a2_clear") ? "Halka oturdu. Yol açıldı." : "Kasa kaydı.");
			emit();
			return;
		}
		if (best.kind === "clock" && best.options) {
			clocks[best.id] = ((clocks[best.id] ?? 0) + 1) % best.options.length;
			syncDerived();
			say(`${best.prompt}: ${best.options[clocks[best.id] ?? 0]}`);
			if (flags.has("a3_clocks")) say("Üç saat birden doğruldu. Deniz duvarı açıldı.");
			audio.tone(320, .15, "square", .04);
			emit();
			return;
		}
		if (best.kind === "mirror") {
			mirrors[best.id] = ((mirrors[best.id] ?? 0) + 1) % 4;
			syncDerived();
			const turns = mirrors[best.id] ?? 0;
			say(flags.has(best.id) ? "Işın bu aynada tutundu." : `Ayna ${turns} çeyrek döndü.`);
			audio.tone(480, .12, "triangle", .04);
			emit();
			return;
		}
		if (best.kind === "hatch") {
			flags.add("a1_hatch");
			say("Kapak kalktı. Ambar merdiveni açıldı.");
			audio.creak();
			syncDerived();
			emit();
			return;
		}
		if (best.kind === "altar") {
			flags.add("a4_ending");
			mode = "ending";
			say("Ninni susuyor.");
			audio.chime();
			persist();
			emit();
			return;
		}
		if (best.kind === "exit") {
			if (act < 4) {
				flags.add(best.flag ?? "");
				act = act + 1;
				const spawn = SPAWN[act];
				x = spawn.x;
				z = spawn.z;
				yaw = Math.PI;
				pitch = -.06;
				safeX = x;
				safeZ = z;
				buildAct(act);
				cardTitle = `Perde ${"I II III IV".split(" ")[act - 1]}`;
				cardLine = `${ACTS[act].name}. ${ACTS[act].line}`;
				mode = "card";
				persist();
				emit();
			}
			return;
		}
		if (best.flag) flags.add(best.flag);
		if (best.oil) oil = Math.min(100, oil + best.oil);
		if (typeof best.sanity === "number") sanity = Math.min(100, Math.max(0, sanity + best.sanity));
		if (best.body) remember(best);
		else if (best.title) say(best.title);
		if (best.kind === "oil") say("Yağ eklendi. Fitil düzeliyor.");
		audio.tone(best.kind === "oil" ? 180 : 240, .12, "triangle", .04);
		syncDerived();
		emit();
	}
	function held(code) {
		return (probe ?? keys).has(code);
	}
	function persist() {
		writeSave({
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
			logs
		});
	}
	function applySave(data) {
		act = data.act;
		x = data.x;
		z = data.z;
		yaw = data.yaw;
		pitch = data.pitch;
		sanity = data.sanity;
		tier = data.tier;
		oil = data.oil;
		lanternOn = data.lanternOn;
		flags.clear();
		data.flags.forEach((f) => flags.add(f));
		Object.keys(crates).forEach((k) => delete crates[k]);
		Object.assign(crates, data.crates);
		Object.keys(clocks).forEach((k) => delete clocks[k]);
		Object.assign(clocks, data.clocks);
		Object.keys(mirrors).forEach((k) => delete mirrors[k]);
		Object.assign(mirrors, data.mirrors);
		logs.splice(0, logs.length, ...data.logs);
		safeX = x;
		safeZ = z;
		buildAct(act);
		syncDerived();
	}
	function resetWorld() {
		flags.clear();
		for (const k of Object.keys(crates)) delete crates[k];
		for (const k of Object.keys(clocks)) delete clocks[k];
		for (const k of Object.keys(mirrors)) delete mirrors[k];
		logs.splice(0, logs.length);
		act = 1;
		sanity = 100;
		tier = "stable";
		oil = BALANCE.startOil;
		lanternOn = true;
		a1p = 0;
		a2p = 0;
		seaT = 0;
		x = SPAWN[1].x;
		z = SPAWN[1].z;
		yaw = Math.PI;
		pitch = -.08;
		safeX = x;
		safeZ = z;
		reading = null;
		clearSave();
		buildAct(1);
	}
	function nearestPrompt() {
		if (mode !== "playing" || reading) return null;
		const fx = -Math.sin(yaw);
		const fz = -Math.cos(yaw);
		let label = null;
		let best = 2.25;
		for (const item of ITEMS) {
			if (item.act !== act) continue;
			const room = ROOMS.find((r) => r.id === item.room);
			if (room?.openFlag && !flags.has(room.openFlag)) continue;
			const p = itemPoint(item);
			const dx = p.x - x;
			const dz = p.z - z;
			const dist = Math.hypot(dx, dz);
			if (dist > best) continue;
			if (dx / (dist || 1) * fx + dz / (dist || 1) * fz < .15 && dist > 1.1) continue;
			label = item.prompt;
			best = dist;
		}
		return label;
	}
	function emit() {
		const g = goalNow();
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
			webgl
		});
	}
	function applyQuality() {
		const cap = settings.quality === "low" ? 1 : settings.quality === "high" ? 1.75 : 1.35;
		const pr = Math.min(window.devicePixelRatio || 1, cap);
		renderer.setPixelRatio(pr);
		resize();
	}
	function resize() {
		const w = canvas.clientWidth || window.innerWidth;
		const h = canvas.clientHeight || window.innerHeight;
		if (w < 2 || h < 2) return;
		renderer.setSize(w, h, false);
		camera.aspect = w / h;
		camera.updateProjectionMatrix();
		viewCam.aspect = w / h;
		viewCam.updateProjectionMatrix();
	}
	const onResize = () => resize();
	window.addEventListener("resize", onResize);
	const onKeyDown = (e) => {
		if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return;
		keys.add(e.code);
		if (e.code === "KeyE") interact();
		if (e.code === "KeyF") toggleLantern();
		if (e.code === "Escape") {
			if (mode === "playing") pause();
			else if (mode === "paused") resume();
		}
	};
	const onKeyUp = (e) => keys.delete(e.code);
	const onBlur = () => keys.clear();
	const onMove = (e) => {
		if (document.pointerLockElement !== canvas) return;
		addLook(e.movementX, e.movementY);
	};
	window.addEventListener("keydown", onKeyDown);
	window.addEventListener("keyup", onKeyUp);
	window.addEventListener("blur", onBlur);
	window.addEventListener("mousemove", onMove);
	function toggleLantern() {
		if (mode !== "playing") return;
		if (!lanternOn && oil <= 0) {
			say("Yağ yok. Fitil tutuşmuyor.");
			return;
		}
		lanternOn = !lanternOn;
		audio.tone(lanternOn ? 220 : 90, .08, "triangle", .04);
		emit();
	}
	function pause() {
		if (mode !== "playing") return;
		mode = "paused";
		document.exitPointerLock?.();
		persist();
		emit();
	}
	function resume() {
		if (mode !== "paused" && mode !== "log" && mode !== "settings") return;
		mode = "playing";
		emit();
	}
	function addLook(dx, dy) {
		lookX += dx;
		lookY += dy;
	}
	function sim(dt) {
		menuT += dt;
		if (mode === "menu") {
			const s = SPAWN[1];
			x = s.x;
			z = s.z + 1.2;
			yaw = Math.PI + Math.sin(menuT * .25) * .18;
			pitch = -.12;
			lanternOn = true;
		} else if (mode === "playing" && !reading) {
			const sens = .00215 * settings.sens;
			yaw -= lookX * sens;
			pitch -= lookY * sens;
			const limit = Math.PI / 2 - .08;
			pitch = Math.max(-limit, Math.min(limit, pitch));
			lookX = 0;
			lookY = 0;
			let ix = stickX;
			let iz = stickY;
			if (held("KeyA") || held("ArrowLeft")) ix -= 1;
			if (held("KeyD") || held("ArrowRight")) ix += 1;
			if (held("KeyW") || held("ArrowUp")) iz += 1;
			if (held("KeyS") || held("ArrowDown")) iz -= 1;
			const len = Math.hypot(ix, iz);
			if (len > 1) {
				ix /= len;
				iz /= len;
			}
			const sprint = (sprintHeld || held("ShiftLeft") || held("ShiftRight")) && len > .2;
			const speed = sprint ? 3.35 : 2.05;
			const fx = -Math.sin(yaw);
			const fz = -Math.cos(yaw);
			const rx = Math.cos(yaw);
			const rz = -Math.sin(yaw);
			const mx = (fx * iz + rx * ix) * speed * dt;
			const mz = (fz * iz + rz * ix) * speed * dt;
			horizSpeed = Math.hypot(mx, mz) / Math.max(dt, 1e-4);
			if (!solid(x + mx, z)) x += mx;
			if (!solid(x, z + mz)) z += mz;
			if (!pickRoom(x, z)) {
				x = safeX;
				z = safeZ;
			}
			const room = pickRoom(x, z);
			if (room && room.name !== roomName) {
				roomName = room.name;
				say(room.name);
			}
			const indoors = !!room?.ceil;
			const dark = !lanternOn || oil <= 0;
			let delta = 0;
			if (room?.safe && lanternOn && oil > 0) delta += BALANCE.safeRegen;
			else if (dark && room && room.ambient < .25 && !room.safe) delta -= BALANCE.darkDrain;
			else if (dark) delta -= BALANCE.moonDrain;
			if (room && (room.kind === "deck" || room.kind === "grave" || room.id === "a4_gal") && horizSpeed < .35 && Math.abs(pitch) < .42) seaT += dt;
			else seaT = Math.max(0, seaT - dt * 2);
			if (seaT > BALANCE.seaDelay) {
				delta -= BALANCE.seaDrain;
				if (!warnedSea) {
					warnedSea = true;
					say("Su seni çağırıyor. Bakışını çek.");
				}
			}
			sanity = Math.max(0, Math.min(100, sanity + delta * dt));
			if (delta < 0) stableHold = 0;
			else if (tier === "stable") stableHold += dt;
			tier = updateTier(sanity, tier);
			if (lanternOn && oil > 0) {
				oil = Math.max(0, oil - (BALANCE.oilBurn + (sprint ? BALANCE.oilSprint : 0)) * dt);
				if (oil <= 0) {
					lanternOn = false;
					if (!warnedOil) {
						warnedOil = true;
						say("Yağ bitti. Karanlık ağırlaşıyor.");
					}
				}
			}
			if (sprint && indoors === false && !warnedWind && lanternOn) {
				warnedWind = true;
				say("Rüzgâr alevi yatırıyor. Koşunca yağ da gider.");
			}
			if (horizSpeed > .6) {
				stepAcc += dt;
				if (stepAcc > (sprint ? .34 : .5)) {
					stepAcc = 0;
					audio.step();
				}
			}
			saveAcc += dt;
			if (saveAcc > 8) {
				saveAcc = 0;
				safeX = x;
				safeZ = z;
				safeYaw = yaw;
				persist();
			}
			distortAcc -= dt;
			if (distortAcc <= 0) {
				distortAcc = 5 + Math.random() * 3;
				const camF = new Vector3(-Math.sin(yaw), 0, -Math.cos(yaw));
				const toP = new Vector3(1.15 - x, 0, 28 - z).normalize();
				if (!(camF.dot(toP) > .45) && tier !== "stable") {
					const mag = tier === "uneasy" ? .08 : tier === "disturbed" ? .28 : .45;
					portraitOff = Math.max(-mag, Math.min(mag, portraitOff + (Math.random() > .5 ? mag : -mag)));
				}
				if (tier === "stable" && stableHold > 10) portraitOff *= .5;
			}
		} else {
			lookX = 0;
			lookY = 0;
			horizSpeed = 0;
		}
		if (subT > 0) {
			subT -= dt;
			if (subT <= 0) subtitle = null;
		}
		const room = pickRoom(x, z);
		const eye = (room ? floorOf(room, z) : 0) + 1.58;
		const bob = settings.motion && horizSpeed > .4 ? Math.sin(menuT * 8) * .025 : 0;
		camera.position.set(x, eye + bob, z);
		camera.rotation.y = yaw;
		camera.rotation.x = pitch;
		const fov = 72 + (settings.motion && tier === "breaking" ? Math.sin(menuT) * 1.2 : 0);
		if (Math.abs(camera.fov - fov) > .05) {
			camera.fov = fov;
			camera.updateProjectionMatrix();
		}
		const fx = -Math.sin(yaw);
		const fz = -Math.cos(yaw);
		const rx = Math.cos(yaw);
		const rz = -Math.sin(yaw);
		const flick = lanternOn && oil > 0 ? .82 + Math.sin(menuT * (sprintHeld ? 28 : 13)) * (sprintHeld ? .12 : .04) : 0;
		lamp.intensity = lanternOn && oil > 0 ? (2.4 + 4.2 * (oil / 100)) * (.75 + flick) : 0;
		lamp.position.set(x + fx * .45 + rx * .22, eye - .25, z + fz * .45 + rz * .22);
		hemi.intensity = room?.ceil ? room.safe && lanternOn ? .28 : .07 : .4;
		moon.intensity = room?.ceil ? .04 : .62;
		flame.visible = lanternOn && oil > 0;
		flame.scale.y = .7 + flick;
		portrait.rotation.y = Math.PI / 2 + portraitOff;
		const g = goalNow();
		if (g && mode === "playing") {
			const gr = pickRoom(g.x, g.z, true);
			goal.position.set(g.x, (gr ? floorOf(gr, g.z) : 0) + 1.15, g.z);
			goal.visible = true;
		} else goal.visible = false;
		if (whale) {
			whale.position.x = 26 + Math.sin(menuT * .15) * 4;
			whale.position.z = 62 + Math.cos(menuT * .1) * 3;
		}
		if (waterMat) waterMat.uniforms.uTime.value = menuT;
		audio.setWind(room?.ceil ? .15 : .7);
		audio.setDrone(tier === "stable" ? 0 : tier === "uneasy" ? .3 : tier === "disturbed" ? .6 : 1);
		renderer.autoClear = false;
		renderer.clear();
		renderer.render(scene, camera);
		renderer.clearDepth();
		renderer.render(viewScene, viewCam);
	}
	let raf = 0;
	let hudAcc = 0;
	const loop = () => {
		raf = requestAnimationFrame(loop);
		clock.update();
		const dt = Math.min(clock.getDelta(), .05);
		sim(dt);
		hudAcc += dt;
		if (hudAcc > .2) {
			hudAcc = 0;
			emit();
		}
	};
	buildAct(1);
	resize();
	loop();
	emit();
	window.__controlsTest = {
		getYaw: () => yaw,
		getSpeed: () => horizSpeed,
		getPos: () => ({
			x,
			z
		}),
		setKeys: (codes) => {
			probe = codes.length ? new Set(codes) : null;
		}
	};
	return {
		dispose: () => {
			cancelAnimationFrame(raf);
			clock.disconnect();
			window.removeEventListener("resize", onResize);
			window.removeEventListener("keydown", onKeyDown);
			window.removeEventListener("keyup", onKeyUp);
			window.removeEventListener("blur", onBlur);
			window.removeEventListener("mousemove", onMove);
			audio.dispose();
			renderer.dispose();
			delete window.__controlsTest;
		},
		setStick: (sx, sy) => {
			stickX = sx;
			stickY = sy;
		},
		setSprint: (v) => {
			sprintHeld = v;
		},
		addLook,
		interact: () => {
			audio.unlock();
			interact();
		},
		toggleLantern: () => {
			audio.unlock();
			toggleLantern();
		},
		pause,
		resume,
		newGame: () => {
			audio.unlock();
			resetWorld();
			mode = "playing";
			say("Kıç güverte. Fener sende. Yağ az.");
			emit();
		},
		continueGame: () => {
			audio.unlock();
			const data = loadSave();
			if (!data) return;
			applySave(data);
			mode = "playing";
			emit();
		},
		back: () => {
			if (reading) {
				reading = null;
				emit();
				return;
			}
			if (mode === "playing") pause();
			else if (mode === "paused" || mode === "log" || mode === "card") {
				mode = mode === "paused" ? "menu" : "playing";
				if (mode === "menu") document.exitPointerLock?.();
				emit();
			} else if (mode === "settings") {
				mode = settingsFrom === "paused" ? "paused" : "menu";
				emit();
			}
		},
		openSettings: (from) => {
			settingsFrom = from;
			mode = "settings";
			emit();
		},
		closeSettings: () => {
			mode = settingsFrom === "paused" ? "paused" : "menu";
			emit();
		},
		setSetting: (patch) => {
			Object.assign(settings, patch);
			writeSettings(settings);
			audio.setVolume(settings.volume);
			applyQuality();
			emit();
		},
		closeReading: () => {
			reading = null;
			emit();
		},
		openLog: () => {
			mode = "log";
			emit();
		},
		closeLog: () => {
			mode = settingsFrom === "paused" || loadSave() ? "paused" : "menu";
			if (mode === "paused" || flags.size) mode = mode === "menu" && flags.size ? "paused" : mode;
			mode = "paused";
			if (!loadSave() && flags.size === 0) mode = "menu";
			emit();
		},
		quitToMenu: () => {
			persist();
			mode = "menu";
			document.exitPointerLock?.();
			emit();
		},
		dismissCard: () => {
			mode = "playing";
			emit();
		},
		rescue: () => {
			x = safeX;
			z = safeZ;
			yaw = safeYaw;
			say("Son güvenli noktaya döndün.");
			emit();
		},
		lock: () => {
			if (matchMedia("(pointer: coarse)").matches) return;
			canvas.requestPointerLock?.();
		}
	};
}
//#endregion
export { createGame };
