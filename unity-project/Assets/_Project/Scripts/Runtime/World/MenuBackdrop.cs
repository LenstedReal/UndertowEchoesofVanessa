using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Behind the main menu: the key art if the project has it, otherwise a small boat on a black sea in the fog,
    /// a lantern on its pole and the music box glowing on the seat, under a slowly drifting camera.
    /// </summary>
    public sealed class MenuBackdrop : MonoBehaviour
    {
        Transform _cam;
        Transform _boat;
        float _t;
        RawImage _art;
        float _isabelT;

        public static void Create()
        {
            var go = new GameObject("[Menu]");
            var mb = go.AddComponent<MenuBackdrop>();
            mb.Build();
            Game.Audio.SetAmbience("amb_menu", 2f);
            if (BootSequence.Done)
            {
                Game.UI.Replace(new MainMenu());
                if (!Game.Audio.MusicPlaying) Game.Audio.PlayMusic("mus_menu", 3f, 0.8f);
            }
        }

        void Build()
        {
            // camera and listener (the scene is a shell, so the menu brings its own)
            var camGo = new GameObject("MenuCamera");
            camGo.transform.SetParent(transform, false);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 400f;
            cam.fieldOfView = 42f;
            var hd = camGo.AddComponent<HDAdditionalCameraData>();
            hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            hd.dithering = true;
            var listener = camGo.AddComponent<AudioListener>();
            Game.Audio.BindListener(listener);
            _cam = camGo.transform;

            var atmos = Atmos.Create(transform, new AtmosPreset
            {
                FogDistance = 14f,
                FogTint = new Color(0.28f, 0.34f, 0.37f),
                FogAlbedo = new Color(0.6f, 0.68f, 0.72f),
                FogHeight = 8f,
                SkyTop = new Color(0.02f, 0.03f, 0.05f),
                SkyMid = new Color(0.06f, 0.08f, 0.1f),
                SkyBottom = new Color(0.1f, 0.12f, 0.13f),
                SkyExposure = 1.5f,
                Exposure = 2.0f,
                Saturation = -22f,
                Contrast = 14f,
                Bloom = 0.3f,
                Vignette = 0.4f,
            });
            LightKit.Directional("Moon", transform, Quaternion.Euler(24f, 195f, 0f), LightKit.Moon, 2.4f, true);
            var sea = WaterSurface.CreateSea(transform, Vector3.zero, 240f, -0.45f, 70);
            sea.Swell = 0.18f;

            var boat = new GameObject("Boat").transform;
            boat.SetParent(transform, false);
            _boat = boat;
            Props.Rowboat().Spawn("hull", boat, Vector3.zero, Quaternion.identity);
            var pole = new PropMesh();
            pole["wood_dark"].Cylinder(new Vector3(0, 0.3f, 1.4f), Quaternion.identity, 1.8f, 0.03f, 0.025f, 8);
            pole["wood_dark"].Box(new Vector3(0, 2.05f, 1.55f), new Vector3(0.04f, 0.04f, 0.35f), Quaternion.identity);
            pole.Spawn("pole", boat, Vector3.zero, Quaternion.identity);
            var lantern = Props.HangingLantern(true).Spawn("lantern", boat, new Vector3(0, 2.0f, 1.7f), Quaternion.identity);
            var l = LightKit.Point("BoatLantern", lantern.transform, new Vector3(0, -0.25f, 0), LightKit.Lantern, 420f, 9f, true, true, 1.4f, 0.04f);
            var fl = lantern.AddComponent<Flicker>();
            fl.Target = l;
            fl.Base = 420f;
            fl.Wind = 0.5f;
            Props.MusicBox(105f, true).Spawn("musicbox", boat, new Vector3(0.05f, 0.39f, -0.5f), Quaternion.Euler(0, 170, 0));
            LightKit.Point("BoxGlow", boat, new Vector3(0.05f, 0.6f, -0.5f), LightKit.Sea, 18f, 1.4f, false, false, 0.3f, 0.05f);
            boat.position = new Vector3(0f, -0.45f, 0f);

            // drifting debris and a far wreck in the fog for scale
            var wreck = new PropMesh();
            wreck["hull_dark"].Box(new Vector3(0, 2f, 0), new Vector3(3f, 6f, 14f), Quaternion.Euler(0, 0, 18));
            wreck["wood_dark"].Cylinder(new Vector3(0, 3f, 2f), Quaternion.Euler(0, 0, 25), 14f, 0.25f, 0.15f, 8);
            wreck.Spawn("wreck", transform, new Vector3(-26f, -2f, 38f), Quaternion.Euler(0, 30, 0));

            // key art, when the project has it (a real image, not a placeholder)
            var art = Resources.Load<Texture2D>("MainMenuKeyArt");
            if (art != null && art.width > 256)
            {
                var canvasGo = new GameObject("KeyArt");
                canvasGo.transform.SetParent(transform, false);
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = -10;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1f;
                var rt = UIKit.Rect(canvasGo.transform, "Art");
                UIKit.Stretch(rt);
                _art = rt.gameObject.AddComponent<RawImage>();
                _art.texture = art;
                var fit = rt.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = art.width / (float)art.height;
            }
        }

        void Update()
        {
            _t += Time.unscaledDeltaTime;
            float a = _t * 0.035f;
            var target = _boat.position + new Vector3(0, 0.6f, 0);
            _cam.position = target + new Vector3(Mathf.Sin(a) * 6.5f, 1.3f + Mathf.Sin(_t * 0.21f) * 0.12f, -Mathf.Cos(a) * 6.5f);
            _cam.LookAt(target + new Vector3(Mathf.Sin(_t * 0.13f) * 0.3f, 0, 0));
            // the boat rides the swell
            _boat.position = new Vector3(0f, -0.45f + Mathf.Sin(_t * 0.6f) * 0.06f, 0f);
            _boat.rotation = Quaternion.Euler(Mathf.Sin(_t * 0.5f) * 2.5f, _t * 0.6f, Mathf.Sin(_t * 0.37f) * 3.5f);
            if (_art != null)
            {
                float z = 1.04f + Mathf.Sin(_t * 0.05f) * 0.03f;
                _art.rectTransform.localScale = new Vector3(z, z, 1f);
            }
            // after the game has been finished once, the box plays a third version now and then
            if (Game.Achievements.Profile.FinishedOnce && Game.UI.Top is MainMenu)
            {
                _isabelT += Time.unscaledDeltaTime;
                if (_isabelT > 95f)
                {
                    _isabelT = 0f;
                    Game.Audio.PlayAt("mb_lullaby_isabel", _boat.position + Vector3.up * 0.5f, Bus.Music, 0.7f, 1f, 2f, 40f, true);
                }
            }
        }
    }
}
