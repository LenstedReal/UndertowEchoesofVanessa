using System;
using System.Collections.Generic;
using UnityEngine;

namespace Undertow.Narrative
{
    public class DialogueSubtitleSystem : MonoBehaviour
    {
        [SerializeField] private string currentLanguageCode = "en";

        private readonly Dictionary<string, Dictionary<string, string>> stringTables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public event Action<string, string, float> OnSubtitleDisplayed;

        public string CurrentLanguageCode => currentLanguageCode;

        private void Awake()
        {
            SeedBuiltInLocalizationTables();
        }

        private static readonly Dictionary<string, string> PromptEn = new Dictionary<string, string>
        {
                ["PROMPT_ALIGN_CARGO_CRATE"] = "Push crate into ring",
                ["PROMPT_ANSWER_VANESSA"] = "Answer Vanessa",
                ["PROMPT_REFUSE_VANESSA"] = "Refuse to answer",
                ["PROMPT_CHOOSE_ECHO_LULLABY"] = "Sing the imperfect lullaby",
                ["PROMPT_CHOOSE_MIMIC_LULLABY"] = "Sing the perfect lullaby",
                ["PROMPT_CONTINUE_TO_NEXT_ACT"] = "Continue",
                ["PROMPT_FOLLOW_ECHO"] = "Follow the childlike voice",
                ["PROMPT_FOLLOW_MIMIC"] = "Follow the polished voice",
                ["PROMPT_INSPECT_VANESSA_DRAWING"] = "Inspect drawing",
                ["PROMPT_OPEN_CARGO_HATCH"] = "Open cargo hatch",
                ["PROMPT_PICKUP_FOCUSING_LENS"] = "Take focusing lens",
                ["PROMPT_PICKUP_GLASS_PRISM"] = "Take glass prism",
                ["PROMPT_PICKUP_SURGEONS_HOOK"] = "Take surgeon's hook",
                ["PROMPT_PICKUP_WHALE_OIL"] = "Take whale oil",
                ["PROMPT_READ_CRANE_LEDGER"] = "Read Crane ledger",
                ["PROMPT_READ_JOURNAL"] = "Read journal",
                ["PROMPT_STRIKE_BELL_LA"] = "Strike bell: La",
                ["PROMPT_GRAVEYARD_BELL_LA"] = "Strike bell: La",
                ["PROMPT_STRIKE_BELL_MI"] = "Strike bell: Mi",
                ["PROMPT_GRAVEYARD_BELL_MI"] = "Strike bell: Mi",
                ["PROMPT_STRIKE_BELL_DO"] = "Strike bell: Do",
                ["PROMPT_GRAVEYARD_BELL_DO"] = "Strike bell: Do",
                ["PROMPT_CLOCK_DIAL_1"] = "Turn clock dial 1",
                ["PROMPT_CLOCK_DIAL_2"] = "Turn clock dial 2",
                ["PROMPT_CLOCK_DIAL_3"] = "Turn clock dial 3",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_1"] = "Rotate mirror 1",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_2"] = "Rotate mirror 2",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_3"] = "Rotate mirror 3",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_4"] = "Rotate mirror 4",
        };

        private static readonly Dictionary<string, string> PromptTr = new Dictionary<string, string>
        {
                ["PROMPT_ALIGN_CARGO_CRATE"] = "Sandığı halkaya it",
                ["PROMPT_ANSWER_VANESSA"] = "Vanessa'ya yanıt ver",
                ["PROMPT_REFUSE_VANESSA"] = "Yanıt verme",
                ["PROMPT_CHOOSE_ECHO_LULLABY"] = "Kusurlu ninniyi söyle",
                ["PROMPT_CHOOSE_MIMIC_LULLABY"] = "Kusursuz ninniyi söyle",
                ["PROMPT_CONTINUE_TO_NEXT_ACT"] = "Devam et",
                ["PROMPT_FOLLOW_ECHO"] = "Çocuksu sesi takip et",
                ["PROMPT_FOLLOW_MIMIC"] = "Cilalı sesi takip et",
                ["PROMPT_INSPECT_VANESSA_DRAWING"] = "Resmi incele",
                ["PROMPT_OPEN_CARGO_HATCH"] = "Kargo kapağını aç",
                ["PROMPT_PICKUP_FOCUSING_LENS"] = "Odaklama merceğini al",
                ["PROMPT_PICKUP_GLASS_PRISM"] = "Cam prizmayı al",
                ["PROMPT_PICKUP_SURGEONS_HOOK"] = "Cerrah kancasını al",
                ["PROMPT_PICKUP_WHALE_OIL"] = "Balina yağını al",
                ["PROMPT_READ_CRANE_LEDGER"] = "Crane defterini oku",
                ["PROMPT_READ_JOURNAL"] = "Günlüğü oku",
                ["PROMPT_STRIKE_BELL_LA"] = "Çana vur: La",
                ["PROMPT_GRAVEYARD_BELL_LA"] = "Çana vur: La",
                ["PROMPT_STRIKE_BELL_MI"] = "Çana vur: Mi",
                ["PROMPT_GRAVEYARD_BELL_MI"] = "Çana vur: Mi",
                ["PROMPT_STRIKE_BELL_DO"] = "Çana vur: Do",
                ["PROMPT_GRAVEYARD_BELL_DO"] = "Çana vur: Do",
                ["PROMPT_CLOCK_DIAL_1"] = "Saat kadranını çevir 1",
                ["PROMPT_CLOCK_DIAL_2"] = "Saat kadranını çevir 2",
                ["PROMPT_CLOCK_DIAL_3"] = "Saat kadranını çevir 3",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_1"] = "Aynayı çevir 1",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_2"] = "Aynayı çevir 2",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_3"] = "Aynayı çevir 3",
                ["PROMPT_ROTATE_TEMPLE_MIRROR_4"] = "Aynayı çevir 4",
        };

        public void SeedBuiltInLocalizationTables()
        {
            var en = new Dictionary<string, string>
            {
                ["OBJ_ACT1_FIND_LANTERN_OIL"] = "Find whale oil to steady the lantern flame.",
                ["OBJ_ACT1_THORNE_JOURNAL"] = "Search the Captain's Cabin aboard the Merciful Requiem.",
                ["OBJ_ACT1_FIVE_BELLS"] = "Ring the five ship bells in the order written in Thorne's margin (La - Mi - Do - Mi - La).",
                ["OBJ_ACT1_CARGO_ESCAPE"] = "Use the surgeon's hook to unlatch the lower hatch and align two crates into the starboard rings.",
                ["THOUGHT_TENT_MEMORY"] = "Fifteen years. The tent canvas barely moved, and still I never unzipped it.",
                ["ECHO_LULLABY_LINE"] = "Hush, little side, don't say a word...",
                ["MIMIC_LULLABY_LINE"] = "Hush, little child, don't say a word...",
                ["JOURNAL_CAPTAIN_THORNE_1724"] = "1724, Merciful Requiem: 'The sea beneath Port Saint-Clair does not drown men—it catalogues the calls nobody answered.'",
                ["JOURNAL_CRANE_BLOODLINE"] = "Parish Registry, 1724: Harbourmaster Elias Crane gave the bell-code to silence the harbor when his daughter vanished.",
                ["ENDING_ANSWER_TITLE"] = "ENDING I: ANSWER — Two silhouettes stand where the tide lets go.",
                ["ENDING_SURFACE_TITLE"] = "ENDING II: SURFACE — The music box remains below. On deck, Ruth's old voicemail plays through static.",
                ["ENDING_UNDERTOW_TITLE"] = "ENDING III: UNDERTOW — A new voice rises from the fog, wearing Adrian's name."
            };

            var tr = new Dictionary<string, string>
            {
                ["OBJ_ACT1_FIND_LANTERN_OIL"] = "Fenerin alevini sabitlemek için balina yağı bul.",
                ["OBJ_ACT1_THORNE_JOURNAL"] = "Merciful Requiem güvertesindeki Kaptan Kamarasını ara.",
                ["OBJ_ACT1_FIVE_BELLS"] = "Geminin beş çanını Thorne'un günlüğündeki sırayla çal (La - Mi - Do - Mi - La).",
                ["OBJ_ACT1_CARGO_ESCAPE"] = "Cerrah kancasıyla alt kapağı aç ve iki sandığı sancak halkalarına yerleştir.",
                ["THOUGHT_TENT_MEMORY"] = "On beş yıl. Çadırın bezi zar zor kıpırdamıştı ama fermuarı yine de açmadım.",
                ["ECHO_LULLABY_LINE"] = "Hush, little side... (Vanessa'nın çocuksu, hatalı söylediği ninni)",
                ["MIMIC_LULLABY_LINE"] = "Hush, little child... (Undertow'un kusursuz ve yaşlı taklidi)",
                ["JOURNAL_CAPTAIN_THORNE_1724"] = "1724, Merciful Requiem: 'Port Saint-Clair'in altındaki deniz insanları boğmaz; kimsenin yanıtlamadığı çağrıları saklar.'",
                ["JOURNAL_CRANE_BLOODLINE"] = "1724 Kilise Kaydı: Liman Reisi Elias Crane, kızı kaybolduğunda limanı susturmak için çan şifresini mühürledi.",
                ["ENDING_ANSWER_TITLE"] = "SON I: YANIT (ANSWER) — Gelgitin çekildiği kıyıda iki silüet yan yana duruyor.",
                ["ENDING_SURFACE_TITLE"] = "SON II: YÜZEY (SURFACE) — Müzik kutusu derinde sessiz kalır. Ruth'un eski sesli mesajı duyulur.",
                ["ENDING_UNDERTOW_TITLE"] = "SON III: UNDERTOW — Sisin içinden Adrian'ın sesiyle yeni bir çağrı yükselir."
            };

            foreach (var kv in PromptEn) en[kv.Key] = kv.Value;
            foreach (var kv in PromptTr) tr[kv.Key] = kv.Value;
            stringTables["en"] = en;
            stringTables["tr"] = tr;
        }

        public void SetLanguage(string languageCode)
        {
            if (stringTables.ContainsKey(languageCode))
                currentLanguageCode = languageCode;
        }

        public string Localize(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (stringTables.TryGetValue(currentLanguageCode, out var table) && table.TryGetValue(key, out var val))
                return val;
            if (stringTables.TryGetValue("en", out var enTable) && enTable.TryGetValue(key, out var fallback))
                return fallback;
            return key;
        }

        public string PresentSubtitle(string speakerTag, string localizationKey, float durationSeconds = 4.2f)
        {
            string resolved = Localize(localizationKey);
            OnSubtitleDisplayed?.Invoke(speakerTag, resolved, durationSeconds);
            return resolved;
        }
    }
}
