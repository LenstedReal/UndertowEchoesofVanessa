using System;
using System.Collections.Generic;

namespace Undertow.Core
{
    public struct EndingCutsceneDirective
    {
        public EndingType Ending;
        public string TitleKey;
        public bool ReleaseCollectedVoices;
        public bool SpawnAdrianAndVanessaShoreSilhouettes;
        public bool LeaveMusicBoxSilentAtHeart;
        public bool PlayRuthVoicemailOnSurface;
        public bool AdrianJoinsUndertowChorus;
        public string FinalAudioCueId;
    }

    public static class EndingEvaluator
    {
        /// <summary>
        /// Counts strictly unique, non-empty trimmed drawing IDs up to CanonicalGameData.TotalVanessaDrawings (6).
        /// Prevents duplicate entries or whitespace strings in tampered saves from falsely triggering the ANSWER ending.
        /// </summary>
        public static int CountValidUniqueDrawings(IReadOnlyList<string> drawingIds)
        {
            if (drawingIds == null || drawingIds.Count == 0)
                return 0;

            var uniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < drawingIds.Count; i++)
            {
                if (CanonicalGameData.TryNormalizeCollectibleId(drawingIds[i], out string normalized))
                {
                    uniqueIds.Add(normalized);
                    if (uniqueIds.Count >= CanonicalGameData.TotalVanessaDrawings)
                        break;
                }
            }
            return uniqueIds.Count;
        }

        /// <summary>
        /// Evaluates the exact canonical behavior-based ending per Section 6 of the Story Bible:
        /// 1. UNDERTOW: Player refused to answer in Act III (or never answered) OR chose Mimic's correctly sung lullaby at the final altar.
        /// 2. ANSWER: Player answered Vanessa in Act III AND discovered >= 4 of 6 unique drawings AND chose Echo's imperfectly sung lullaby ("Hush, little side").
        /// 3. SURFACE: Player answered in Act III and did not choose Mimic's lullaby, but did not satisfy all requirements for ANSWER (e.g., < 4 unique drawings).
        /// </summary>
        public static EndingType EvaluateEnding(CampaignState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            // No ending may be resolved until the final altar choice has actually been made.
            if (state.FinalAltarLullabyChoice == LullabyChoice.None)
                return EndingType.None;

            // Rule 1: Refusing to answer in Act III OR choosing Mimic's correctly sung lullaby leads to UNDERTOW.
            if (!state.AnsweredVanessaInMirrorRoom || state.FinalAltarLullabyChoice == LullabyChoice.MimicCorrect)
                return EndingType.Undertow;

            // Rule 2: Answered in Act III + >= 4 unique valid drawings + Echo's imperfect lullaby -> ANSWER.
            int uniqueDrawings = CountValidUniqueDrawings(state.DiscoveredDrawingIds);
            if (uniqueDrawings >= CanonicalGameData.RequiredDrawingsForAnswerEnding &&
                state.FinalAltarLullabyChoice == LullabyChoice.EchoIncorrect)
                return EndingType.Answer;

            // Rule 3: Answered in Act III and chose Echo, but missed the drawing threshold -> SURFACE.
            return EndingType.Surface;
        }

        public static EndingCutsceneDirective BuildEndingDirective(CampaignState state)
        {
            EndingType ending = EvaluateEnding(state);
            switch (ending)
            {
                case EndingType.Answer:
                    return new EndingCutsceneDirective
                    {
                        Ending = EndingType.Answer,
                        TitleKey = "ENDING_ANSWER_TITLE",
                        ReleaseCollectedVoices = true,
                        SpawnAdrianAndVanessaShoreSilhouettes = true,
                        LeaveMusicBoxSilentAtHeart = false,
                        PlayRuthVoicemailOnSurface = false,
                        AdrianJoinsUndertowChorus = false,
                        FinalAudioCueId = "SFX_VOICES_RELEASED_DAWN_SHORE"
                    };

                case EndingType.Surface:
                    return new EndingCutsceneDirective
                    {
                        Ending = EndingType.Surface,
                        TitleKey = "ENDING_SURFACE_TITLE",
                        ReleaseCollectedVoices = false,
                        SpawnAdrianAndVanessaShoreSilhouettes = false,
                        LeaveMusicBoxSilentAtHeart = true,
                        PlayRuthVoicemailOnSurface = true,
                        AdrianJoinsUndertowChorus = false,
                        FinalAudioCueId = "VO_RUTH_OLD_VOICEMAIL_STATIC"
                    };

                case EndingType.Undertow:
                    return new EndingCutsceneDirective
                    {
                        Ending = EndingType.Undertow,
                        TitleKey = "ENDING_UNDERTOW_TITLE",
                        ReleaseCollectedVoices = false,
                        SpawnAdrianAndVanessaShoreSilhouettes = false,
                        LeaveMusicBoxSilentAtHeart = false,
                        PlayRuthVoicemailOnSurface = false,
                        AdrianJoinsUndertowChorus = true,
                        FinalAudioCueId = "VO_ADRIAN_CALLING_NEXT_SOUL_INTO_SEA"
                    };
                default:
                    return new EndingCutsceneDirective { Ending = EndingType.None, TitleKey = "ENDING_NOT_READY" };
            }
        }
    }
}
