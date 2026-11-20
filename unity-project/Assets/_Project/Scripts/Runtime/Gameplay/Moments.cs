using System.Collections;
using System.Linq;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>The scripted "clip moments" that close each act, and the two choices of the story.</summary>
    public static class Moments
    {
        static Vector3 AnchorOr(ActRuntime rt, string name, Vector3 fallback)
        {
            var a = rt.World.AnchorOrNull(name);
            return a != null ? a.position : fallback;
        }

        static void Say(VoiceKind k, string key, float seconds, Vector3? from = null, string clip = null)
        {
            if (!Game.Loc.Has(key)) return;
            Game.UI.Subtitles.Say(k, Game.Loc[key], seconds, from);
            var id = clip ?? ("vo_" + key.Replace("voice.", ""));
            if (Game.Audio.Clip(id) == null) id = "vo_hum_" + k.ToString().ToLowerInvariant();
            if (from.HasValue) Game.Audio.PlayAt(id, from.Value, Bus.Voice, 1f, 1f, 2.5f, 40f, true);
            else Game.Audio.Play2D(id, Bus.Voice, 0.9f);
        }

        static void Caption(string key, float seconds, Vector3? at = null)
        {
            if (Game.Settings.SoundCaptions && Game.Loc.Has(key)) Game.UI.Subtitles.Caption(Game.Loc[key], seconds, at);
        }

        // ------------------------------------------------------------------ act exits
        public static IEnumerator ActExit(ActRuntime rt, ItemDef it)
        {
            rt.BeginMoment();
            var p = rt.Player;
            switch (rt.Act.Id)
            {
                case 1:
                {
                    var boxes = AnchorOr(rt, "boxes", rt.World.ItemPoint(it));
                    Game.Audio.Silence(Balance.SilenceBeforeKeyMoment);
                    yield return p.LookAt(boxes, 1.6f, false);
                    yield return new WaitForSeconds(0.8f);
                    Game.Audio.PlayAt("mb_two_voices", boxes, Bus.Music, 1f, 1f, 1.5f, 25f, true);
                    yield return new WaitForSeconds(1.2f);
                    Say(VoiceKind.Echo, "voice.a1_boxes_echo", 4.5f, boxes + Vector3.left * 0.3f);
                    yield return new WaitForSeconds(4.6f);
                    Say(VoiceKind.Mimic, "voice.a1_boxes_mimic", 4.5f, boxes + Vector3.right * 0.3f);
                    yield return new WaitForSeconds(4.8f);
                    // the lights die, something vast passes under the keel, the whole hull groans
                    var lamp = rt.World.AnchorOrNull("capt_lamp");
                    if (lamp != null) foreach (var l in lamp.GetComponentsInChildren<Light>()) l.enabled = false;
                    foreach (var l in rt.World.FlickerLights) if (l != null) l.enabled = false;
                    rt.Player.Lantern.Flicker.Strength = 0f;
                    Game.Audio.Play2D("sfx_hull_groan_big", Bus.Effects, 1f);
                    Caption("caption.something_passes", 4f);
                    p.Shake(0.9f, 3.5f);
                    Game.Input.Rumble(0.6f, 0.3f, 2.5f);
                    yield return new WaitForSeconds(3.2f);
                    break;
                }
                case 2:
                {
                    var at = rt.World.ItemPoint(it);
                    if (it.Kind == ItemKind.Boat)
                    {
                        // Echo: a still cove, the little boat, her humming out over the water
                        yield return p.LookAt(at + Vector3.forward * 6f, 1.8f, false);
                        Game.Audio.PlayAt("lead_echo", at + Vector3.forward * 5f, Bus.Voice, 0.8f, 1f, 2f, 30f, true);
                        Say(VoiceKind.Echo, "voice.a2_end_echo", 5f, at + Vector3.forward * 5f);
                        Game.Audio.PlayAt("sfx_oars", at, Bus.Effects, 0.8f);
                        yield return new WaitForSeconds(5.5f);
                    }
                    else
                    {
                        // Mimic: the hulks close in behind and the way back is gone
                        yield return p.LookAt(at + Vector3.forward * 4f, 1.4f, false);
                        Say(VoiceKind.Mimic, "voice.a2_end_mimic", 4.5f, at + Vector3.forward * 4f);
                        yield return new WaitForSeconds(2.2f);
                        Game.Audio.Play2D("sfx_walls_close", Bus.Effects, 1f);
                        Caption("caption.walls_close", 4f);
                        p.Shake(0.7f, 3f);
                        rt.World.Atmos.SetFogDistance(4f);
                        yield return new WaitForSeconds(3.5f);
                    }
                    break;
                }
                case 3:
                {
                    var at = rt.World.ItemPoint(it);
                    yield return p.LookAt(at + Vector3.down * 2f + Vector3.right * 3f, 1.6f, false);
                    Game.Audio.Play2D("sfx_sea_enter", Bus.Effects, 1f);
                    Caption("caption.sea_enter", 3.5f);
                    Say(VoiceKind.Echo, "voice.a3_end_echo", 4f, at + Vector3.right * 4f);
                    yield return new WaitForSeconds(3.5f);
                    break;
                }
            }
            yield return Game.UI.Fade(1f, 2.2f);
            rt.FinishAct();
        }

        // ------------------------------------------------------------------ Act III: the mirror hall
        public static IEnumerator MirrorHall(ActRuntime rt, ItemDef it)
        {
            rt.BeginMoment();
            var p = rt.Player;
            var mirror = rt.World.ItemPoint(it);
            Game.Audio.Silence(Balance.SilenceBeforeKeyMoment + 1f);
            yield return p.LookAt(mirror, 1.6f, false);
            rt.World.Atmos.SetDepthOfField(true, Vector3.Distance(p.EyePos, mirror));
            yield return new WaitForSeconds(1.0f);
            // in the glass: the tent, a seventeen-year-old with his hands over his ears
            var self = rt.World.AnchorOrNull("mirror_self");
            if (self != null) self.gameObject.SetActive(true);
            Caption("caption.mirror_tent", 3.5f, mirror);
            yield return new WaitForSeconds(2.4f);
            var outside = mirror - p.Forward * 9f + Vector3.up * 0.4f;
            Say(VoiceKind.Echo, "voice.a3_mirror_call", 3.2f, outside, "vo_a3_mirror_call");
            yield return new WaitForSeconds(1.2f);
            bool answered = false, done = false;
            Game.UI.Push(new AnswerPrompt(7.5f, a => { answered = a; done = true; }));
            while (!done) yield return null;
            rt.World.Atmos.SetDepthOfField(false, 0f);
            var r = rt.Session.RecordAnswer(answered);
            if (r.Achievements.Count > 0) Game.Achievements.UnlockAll(r.Achievements);
            if (answered)
            {
                Say(VoiceKind.Adrian, "voice.a3_answer", 3.5f, null, "vo_a3_answer");
                if (self != null) self.gameObject.SetActive(false);
                p.Shake(0.3f, 2f);
                Game.Audio.Play2D("sfx_water_recede", Bus.Effects, 1f);
                Caption("caption.water_recede", 3.5f);
                foreach (var f in r.FlagsSet) rt.RaiseFlagEvent(f);
                yield return new WaitForSeconds(3.2f);
                rt.SaveCheckpointNow("a3_sea_open");
                rt.EndMoment();
                yield break;
            }
            // silence: the hall fills and pulls him down
            Game.Audio.Play2D("sfx_flood", Bus.Effects, 1f);
            Caption("caption.flood", 4f);
            foreach (var w in rt.World.Waters) if (w != null && !w.Sea) w.AnimateTo(w.Level + 2.6f, 4.5f);
            p.Shake(0.5f, 4f);
            yield return new WaitForSeconds(2.5f);
            Say(VoiceKind.Mimic, "voice.a3_silent_mimic", 3.5f, mirror);
            yield return new WaitForSeconds(2.2f);
            yield return Game.UI.Fade(1f, 2f);
            rt.FinishAct();
        }

        // ------------------------------------------------------------------ Act IV: the altar and the last choice
        public static IEnumerator Altar(ActRuntime rt, ItemDef it)
        {
            rt.BeginMoment();
            var p = rt.Player;
            var at = rt.World.ItemPoint(it);
            Game.Audio.Silence(Balance.SilenceBeforeKeyMoment);
            yield return p.LookAt(at + Vector3.up * 1.5f, 1.8f, false);
            Game.Audio.PlayMusic("mus_choir", 3f, 0.9f, false);
            Caption("caption.choir", 4f);
            p.Shake(0.25f, 5f);
            yield return new WaitForSeconds(2.5f);
            Say(VoiceKind.Choir, "voice.a4_choir", 6f, at + Vector3.up * 3f);
            yield return new WaitForSeconds(6.2f);
            Game.UI.Hud.Message(Game.Loc["msg.altar_choose"], 6f);
            rt.EndMoment();
        }

        public static IEnumerator AltarChoice(ActRuntime rt, ItemDef it)
        {
            rt.BeginMoment();
            var p = rt.Player;
            var at = rt.World.ItemPoint(it);
            bool echo = it.Index == 1;
            yield return p.LookAt(at, 1.2f, false);
            Game.Audio.StopMusic(2f);
            Game.Audio.PlayAt(echo ? "mb_lullaby_echo" : "mb_lullaby_mimic", at, Bus.Music, 1f, 1f, 1.5f, 30f, true);
            yield return new WaitForSeconds(1.5f);
            Say(echo ? VoiceKind.Echo : VoiceKind.Mimic, echo ? "voice.a4_choice_echo_end" : "voice.a4_choice_mimic_end", 4.5f, at);
            yield return new WaitForSeconds(5f);
            p.Shake(0.4f, 3f);
            Game.Audio.Play2D(echo ? "sfx_release" : "sfx_undertow_pull", Bus.Effects, 1f);
            Caption(echo ? "caption.release" : "caption.pull", 4f);
            rt.World.Atmos.Flash(echo ? 0.8f : 0.2f);
            yield return new WaitForSeconds(2.5f);
            yield return Game.UI.Fade(1f, 3f);
            rt.FinishAct();
        }
    }
}
