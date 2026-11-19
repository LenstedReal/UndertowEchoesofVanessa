using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Audio
{
    public struct VocalCueMetadata
    {
        public string CueId;
        public bool IsEchoTrueVanessa;
        public string SungLyric;
        public float FundamentalPitchHz;
        public float FormantShiftRatio;
        public Vector3 WorldOrigin;
        public bool LeadsToSafety;
    }

    public class EchoMimicVoiceDirector : MonoBehaviour
    {
        public event Action<VocalCueMetadata> OnVocalCueEmitted;

        public VocalCueMetadata CreateEchoCue(string cueId, Vector3 worldOrigin)
        {
            var cue = new VocalCueMetadata
            {
                CueId = cueId,
                IsEchoTrueVanessa = true,
                SungLyric = CanonicalGameData.EchoLullabyLyric,
                FundamentalPitchHz = 349.23f,
                FormantShiftRatio = 1.18f,
                WorldOrigin = worldOrigin,
                LeadsToSafety = true
            };
            OnVocalCueEmitted?.Invoke(cue);
            return cue;
        }

        public VocalCueMetadata CreateMimicCue(string cueId, Vector3 worldOrigin)
        {
            var cue = new VocalCueMetadata
            {
                CueId = cueId,
                IsEchoTrueVanessa = false,
                SungLyric = CanonicalGameData.MimicLullabyLyric,
                FundamentalPitchHz = 246.94f,
                FormantShiftRatio = 0.92f,
                WorldOrigin = worldOrigin,
                LeadsToSafety = false
            };
            OnVocalCueEmitted?.Invoke(cue);
            return cue;
        }
    }
}
