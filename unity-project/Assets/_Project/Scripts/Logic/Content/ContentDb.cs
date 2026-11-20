using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    /// <summary>All acts, built once. Scene names map to acts for the runtime.</summary>
    public static class ContentDb
    {
        public const string MenuScene = "MainMenu";
        public const string EndingsScene = "Endings_Resolution";

        static ActDef[] _acts;

        public static ActDef[] Acts => _acts ?? (_acts = new[] { Act1Content.Build(), Act2Content.Build(), Act3Content.Build(), Act4Content.Build() });

        public static ActDef Act(int id) => id >= 1 && id <= 4 ? Acts[id - 1] : null;

        public static ActDef ActForScene(string scene) => Acts.FirstOrDefault(a => string.Equals(a.SceneName, scene, StringComparison.Ordinal));

        public static IEnumerable<string> AllScenes()
        {
            yield return MenuScene;
            foreach (var a in Acts) yield return a.SceneName;
            yield return EndingsScene;
        }

        /// <summary>Every localisation key the content needs (used by the localisation completeness test).</summary>
        public static IEnumerable<string> RequiredKeys()
        {
            foreach (var a in Acts)
            {
                yield return a.NameKey;
                yield return a.LineKey;
                foreach (var r in a.Rooms) yield return r.NameKey;
                foreach (var i in a.Items)
                {
                    yield return i.PromptKey;
                    if (i.HasText)
                    {
                        yield return i.TitleKey;
                        yield return i.BodyKey;
                    }
                    if (i.Need != null && i.Need.Length > 0) yield return i.NeedKey;
                }
                foreach (var g in a.Goals) yield return g.TextKey;
                foreach (var v in a.Voices) yield return v.LineKey;
            }
            foreach (var ac in AchievementProfile.All)
            {
                yield return ac.TitleKey;
                yield return ac.DescKey;
            }
        }

        /// <summary>
        /// The state a chapter starts with when picked from the chapter menu without a snapshot:
        /// every flag a later act depends on, average oil and sanity.
        /// </summary>
        public static GameState ChapterStartState(int act)
        {
            var s = new GameState();
            s.BeginAct(act);
            if (act >= 2)
            {
                foreach (var f in new[] { "a1_left", "a1_locket", "a1_music", "a1_crates" }) s.Set(f);
            }
            if (act >= 3)
            {
                foreach (var f in new[] { "a2_left", "a2_nail", "a2_bells" }) s.Set(f);
                s.Act2Path = EchoPath.Echo;
            }
            if (act >= 4)
            {
                foreach (var f in new[] { "a3_left", "a3_clocks", "a3_tree", "a3_choice" }) s.Set(f);
                s.Answered = 1;
            }
            s.Oil = Balance.StartOil;
            s.Sanity = Balance.SanityStart;
            s.Checkpoint = "start";
            return s;
        }
    }
}
