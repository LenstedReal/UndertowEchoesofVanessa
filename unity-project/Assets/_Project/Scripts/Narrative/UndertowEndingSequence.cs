using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Core;

namespace Undertow.Narrative
{
    /// <summary>Resolves the final campaign state into one of the three canonical ending tableaux.</summary>
    public sealed class UndertowEndingSequence : MonoBehaviour
    {
        [SerializeField] private GameObject answerTableau;
        [SerializeField] private GameObject surfaceTableau;
        [SerializeField] private GameObject undertowTableau;
        [SerializeField] private Light dawnLight;
        private EndingCutsceneDirective directive;
        private bool validEnding;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle footerStyle;
        private float reveal;

        private void Start()
        {
            CampaignState state = ActProgressionManager.Instance != null ? ActProgressionManager.Instance.State : null;
            if (state == null) return;
            directive = EndingEvaluator.BuildEndingDirective(state);
            validEnding = directive.Ending != EndingType.None;
            if (!validEnding) return;
            if (answerTableau != null) answerTableau.SetActive(directive.Ending == EndingType.Answer);
            if (surfaceTableau != null) surfaceTableau.SetActive(directive.Ending == EndingType.Surface);
            if (undertowTableau != null) undertowTableau.SetActive(directive.Ending == EndingType.Undertow);
            if (dawnLight != null)
            {
                dawnLight.color = directive.Ending == EndingType.Answer ? new Color(0.56f, 0.76f, 0.88f) :
                    directive.Ending == EndingType.Surface ? new Color(0.76f, 0.55f, 0.34f) : new Color(0.025f, 0.08f, 0.12f);
                dawnLight.intensity = directive.Ending == EndingType.Undertow ? 0.18f : 1.15f;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!validEnding) return;
            reveal = Mathf.MoveTowards(reveal, 1f, Time.unscaledDeltaTime * 0.22f);
            if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
                SceneManager.LoadScene("MainMenu");
        }

        private void OnGUI()
        {
            if (!validEnding) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.height / 15, 34, 74), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.92f, 0.88f, 0.79f) } };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.height / 48, 18, 28), alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.76f, 0.83f, 0.84f) } };
                footerStyle = new GUIStyle(bodyStyle) { fontSize = Mathf.Clamp(Screen.height / 80, 12, 18), normal = { textColor = new Color(0.55f, 0.68f, 0.7f) } };
            }
            GUI.color = new Color(1f, 1f, 1f, reveal);
            float width = Screen.width * 0.76f;
            float x = (Screen.width - width) * 0.5f;
            string title = directive.Ending == EndingType.Answer ? "ANSWER" : directive.Ending == EndingType.Surface ? "SURFACE" : "UNDERTOW";
            string body = directive.Ending == EndingType.Answer
                ? "The voices are finally released. At the edge of the tide, two silhouettes stand together."
                : directive.Ending == EndingType.Surface
                    ? "The music box falls silent. Adrian returns to the surface, carrying Ruth's last message."
                    : "The chorus has learned another voice. Somewhere beyond the dark, Adrian calls again.";
            GUI.Label(new Rect(x, Screen.height * 0.62f, width, 90f), title, titleStyle);
            GUI.Label(new Rect(x + width * 0.08f, Screen.height * 0.72f, width * 0.84f, 90f), body, bodyStyle);
            GUI.Label(new Rect(x, Screen.height * 0.91f, width, 32f), "PRESS ANY KEY TO RETURN TO THE MAIN MENU", footerStyle);
            GUI.color = Color.white;
        }
    }
}
