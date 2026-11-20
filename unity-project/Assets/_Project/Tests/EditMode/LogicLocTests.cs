using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Undertow.Logic;

namespace Undertow.Tests
{
    /// <summary>
    /// The shipped localisation tables (Resources/Undertow/Loc). The files are read from disk relative to
    /// the project folder, which is the working directory both in Unity's Test Runner and in the
    /// command-line logic harness.
    /// </summary>
    [TestFixture]
    public class LogicLocTests
    {
        const string LocDir = "Assets/_Project/Resources/Undertow/Loc";
        const string RuntimeDir = "Assets/_Project/Scripts/Runtime";
        static readonly Regex KeyLiteral = new Regex("\"((?:ui|msg|caption|hint|boot|ending|credits|drawing|speaker|tip|journal|voice|hud|toast)\\.[a-z0-9_.]+)\"");
        static readonly Regex Placeholder = new Regex(@"\{\d+\}");

        static LocTable Load(string code, List<string> errors = null)
        {
            string path = Path.Combine(LocDir, code + ".txt");
            Assert.IsTrue(File.Exists(path), "missing table " + path);
            return LocTable.Parse(code, File.ReadAllText(path), errors);
        }

        /// <summary>The same key set the editor validator checks, plus a journal line for every goal.</summary>
        static HashSet<string> RequiredKeys()
        {
            var keys = new HashSet<string>(ContentDb.RequiredKeys());
            foreach (var file in Directory.GetFiles(RuntimeDir, "*.cs", SearchOption.AllDirectories))
                foreach (Match m in KeyLiteral.Matches(File.ReadAllText(file))) keys.Add(m.Groups[1].Value);
            for (int i = 0; i < 4; i++) { keys.Add("ui.size." + i); keys.Add("ui.aa." + i); }
            for (int i = 0; i < 3; i++) keys.Add("ui.display." + i);
            foreach (var q in new[] { "low", "medium", "high", "ultra" }) keys.Add("ui.quality." + q);
            for (int i = 1; i <= 6; i++) keys.Add("drawing." + i);
            for (int i = 1; i <= 10; i++) keys.Add("tip." + i);
            keys.Add("msg.bells_solved_1");
            keys.Add("msg.bells_solved_2");
            foreach (var e in new[] { "answer", "surface", "undertow" })
            {
                keys.Add("ending." + e + ".title");
                keys.Add("ending." + e + ".line");
                for (int i = 1; i <= 5; i++) keys.Add("ending." + e + "." + i);
            }
            foreach (var k in Enum.GetNames(typeof(VoiceKind))) keys.Add("speaker." + k.ToLowerInvariant());
            foreach (var a in ContentDb.Acts)
            {
                foreach (var it in a.Items) if (it.Speaker != null) keys.Add("speaker." + it.Speaker);
                foreach (var g in a.Goals) keys.Add("journal." + g.Flag);
            }
            keys.RemoveWhere(k => k.EndsWith(".") || k.EndsWith("_"));
            return keys;
        }

        static IEnumerable<string> Codes => LocTable.Languages.Select(l => l.Code);

        [Test]
        public void EveryTableParsesWithoutErrors()
        {
            foreach (var code in Codes)
            {
                var errors = new List<string>();
                Load(code, errors);
                Assert.AreEqual(0, errors.Count, code + ": " + string.Join("; ", errors.Take(5)));
            }
        }

        [Test]
        public void EveryLanguageHasEveryKey()
        {
            var keys = RequiredKeys();
            Assert.Greater(keys.Count, 600, "key scan found too few keys");
            foreach (var code in Codes)
            {
                var t = Load(code);
                var missing = keys.Where(k => !t.Has(k)).OrderBy(k => k).ToList();
                Assert.AreEqual(0, missing.Count, code + " is missing " + missing.Count + ": " + string.Join(", ", missing.Take(8)));
            }
        }

        [Test]
        public void NoTableHasKeysEnglishLacks()
        {
            var en = Load("en");
            foreach (var code in Codes.Where(c => c != "en"))
            {
                var extra = Load(code).Keys.Where(k => !en.Has(k)).ToList();
                Assert.AreEqual(0, extra.Count, code + " has unknown keys (typos?): " + string.Join(", ", extra.Take(8)));
            }
        }

        [Test]
        public void PlaceholdersMatchEnglish()
        {
            var en = Load("en");
            foreach (var code in Codes.Where(c => c != "en"))
            {
                var t = Load(code);
                foreach (var key in en.Keys)
                {
                    if (!t.Has(key)) continue;
                    var want = string.Join(",", Placeholder.Matches(en.Get(key)).Cast<Match>().Select(m => m.Value).OrderBy(x => x));
                    var got = string.Join(",", Placeholder.Matches(t.Get(key)).Cast<Match>().Select(m => m.Value).OrderBy(x => x));
                    Assert.AreEqual(want, got, code + ": placeholders differ in " + key);
                }
            }
        }

        [Test]
        public void NoEmptyValues()
        {
            foreach (var code in Codes)
            {
                var t = Load(code);
                foreach (var key in t.Keys) Assert.IsFalse(string.IsNullOrWhiteSpace(t.Get(key)), code + ": empty value for " + key);
            }
        }

        [Test]
        public void NotesStayUnderEightyWords()
        {
            // Story bible, rule 1: no wall of text. Checked on the two reviewed languages.
            foreach (var code in new[] { "en", "tr" })
            {
                var t = Load(code);
                foreach (var key in t.Keys.Where(k => k.StartsWith("item.", StringComparison.Ordinal) && k.EndsWith(".body", StringComparison.Ordinal)))
                    Assert.LessOrEqual(LocTable.WordCount(t.Get(key), false), 80, code + ": " + key + " is longer than 80 words");
            }
        }

        [Test]
        public void TheTwoVoicesStayTellableApart()
        {
            // The real echo signs as the child signed her drawings; the mimic only knows her full name.
            // And the lullaby keeps its wrong word in every language: that is the clue the whole game rests on.
            foreach (var code in Codes)
            {
                var t = Load(code);
                Assert.AreEqual(t.Get("speaker.vanessa"), t.Get("speaker.echo"), code + ": the echo should be labelled with Vanessa's own nickname");
                Assert.AreNotEqual(t.Get("speaker.echo"), t.Get("speaker.mimic"), code + ": echo and mimic share a label");
                Assert.AreNotEqual(t.Get("voice.a1_boxes_echo"), t.Get("voice.a1_boxes_mimic"), code + ": the two lullabies read the same");
                Assert.AreNotEqual(t.Get("item.a4_choice_echo.prompt"), t.Get("item.a4_choice_mimic.prompt"), code + ": the two final songs read the same");
            }
        }

        [Test]
        public void TheDialHoursAppearInTheirClues()
        {
            // Act III: the three hours must survive translation in the notes that give them.
            foreach (var code in Codes)
            {
                var t = Load(code);
                StringAssertContains(t.Get("item.a1_log_stern.body"), "03:17", code);
                StringAssertContains(t.Get("item.a3_house_watch.body"), "06:05", code);
                StringAssertContains(t.Get("item.a1_exam_surg.body"), "11:40", code);
                StringAssertContains(t.Get("act.3.line"), "11:40", code);
            }
        }

        static void StringAssertContains(string text, string part, string code)
        {
            Assert.IsTrue(text != null && text.Contains(part), code + ": expected '" + part + "' in: " + text);
        }
    }
}
