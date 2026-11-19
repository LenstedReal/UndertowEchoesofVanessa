using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Player;

namespace Undertow.UI
{
    public enum AntiAliasingMode { TAA = 0, DLAA = 1, SMAA = 2 }

    [Serializable]
    public class UserSettingsProfile
    {
        public AntiAliasingMode AntiAliasing = AntiAliasingMode.TAA;
        public bool EnableSSR = true;
        public bool EnableVolumetricFog = true;
        public bool EnableContactShadows = true;
        public float ExposureCompensationEv = -0.35f;
        public float MouseSensitivity = 1.65f;
        public float MasterVolume = 0.90f;
        public float VoiceVolume = 1.00f;
        public float SfxVolume = 0.85f;
        public float MusicVolume = 0.78f;
        public bool StreamerSafeAudioMode = true;
        public int ResolutionWidth = 3840;
        public int ResolutionHeight = 2160;
        public bool Fullscreen = true;
        public bool VSync = true;
        public int TargetFrameRate = 60;
        public int QualityLevel = 0;
    }

    public sealed class GraphicsAndAudioSettingsManager : MonoBehaviour
    {
        private const string PreferencesKey = "UNDERTOW_USER_SETTINGS_V1";
        [SerializeField] private UserSettingsProfile currentProfile = new UserSettingsProfile();
        public UserSettingsProfile CurrentProfile => currentProfile;

        private void Awake()
        {
            if (PlayerPrefs.HasKey(PreferencesKey))
            {
                try
                {
                    UserSettingsProfile loaded = JsonUtility.FromJson<UserSettingsProfile>(PlayerPrefs.GetString(PreferencesKey));
                    if (loaded != null) currentProfile = loaded;
                }
                catch (Exception ex) { Debug.LogWarning("[Undertow] Settings reset to defaults: " + ex.Message); }
            }
            ApplyProfile(currentProfile, false);
        }

        public void ApplyProfile(UserSettingsProfile profile) => ApplyProfile(profile, true);

        private void ApplyProfile(UserSettingsProfile profile, bool persist)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            currentProfile = profile;
            currentProfile.MasterVolume = Mathf.Clamp01(currentProfile.MasterVolume);
            currentProfile.MouseSensitivity = Mathf.Clamp(currentProfile.MouseSensitivity, 0.1f, 10f);
            currentProfile.TargetFrameRate = Mathf.Clamp(currentProfile.TargetFrameRate, 30, 240);
            currentProfile.QualityLevel = Mathf.Clamp(currentProfile.QualityLevel, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            AudioListener.volume = currentProfile.MasterVolume;
            QualitySettings.SetQualityLevel(currentProfile.QualityLevel, true);
            QualitySettings.vSyncCount = currentProfile.VSync ? 1 : 0;
            Application.targetFrameRate = currentProfile.VSync ? -1 : currentProfile.TargetFrameRate;

            int width = Mathf.Max(1280, currentProfile.ResolutionWidth);
            int height = Mathf.Max(720, currentProfile.ResolutionHeight);
            // Do not request a mode beyond the connected display's native capability.
            Resolution display = Screen.currentResolution;
            if (display.width < width || display.height < height)
            {
                width = Mathf.Max(1280, display.width);
                height = Mathf.Max(720, display.height);
            }
            Screen.SetResolution(width, height, currentProfile.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            foreach (Camera camera in FindObjectsOfType<Camera>())
            {
                HDAdditionalCameraData hd = camera.GetComponent<HDAdditionalCameraData>();
                if (hd != null)
                {
                    hd.antialiasing = currentProfile.AntiAliasing == AntiAliasingMode.SMAA
                        ? ResolveSmaaMode()
                        : HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                }
                PlayerCameraLook look = camera.GetComponent<PlayerCameraLook>();
                if (look != null) look.MouseSensitivity = currentProfile.MouseSensitivity;
            }

            foreach (Volume volume in FindObjectsOfType<Volume>())
            {
                VolumeProfile runtimeProfile = volume.profile;
                if (runtimeProfile == null) continue;
                if (runtimeProfile.TryGet(out ScreenSpaceReflection ssr)) ssr.active = currentProfile.EnableSSR;
                if (runtimeProfile.TryGet(out Fog fog)) fog.active = currentProfile.EnableVolumetricFog;
            }

            if (persist)
            {
                PlayerPrefs.SetString(PreferencesKey, JsonUtility.ToJson(currentProfile));
                PlayerPrefs.Save();
            }
        }

        public void SetUHDResolution()
        {
            currentProfile.ResolutionWidth = 3840;
            currentProfile.ResolutionHeight = 2160;
            ApplyProfile(currentProfile);
        }

        public void SetDisplayResolution()
        {
            currentProfile.ResolutionWidth = Screen.currentResolution.width;
            currentProfile.ResolutionHeight = Screen.currentResolution.height;
            ApplyProfile(currentProfile);
        }

        public void SetMasterVolume(float value)
        {
            currentProfile.MasterVolume = Mathf.Clamp01(value);
            AudioListener.volume = currentProfile.MasterVolume;
            PlayerPrefs.SetString(PreferencesKey, JsonUtility.ToJson(currentProfile));
            PlayerPrefs.Save();
        }

        private static HDAdditionalCameraData.AntialiasingMode ResolveSmaaMode()
        {
            // The enum member name differs between HDRP versions, so resolve it by name.
            foreach (string candidate in new[] { "SubpixelMorphologicalAntialiasing", "SMAA" })
            {
                if (System.Enum.TryParse<HDAdditionalCameraData.AntialiasingMode>(candidate, out HDAdditionalCameraData.AntialiasingMode mode))
                    return mode;
            }
            return HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
        }
    }
}
