using UnityEngine;
using UnityEngine.UI;

namespace PlanetaGameLabo.UnityGitVersion
{
    /// <summary>
    /// A component to display version string to uGUI text.
    /// </summary>
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("PlanetaGameLabo/UnityGitVersion/GitVersionGUIText")]
    public sealed class GitVersionGUIText : MonoBehaviour
    {
        private Text _myText;

        private void Awake()
        {
            _myText = GetComponent<Text>();
            _myText.text = GitVersion.version.versionString;
        }
    }
}
