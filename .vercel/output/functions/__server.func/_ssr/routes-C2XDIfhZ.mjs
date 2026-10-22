import { i as __toESM } from "../_runtime.mjs";
import { K as require_react, b as require_jsx_runtime } from "../_libs/@tanstack/react-router+[...].mjs";
import { a as Hand, i as Pause, o as Flame, r as Settings, s as BookOpen, t as Wind } from "../_libs/lucide-react.mjs";
//#region node_modules/.nitro/vite/services/ssr/assets/routes-C2XDIfhZ.js
var import_react = /* @__PURE__ */ __toESM(require_react());
var import_jsx_runtime = require_jsx_runtime();
var act1_default = "/assets/act1-BZIWAqI0.jpg";
var initial = {
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
	volume: .75,
	subtitles: true,
	motion: true,
	haptics: false,
	webgl: true
};
function HorrorGame() {
	const canvasRef = (0, import_react.useRef)(null);
	const api = (0, import_react.useRef)(null);
	const [hud, setHud] = (0, import_react.useState)(initial);
	const [knob, setKnob] = (0, import_react.useState)({
		x: 0,
		y: 0
	});
	const stick = (0, import_react.useRef)(null);
	const look = (0, import_react.useRef)(null);
	(0, import_react.useEffect)(() => {
		if (!canvasRef.current) return;
		let dead = false;
		let local = null;
		import("./engine-D4Y7gtKL.mjs").then(({ createGame }) => {
			if (dead || !canvasRef.current) return;
			try {
				local = createGame(canvasRef.current, setHud);
				api.current = local;
			} catch (err) {
				setHud((h) => ({
					...h,
					webgl: false,
					subtitle: err instanceof Error ? err.message : "3D başlatılamadı"
				}));
			}
		});
		const onPop = () => {
			api.current?.back();
			history.pushState({ undertow: 1 }, "");
		};
		history.pushState({ undertow: 1 }, "");
		window.addEventListener("popstate", onPop);
		return () => {
			dead = true;
			window.removeEventListener("popstate", onPop);
			local?.dispose();
			api.current = null;
		};
	}, []);
	const playing = hud.mode === "playing";
	return /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("main", {
		className: "fixed inset-0 overflow-hidden bg-abyss font-sans text-foam",
		children: [
			/* @__PURE__ */ (0, import_jsx_runtime.jsx)("img", {
				src: act1_default,
				alt: "",
				className: "absolute inset-0 h-full w-full object-cover opacity-40"
			}),
			/* @__PURE__ */ (0, import_jsx_runtime.jsx)("canvas", {
				ref: canvasRef,
				className: "absolute inset-0 h-full w-full touch-none",
				"aria-label": "Undertow oyun sahnesi"
			}),
			/* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", { className: "grain pointer-events-none absolute inset-0" }),
			playing && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", {
				className: "absolute inset-0 z-10 touch-none",
				onPointerDown: (e) => {
					if (e.pointerType === "mouse" && e.button === 0) api.current?.lock();
					look.current = {
						id: e.pointerId,
						x: e.clientX,
						y: e.clientY
					};
					e.currentTarget.setPointerCapture(e.pointerId);
				},
				onPointerMove: (e) => {
					if (!look.current || look.current.id !== e.pointerId) return;
					const dx = e.clientX - look.current.x;
					const dy = e.clientY - look.current.y;
					look.current = {
						id: e.pointerId,
						x: e.clientX,
						y: e.clientY
					};
					if (document.pointerLockElement) return;
					api.current?.addLook(dx, dy);
				},
				onPointerUp: (e) => {
					if (look.current?.id === e.pointerId) look.current = null;
				}
			}),
			hud.mode === "menu" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("section", {
				className: "safe-pad absolute inset-0 z-30 flex flex-col justify-between bg-linear-to-r from-ink/90 via-ink/55 to-transparent p-5 md:max-w-xl",
				children: [/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", { children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "text-xs tracking-[0.28em] text-moon uppercase",
						children: "Echoes of Vanessa"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h1", {
						className: "hero-title mt-2 text-foam",
						children: "Undertow"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-3 max-w-sm text-sm leading-relaxed text-fog",
						children: "Dört perde. Fener, sis, Vanessa’nın ninnisi. Zıplama korkusu yok; karanlık ve yanlış hatıra var."
					})
				] }), /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
					className: "flex flex-col gap-2",
					children: [
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "min-h-12 rounded-full bg-lantern px-5 font-medium text-ink",
							onClick: () => api.current?.newGame(),
							children: "Yeni oyun"
						}),
						hud.hasSave && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "min-h-12 rounded-full border border-fog/50 bg-abyss/80 px-5",
							onClick: () => api.current?.continueGame(),
							children: "Devam et"
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
							className: "flex gap-2",
							children: [/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("button", {
								type: "button",
								className: "min-h-12 flex-1 rounded-full border border-fog/40 px-4",
								onClick: () => api.current?.openSettings("menu"),
								children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)(Settings, {
									className: "mr-2 inline size-4",
									"aria-hidden": true
								}), "Ayarlar"]
							}), /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("button", {
								type: "button",
								className: "min-h-12 flex-1 rounded-full border border-fog/40 px-4",
								onClick: () => api.current?.openLog(),
								children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)(BookOpen, {
									className: "mr-2 inline size-4",
									"aria-hidden": true
								}), "Günlük"]
							})]
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
							className: "text-xs leading-relaxed text-fog",
							children: "Sol başparmak yürü. Sağda sürükle, bak. Fener yağ yakar. Denize uzun bakma."
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
							className: "text-[0.7rem] tracking-wide text-moon",
							children: "© Lenstedreal · All Rights Reserved."
						})
					]
				})]
			}),
			playing && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
				className: "pointer-events-none absolute inset-x-0 top-0 z-20 p-4 pt-[max(0.75rem,env(safe-area-inset-top))]",
				children: [/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("p", {
					className: "text-[0.65rem] tracking-[0.22em] text-moon uppercase",
					children: [
						"Perde ",
						hud.act,
						" · ",
						hud.actName
					]
				}), /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
					className: "mt-1 max-w-md text-sm text-foam",
					children: hud.objective
				})]
			}),
			hud.subtitle && playing && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
				className: "pointer-events-none absolute inset-x-6 bottom-36 z-20 text-center text-sm text-foam md:bottom-28",
				children: hud.subtitle
			}),
			hud.prompt && playing && !hud.reading && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
				className: "pointer-events-none absolute inset-x-0 bottom-28 z-20 text-center text-sm text-lantern md:bottom-20",
				children: hud.prompt
			}),
			playing && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)(import_jsx_runtime.Fragment, { children: [
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", {
					className: "absolute bottom-4 left-4 z-30 size-28 touch-none rounded-full border border-moon/40 bg-ink/45 safe-pad",
					onPointerDown: (e) => {
						e.stopPropagation();
						stick.current = {
							id: e.pointerId,
							x: e.clientX,
							y: e.clientY
						};
						e.currentTarget.setPointerCapture(e.pointerId);
					},
					onPointerMove: (e) => {
						if (!stick.current || stick.current.id !== e.pointerId) return;
						const dx = e.clientX - stick.current.x;
						const dy = e.clientY - stick.current.y;
						const mag = Math.hypot(dx, dy) || 1;
						const cap = 42;
						const k = Math.min(cap, mag) / mag;
						setKnob({
							x: dx * k,
							y: dy * k
						});
						api.current?.setStick(Math.max(-1, Math.min(1, dx / cap)), Math.max(-1, Math.min(1, -dy / cap)));
					},
					onPointerUp: (e) => {
						if (stick.current?.id !== e.pointerId) return;
						stick.current = null;
						setKnob({
							x: 0,
							y: 0
						});
						api.current?.setStick(0, 0);
					},
					"aria-label": "Hareket",
					children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)("span", {
						className: "absolute top-1/2 left-1/2 size-12 -translate-x-1/2 -translate-y-1/2 rounded-full bg-moon/80",
						style: {
							marginLeft: knob.x,
							marginTop: knob.y
						}
					})
				}),
				/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
					className: "absolute right-3 bottom-4 z-30 flex items-end gap-2",
					children: [
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "grid size-14 place-items-center rounded-full border border-fog/40 bg-ink/70",
							"aria-label": "Koş",
							onPointerDown: (e) => {
								e.stopPropagation();
								api.current?.setSprint(true);
							},
							onPointerUp: () => api.current?.setSprint(false),
							onPointerLeave: () => api.current?.setSprint(false),
							children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)(Wind, { className: "size-5" })
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "grid size-16 place-items-center rounded-full bg-lantern text-ink",
							onClick: () => api.current?.interact(),
							"aria-label": "Etkileşim",
							children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)(Hand, { className: "size-6" })
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: `grid size-14 place-items-center rounded-full border ${hud.lanternOn ? "border-lantern bg-lantern/20 text-lantern" : "border-fog/40 bg-ink/70"}`,
							onClick: () => api.current?.toggleLantern(),
							"aria-label": "Fener",
							children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)(Flame, { className: "size-5" })
						}),
						/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "grid size-12 place-items-center rounded-full border border-fog/40 bg-ink/70",
							onClick: () => api.current?.pause(),
							"aria-label": "Duraklat",
							children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)(Pause, { className: "size-4" })
						})
					]
				}),
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", {
					className: "absolute top-4 right-4 z-20 h-16 w-2 overflow-hidden rounded-full bg-ink/70",
					"aria-label": "Yağ",
					children: /* @__PURE__ */ (0, import_jsx_runtime.jsx)("span", {
						className: "absolute bottom-0 w-full bg-lantern",
						style: { height: `${hud.oil}%` }
					})
				})
			] }),
			hud.reading && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("section", {
				className: "absolute inset-x-0 bottom-0 z-40 max-h-[55%] overflow-auto rounded-t-3xl bg-ink/95 p-5 safe-pad",
				children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h2", {
						className: "font-display text-2xl text-lantern",
						children: hud.reading.title
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-3 text-sm leading-relaxed text-foam",
						children: hud.reading.body
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "mt-4 min-h-12 rounded-full bg-lantern px-5 text-ink",
						onClick: () => api.current?.closeReading(),
						children: "Kapat"
					})
				]
			}),
			hud.mode === "paused" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)(Panel, {
				title: "Duraklatıldı",
				children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-main",
						onClick: () => api.current?.resume(),
						children: "Devam"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-ghost",
						onClick: () => api.current?.rescue(),
						children: "Son güvenli nokta"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-ghost",
						onClick: () => api.current?.openLog(),
						children: "Günlük"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-ghost",
						onClick: () => api.current?.openSettings("paused"),
						children: "Ayarlar"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-ghost",
						onClick: () => api.current?.quitToMenu(),
						children: "Ana menü"
					})
				]
			}),
			hud.mode === "settings" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)(Panel, {
				title: "Ayarlar",
				children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("label", {
						className: "block text-sm text-fog",
						children: ["Bakış hassasiyeti", /* @__PURE__ */ (0, import_jsx_runtime.jsx)("input", {
							className: "mt-1 w-full",
							type: "range",
							min: .45,
							max: 2,
							step: .05,
							value: hud.sens,
							onChange: (e) => api.current?.setSetting({ sens: Number(e.target.value) })
						})]
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("label", {
						className: "mt-3 block text-sm text-fog",
						children: ["Ses", /* @__PURE__ */ (0, import_jsx_runtime.jsx)("input", {
							className: "mt-1 w-full",
							type: "range",
							min: 0,
							max: 1,
							step: .05,
							value: hud.volume,
							onChange: (e) => api.current?.setSetting({ volume: Number(e.target.value) })
						})]
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", {
						className: "mt-3 flex flex-wrap gap-2",
						children: [
							"low",
							"medium",
							"high"
						].map((q) => /* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: `btn-ghost ${hud.quality === q ? "border-lantern text-lantern" : ""}`,
							onClick: () => api.current?.setSetting({ quality: q }),
							children: q === "low" ? "Düşük" : q === "medium" ? "Orta" : "Yüksek"
						}, q))
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("label", {
						className: "mt-3 flex items-center gap-2 text-sm",
						children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)("input", {
							type: "checkbox",
							checked: hud.subtitles,
							onChange: (e) => api.current?.setSetting({ subtitles: e.target.checked })
						}), "Altyazı"]
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("label", {
						className: "mt-2 flex items-center gap-2 text-sm",
						children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)("input", {
							type: "checkbox",
							checked: !hud.motion,
							onChange: (e) => api.current?.setSetting({ motion: !e.target.checked })
						}), "Hareket azalt"]
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-main mt-4",
						onClick: () => api.current?.closeSettings(),
						children: "Kapat"
					})
				]
			}),
			hud.mode === "log" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)(Panel, {
				title: "Günlük",
				children: [/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
					className: "max-h-64 space-y-3 overflow-auto",
					children: [hud.logs.length === 0 && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "text-sm text-fog",
						children: "Henüz yazı yok."
					}), hud.logs.map((l) => /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("article", { children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h3", {
						className: "text-lantern",
						children: l.title
					}), /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "text-sm leading-relaxed text-fog",
						children: l.body
					})] }, l.id))]
				}), /* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
					type: "button",
					className: "btn-main mt-4",
					onClick: () => api.current?.closeLog(),
					children: "Kapat"
				})]
			}),
			hud.mode === "card" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("section", {
				className: "absolute inset-0 z-40 flex flex-col items-center justify-center bg-ink/80 p-6 text-center",
				children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "text-xs tracking-[0.28em] text-moon uppercase",
						children: hud.cardTitle
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h2", {
						className: "hero-title mt-3",
						children: hud.actName
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-4 max-w-md text-sm leading-relaxed text-fog",
						children: hud.cardLine
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
						type: "button",
						className: "btn-main mt-6",
						onClick: () => api.current?.dismissCard(),
						children: "Devam"
					})
				]
			}),
			hud.mode === "ending" && /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("section", {
				className: "absolute inset-0 z-40 flex flex-col justify-end bg-ink/88 p-6 safe-pad",
				children: [
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "text-xs tracking-[0.28em] text-moon uppercase",
						children: "Son"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h2", {
						className: "hero-title mt-2",
						children: "Vanessa"
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-4 max-w-xl text-sm leading-relaxed text-foam",
						children: "Ninni bitti. Kolye sunağa bırakıldı. Kasaba saati 11:40’ta donmuştu; gemi 02:17’de sis aldı, vardiyası 06:05’te kapanmıştı. Vanessa güvertede kaldı ki diğerleri filikaya binsin. Uzaktaki gölge geçti, vurmadı. Fener bir kez daha düz yandı."
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-3 text-sm text-fog",
						children: "Mezarlık susuyor. Su seni bırakıyor."
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
						className: "mt-5 flex gap-2",
						children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "btn-main",
							onClick: () => api.current?.newGame(),
							children: "Baştan"
						}), /* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
							type: "button",
							className: "btn-ghost",
							onClick: () => api.current?.quitToMenu(),
							children: "Menü"
						})]
					}),
					/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
						className: "mt-4 text-[0.7rem] text-moon",
						children: "© Lenstedreal · All Rights Reserved."
					})
				]
			}),
			!hud.webgl && /* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
				className: "absolute bottom-4 left-4 z-50 text-sm text-lantern",
				children: "Bu cihazda 3D görüntü açılamadı."
			})
		]
	});
}
function Panel({ title, children }) {
	return /* @__PURE__ */ (0, import_jsx_runtime.jsx)("section", {
		className: "absolute inset-0 z-40 flex items-end justify-center bg-ink/75 p-4 safe-pad md:items-center",
		children: /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", {
			className: "w-full max-w-md rounded-3xl border border-fog/30 bg-abyss/95 p-5",
			children: [/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h2", {
				className: "font-display text-3xl text-foam",
				children: title
			}), /* @__PURE__ */ (0, import_jsx_runtime.jsx)("div", {
				className: "mt-4 flex flex-col gap-2",
				children
			})]
		})
	});
}
function Home() {
	return /* @__PURE__ */ (0, import_jsx_runtime.jsx)(HorrorGame, {});
}
//#endregion
export { Home as component };
