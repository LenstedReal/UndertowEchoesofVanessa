using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>Every option in the settings menu. Saved as JSON next to the save slots.</summary>
    public sealed class GameSettings
    {
        public static readonly string[] QualityKeys = { "ui.quality.low", "ui.quality.medium", "ui.quality.high", "ui.quality.ultra" };
        public static readonly int[] FrameCaps = { 0, 30, 60, 120, 144 };

        // first run
        public string Language = "";
        public bool LanguageChosen;
        public bool Calibrated;
        public bool WarningSeen;

        // gameplay & accessibility
        public bool Subtitles = true;
        public int SubtitleSize = 1;            // 0 small, 1 medium, 2 large, 3 huge
        public float SubtitleBackground = 0.55f;
        public bool SpeakerNames = true;
        public bool SoundCaptions = true;
        public bool DirectionArrows = true;
        public bool ObjectiveHints = true;
        public bool Crosshair = true;
        public bool HudVisible = true;
        public bool ShowVersion = true;
        public float Fov = 90f;                 // horizontal field of view, degrees
        public float HeadBob = 0.6f;
        public float CameraShake = 0.7f;
        public bool MotionBlur;
        public bool InvertY;
        public float MouseSensitivity = 1f;
        public float PadSensitivity = 1f;
        public bool ToggleSprint;
        public bool ToggleCrouch = true;
        public bool ToggleListen;
        public float SanityEffects = 1f;        // strength of distortion post effects (accessibility)
        public bool Rumble = true;

        // graphics
        public int DisplayMode;                 // 0 borderless, 1 exclusive fullscreen, 2 windowed
        public int ResolutionWidth;
        public int ResolutionHeight;
        public bool VSync = true;
        public int FrameCapIndex;
        public int Quality = 2;
        public float Brightness;                // exposure compensation, EV
        public int AntiAliasing = 2;            // 0 off, 1 FXAA, 2 SMAA, 3 TAA
        public bool FilmGrain = true;
        public bool ChromaticAberration = true;
        public bool DepthOfField = true;

        // audio
        public float Master = 0.9f;
        public float Music = 0.8f;
        public float Effects = 0.9f;
        public float Voice = 1f;
        public float Ambience = 0.85f;
        public bool Limiter = true;             // dynamic range control: nothing ever spikes
        public bool Mono;
        public bool StreamerMode;

        public event Action Changed;

        public void NotifyChanged()
        {
            Apply();
            Changed?.Invoke();
        }

        public int FrameCap => FrameCaps[MathX.Clamp(FrameCapIndex, 0, FrameCaps.Length - 1)];

        // ------------------------------------------------------------------ persistence
        public static GameSettings Load(string path)
        {
            var s = new GameSettings();
            try
            {
                if (File.Exists(path) && MiniJson.TryParse(File.ReadAllText(path), out var root) && root is Dictionary<string, object> d)
                    s.Read(d);
                else
                    s.Quality = FirstRunQuality(SystemInfo.graphicsMemorySize, SystemInfo.systemMemorySize);
            }
            catch (Exception e)
            {
                Game.Warn("settings unreadable, using defaults: " + e.Message);
            }
            return s;
        }

        /// <summary>
        /// First start only: older or smaller graphics cards begin on a lighter preset (the player can
        /// raise it in Settings). 0 = Low, 1 = Medium, 2 = High.
        /// </summary>
        public static int FirstRunQuality(int vramMB, int ramMB)
        {
            if (vramMB > 0 && vramMB < 1800) return 0;
            if (ramMB > 0 && ramMB < 7000) return 0;
            if (vramMB > 0 && vramMB < 3500) return 1;
            return 2;
        }

        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, MiniJson.Serialize(Write(), true));
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Game.Warn("settings not saved: " + e.Message);
            }
        }

        Dictionary<string, object> Write() => new Dictionary<string, object>
        {
            ["language"] = Language, ["languageChosen"] = LanguageChosen, ["calibrated"] = Calibrated, ["warningSeen"] = WarningSeen,
            ["subtitles"] = Subtitles, ["subtitleSize"] = SubtitleSize, ["subtitleBackground"] = (double)SubtitleBackground,
            ["speakerNames"] = SpeakerNames, ["soundCaptions"] = SoundCaptions, ["directionArrows"] = DirectionArrows,
            ["objectiveHints"] = ObjectiveHints, ["crosshair"] = Crosshair, ["hud"] = HudVisible, ["showVersion"] = ShowVersion,
            ["fov"] = (double)Fov, ["headBob"] = (double)HeadBob, ["cameraShake"] = (double)CameraShake, ["motionBlur"] = MotionBlur,
            ["invertY"] = InvertY, ["mouseSensitivity"] = (double)MouseSensitivity, ["padSensitivity"] = (double)PadSensitivity,
            ["toggleSprint"] = ToggleSprint, ["toggleCrouch"] = ToggleCrouch, ["toggleListen"] = ToggleListen, ["sanityEffects"] = (double)SanityEffects, ["rumble"] = Rumble,
            ["displayMode"] = DisplayMode, ["resW"] = ResolutionWidth, ["resH"] = ResolutionHeight, ["vsync"] = VSync,
            ["frameCap"] = FrameCapIndex, ["quality"] = Quality, ["brightness"] = (double)Brightness, ["aa"] = AntiAliasing,
            ["filmGrain"] = FilmGrain, ["chromatic"] = ChromaticAberration, ["dof"] = DepthOfField,
            ["master"] = (double)Master, ["music"] = (double)Music, ["effects"] = (double)Effects, ["voice"] = (double)Voice,
            ["ambience"] = (double)Ambience, ["limiter"] = Limiter, ["mono"] = Mono, ["streamer"] = StreamerMode,
        };

        void Read(Dictionary<string, object> d)
        {
            Language = MiniJson.Str(d, "language", Language);
            LanguageChosen = MiniJson.Bool(d, "languageChosen", LanguageChosen);
            Calibrated = MiniJson.Bool(d, "calibrated", Calibrated);
            WarningSeen = MiniJson.Bool(d, "warningSeen", WarningSeen);
            Subtitles = MiniJson.Bool(d, "subtitles", Subtitles);
            SubtitleSize = MathX.Clamp(MiniJson.Int(d, "subtitleSize", SubtitleSize), 0, 3);
            SubtitleBackground = MathX.Clamp01(MiniJson.Float(d, "subtitleBackground", SubtitleBackground));
            SpeakerNames = MiniJson.Bool(d, "speakerNames", SpeakerNames);
            SoundCaptions = MiniJson.Bool(d, "soundCaptions", SoundCaptions);
            DirectionArrows = MiniJson.Bool(d, "directionArrows", DirectionArrows);
            ObjectiveHints = MiniJson.Bool(d, "objectiveHints", ObjectiveHints);
            Crosshair = MiniJson.Bool(d, "crosshair", Crosshair);
            HudVisible = MiniJson.Bool(d, "hud", HudVisible);
            ShowVersion = MiniJson.Bool(d, "showVersion", ShowVersion);
            Fov = MathX.Clamp(MiniJson.Float(d, "fov", Fov), 70f, 120f);
            HeadBob = MathX.Clamp01(MiniJson.Float(d, "headBob", HeadBob));
            CameraShake = MathX.Clamp01(MiniJson.Float(d, "cameraShake", CameraShake));
            MotionBlur = MiniJson.Bool(d, "motionBlur", MotionBlur);
            InvertY = MiniJson.Bool(d, "invertY", InvertY);
            MouseSensitivity = MathX.Clamp(MiniJson.Float(d, "mouseSensitivity", MouseSensitivity), 0.1f, 4f);
            PadSensitivity = MathX.Clamp(MiniJson.Float(d, "padSensitivity", PadSensitivity), 0.1f, 4f);
            ToggleSprint = MiniJson.Bool(d, "toggleSprint", ToggleSprint);
            ToggleCrouch = MiniJson.Bool(d, "toggleCrouch", ToggleCrouch);
            ToggleListen = MiniJson.Bool(d, "toggleListen", ToggleListen);
            SanityEffects = MathX.Clamp01(MiniJson.Float(d, "sanityEffects", SanityEffects));
            Rumble = MiniJson.Bool(d, "rumble", Rumble);
            DisplayMode = MathX.Clamp(MiniJson.Int(d, "displayMode", DisplayMode), 0, 2);
            ResolutionWidth = MiniJson.Int(d, "resW", 0);
            ResolutionHeight = MiniJson.Int(d, "resH", 0);
            VSync = MiniJson.Bool(d, "vsync", VSync);
            FrameCapIndex = MathX.Clamp(MiniJson.Int(d, "frameCap", FrameCapIndex), 0, FrameCaps.Length - 1);
            Quality = MathX.Clamp(MiniJson.Int(d, "quality", Quality), 0, 3);
            Brightness = MathX.Clamp(MiniJson.Float(d, "brightness", Brightness), -2f, 2f);
            AntiAliasing = MathX.Clamp(MiniJson.Int(d, "aa", AntiAliasing), 0, 3);
            FilmGrain = MiniJson.Bool(d, "filmGrain", FilmGrain);
            ChromaticAberration = MiniJson.Bool(d, "chromatic", ChromaticAberration);
            DepthOfField = MiniJson.Bool(d, "dof", DepthOfField);
            Master = MathX.Clamp01(MiniJson.Float(d, "master", Master));
            Music = MathX.Clamp01(MiniJson.Float(d, "music", Music));
            Effects = MathX.Clamp01(MiniJson.Float(d, "effects", Effects));
            Voice = MathX.Clamp01(MiniJson.Float(d, "voice", Voice));
            Ambience = MathX.Clamp01(MiniJson.Float(d, "ambience", Ambience));
            Limiter = MiniJson.Bool(d, "limiter", Limiter);
            Mono = MiniJson.Bool(d, "mono", Mono);
            StreamerMode = MiniJson.Bool(d, "streamer", StreamerMode);
        }

        // ------------------------------------------------------------------ apply to the engine
        int _appliedW, _appliedH, _appliedMode = -1;

        public void Apply()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = VSync ? -1 : (FrameCap > 0 ? FrameCap : -1);
            QualitySettings.anisotropicFiltering = Quality >= 2 ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Enable;
            QualitySettings.globalTextureMipmapLimit = Quality == 0 ? 1 : 0;
            QualitySettings.lodBias = Quality == 0 ? 0.7f : (Quality == 1 ? 1f : (Quality == 2 ? 1.5f : 2f));

            if (!Application.isEditor)
            {
                var mode = DisplayMode == 1 ? FullScreenMode.ExclusiveFullScreen : (DisplayMode == 2 ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
                int w = ResolutionWidth > 0 ? ResolutionWidth : Screen.currentResolution.width;
                int h = ResolutionHeight > 0 ? ResolutionHeight : Screen.currentResolution.height;
                if (w != _appliedW || h != _appliedH || (int)mode != _appliedMode)
                {
                    Screen.SetResolution(w, h, mode);
                    _appliedW = w;
                    _appliedH = h;
                    _appliedMode = (int)mode;
                }
            }
        }

        public void ResetSection(string section)
        {
            var d = new GameSettings();
            switch (section)
            {
                case "gameplay":
                    Subtitles = d.Subtitles; SubtitleSize = d.SubtitleSize; SubtitleBackground = d.SubtitleBackground; SpeakerNames = d.SpeakerNames;
                    SoundCaptions = d.SoundCaptions; DirectionArrows = d.DirectionArrows; ObjectiveHints = d.ObjectiveHints; Crosshair = d.Crosshair;
                    HudVisible = d.HudVisible; Fov = d.Fov; HeadBob = d.HeadBob; CameraShake = d.CameraShake; MotionBlur = d.MotionBlur; InvertY = d.InvertY;
                    MouseSensitivity = d.MouseSensitivity; PadSensitivity = d.PadSensitivity; ToggleSprint = d.ToggleSprint; ToggleCrouch = d.ToggleCrouch;
                    ToggleListen = d.ToggleListen; SanityEffects = d.SanityEffects; ShowVersion = d.ShowVersion; Rumble = d.Rumble;
                    break;
                case "graphics":
                    VSync = d.VSync; FrameCapIndex = d.FrameCapIndex; Quality = d.Quality; Brightness = d.Brightness; AntiAliasing = d.AntiAliasing;
                    FilmGrain = d.FilmGrain; ChromaticAberration = d.ChromaticAberration; DepthOfField = d.DepthOfField;
                    break;
                case "audio":
                    Master = d.Master; Music = d.Music; Effects = d.Effects; Voice = d.Voice; Ambience = d.Ambience; Limiter = d.Limiter; Mono = d.Mono; StreamerMode = d.StreamerMode;
                    break;
            }
            NotifyChanged();
        }
    }
}
