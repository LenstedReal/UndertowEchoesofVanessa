using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// The three endings, each a short staged scene with subtitles, then the end credits and the menu.
    /// Answer: two children walking along a dawn shoreline. Surface: Adrian wakes in his boat, the box is
    /// silent, his mother's old message plays. Undertow: another shore, another tent, and his voice in the water.
    /// </summary>
    public sealed class EndingsDirector : MonoBehaviour
    {
        GameState _s;
        Transform _cam;
        Atmos _atmos;
        Transform _kids;

        public static void Create(GameState s)
        {
            var go = new GameObject("[Ending]");
            var d = go.AddComponent<EndingsDirector>();
            d._s = s;
            d.Build();
            d.StartCoroutine(d.Play());
        }

        EndingId Ending => _s.Ending == EndingId.None ? Endings.Resolve(_s) : _s.Ending;

        void Build()
        {
            var e = Ending;
            var camGo = new GameObject("EndingCamera");
            camGo.transform.SetParent(transform, false);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 40f;
            var hd = camGo.AddComponent<HDAdditionalCameraData>();
            hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            Game.Audio.BindListener(camGo.AddComponent<AudioListener>());
            _cam = camGo.transform;
            bool dawn = e != EndingId.Undertow;
            _atmos = Atmos.Create(transform, new AtmosPreset
            {
                FogDistance = dawn ? 45f : 16f,
                FogTint = dawn ? new Color(0.62f, 0.6f, 0.58f) : new Color(0.24f, 0.3f, 0.33f),
                FogAlbedo = dawn ? new Color(0.9f, 0.84f, 0.78f) : new Color(0.55f, 0.62f, 0.66f),
                SkyTop = dawn ? new Color(0.25f, 0.34f, 0.45f) : new Color(0.02f, 0.03f, 0.05f),
                SkyMid = dawn ? new Color(0.85f, 0.66f, 0.52f) : new Color(0.06f, 0.08f, 0.1f),
                SkyBottom = dawn ? new Color(0.95f, 0.8f, 0.66f) : new Color(0.09f, 0.1f, 0.12f),
                SkyExposure = dawn ? 6.5f : 1.4f,
                Exposure = dawn ? 7.5f : 2.0f,
                Saturation = dawn ? -12f : -25f,
                Contrast = 10f,
                Bloom = dawn ? 0.4f : 0.25f,
                Vignette = 0.35f,
            });
            var sun = LightKit.Directional(dawn ? "Sun" : "Moon", transform, dawn ? Quaternion.Euler(8f, 70f, 0f) : Quaternion.Euler(28f, 200f, 0f), dawn ? new Color(1f, 0.78f, 0.58f) : LightKit.Moon, dawn ? 600f : 2.2f, true);
            var sea = WaterSurface.CreateSea(transform, new Vector3(0, 0, 40f), 400f, -0.2f, 70);
            sea.Swell = dawn ? 0.08f : 0.18f;
            // the shore
            var sand = new MeshBuilder { TexScale = 3f };
            sand.Grid(new Vector3(-60f, 0, -40f), new Vector2(120f, 40f), 40, 16, (x, z) => 0.35f - z * 0.022f + Mathf.Sin(x * 0.3f) * 0.03f, 3f);
            Kit.Object("shore", transform, Vector3.zero, Quaternion.identity, sand, "sand");
            switch (e)
            {
                case EndingId.Answer:
                {
                    // two small figures, hand in hand, walking away along the waterline
                    _kids = new GameObject("Children").transform;
                    _kids.SetParent(transform, false);
                    foreach (float x in new[] { -0.28f, 0.28f })
                    {
                        var kid = new PropMesh();
                        float h = x < 0 ? 1.25f : 1.05f;
                        kid["silhouette"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.09f, 0f), new Vector2(0.14f, h * 0.45f), new Vector2(0.1f, h * 0.75f), new Vector2(0.06f, h * 0.8f), new Vector2(0.09f, h * 0.9f), new Vector2(0f, h) }, 10);
                        kid.Spawn("kid", _kids, new Vector3(x, 0, 0), Quaternion.identity);
                    }
                    _kids.position = new Vector3(-4f, -0.15f, -17.5f);
                    _kids.rotation = Quaternion.Euler(0, 84, 0);
                    break;
                }
                case EndingId.Surface:
                {
                    // the boat at dawn, the music box on the seat, silent now
                    var boat = Props.Rowboat().Spawn("boat", transform, new Vector3(0, -0.2f, 6f), Quaternion.Euler(0, 20, 0));
                    Props.MusicBox(10f, true).Spawn("box", boat.transform, new Vector3(0.05f, 0.39f, -0.5f), Quaternion.Euler(0, 170, 0));
                    break;
                }
                default:
                {
                    // another shore, another tent with a lamp inside, a boy asleep
                    var tent = new PropMesh();
                    var c = tent["cloth_blue"];
                    c.Quad(new Vector3(-1.1f, 0, -1.2f), new Vector3(-1.1f, 0, 1.2f), new Vector3(0, 1.3f, 1.2f), new Vector3(0, 1.3f, -1.2f));
                    c.Quad(new Vector3(1.1f, 0, 1.2f), new Vector3(1.1f, 0, -1.2f), new Vector3(0, 1.3f, -1.2f), new Vector3(0, 1.3f, 1.2f));
                    c.Quad(new Vector3(-1.1f, 0, 1.2f), new Vector3(-1.1f, 0, -1.2f), new Vector3(0, 1.3f, -1.2f), new Vector3(0, 1.3f, 1.2f));
                    c.Quad(new Vector3(1.1f, 0, -1.2f), new Vector3(1.1f, 0, 1.2f), new Vector3(0, 1.3f, 1.2f), new Vector3(0, 1.3f, -1.2f));
                    tent.Spawn("tent", transform, new Vector3(3f, -0.05f, -22f), Quaternion.Euler(0, -25, 0));
                    LightKit.Point("tent_lamp", transform, new Vector3(3f, 0.45f, -22f), LightKit.Lantern, 160f, 5f, false, false, 1.2f, 0.1f);
                    break;
                }
            }
        }

        IEnumerator Play()
        {
            var e = Ending;
            string key = "ending." + Endings.Key(e);
            Game.UI.InGame = false;
            Game.UI.Cinematic = true;
            Game.UI.RefreshMode();
            Cursor.visible = false;
            Game.Audio.SetAmbience(e == EndingId.Undertow ? "amb_menu" : "amb_shore", 3f);
            Game.Achievements.MarkEnding(e);
            Game.Achievements.Unlock("ending_" + Endings.Key(e));
            yield return Game.UI.Fade(0f, 3f);
            switch (e)
            {
                case EndingId.Answer: Game.Audio.PlayMusic("mus_ending_answer", 4f, 0.8f, false); break;
                case EndingId.Surface: Game.Audio.PlayMusic("mus_ending_surface", 4f, 0.7f, false); break;
                default: Game.Audio.PlayMusic("mus_ending_undertow", 4f, 0.8f, false); break;
            }
            // five lines, one camera move
            for (int i = 1; i <= 5; i++)
            {
                string line = key + "." + i;
                if (!Game.Loc.Has(line)) continue;
                var who = e == EndingId.Undertow && i >= 4 ? VoiceKind.Adrian : (e == EndingId.Surface && i >= 3 && i <= 4 ? VoiceKind.Ruth : VoiceKind.Whisper);
                if (e == EndingId.Surface && i == 3) Game.Audio.Play2D("sfx_answering_machine", Bus.Effects, 0.9f);
                string clip = "vo_" + line.Replace(".", "_");
                if (Game.Audio.Clip(clip) != null) Game.Audio.Play2D(clip, Bus.Voice, 1f);
                Game.UI.Subtitles.Say(who, Game.Loc[line], 5.5f);
                yield return new WaitForSeconds(6.2f);
            }
            // the title of the ending, quietly, then out
            var title = UIKit.Label(Game.UI.OverlayLayer, "EndingTitle", Game.Loc[key + ".title"], 64, FontRole.Title, TextAnchor.MiddleCenter, UIKit.Ink);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 100));
            var group = title.gameObject.AddComponent<CanvasGroup>();
            for (float t = 0; t < 6f; t += Time.deltaTime)
            {
                group.alpha = t < 1.5f ? t / 1.5f : (t > 4.5f ? (6f - t) / 1.5f : 1f);
                yield return null;
            }
            Destroy(title.gameObject);
            yield return Game.UI.Fade(1f, 2.5f);
            Game.UI.Cinematic = false;
            bool done = false;
            Game.UI.Push(new CreditsScreen(true, () => done = true));
            Game.UI.SetFade(0f);
            while (!done) yield return null;
            Game.Audio.StopMusic(2f);
            Game.Flow.ToMenu();
        }

        void Update()
        {
            float t = Time.time;
            var e = Ending;
            if (e == EndingId.Answer && _kids != null)
            {
                _kids.position += _kids.forward * 0.42f * Time.deltaTime;
                foreach (Transform k in _kids) k.localPosition = new Vector3(k.localPosition.x, Mathf.Abs(Mathf.Sin(t * 5f + k.localPosition.x * 3f)) * 0.04f, 0);
                _cam.position = new Vector3(-6f + t * 0.05f, 1.2f, -24f);
                _cam.LookAt(_kids.position + Vector3.up * 0.6f);
            }
            else if (e == EndingId.Surface)
            {
                _cam.position = new Vector3(0.4f + Mathf.Sin(t * 0.3f) * 0.05f, 1.1f + Mathf.Sin(t * 0.5f) * 0.03f, 4.6f);
                _cam.LookAt(new Vector3(0.1f, 0.2f, 5.6f));
            }
            else
            {
                _cam.position = new Vector3(-4f, 0.9f + Mathf.Sin(t * 0.4f) * 0.08f, -12f - t * 0.03f);
                _cam.LookAt(new Vector3(3f, 0.4f, -22f));
            }
        }
    }
}
