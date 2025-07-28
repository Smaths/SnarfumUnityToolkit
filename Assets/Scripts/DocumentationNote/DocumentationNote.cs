using UnityEngine;
using UnityEngine.Serialization;

namespace DocumentationNote
{
    [ExecuteAlways]
    public class DocumentationNote : MonoBehaviour
    {
        [TextArea(2, 5)]
        public string documentationUrl = "https://collidascope.atlassian.net/wiki/spaces/COL/overview";

        [FormerlySerializedAs("markdownNote")]
        [TextArea(8, 20)] 
        public string markdown = "# Heading 1\n## Heading 2\nSome **bold** text.";

        private void Reset()
        {
            documentationUrl = string.Empty;
            markdown = string.Empty;
        }
    }
}