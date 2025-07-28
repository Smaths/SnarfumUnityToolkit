using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// TODO: 
// 1. Change SetDirty to properly handle serialized fields for undo support. 

namespace DocumentationNote.Editor
{
    [CustomEditor(typeof(DocumentationNote))]
    public class DocumentationNoteEditor : UnityEditor.Editor
    {
        private bool _isEditing = false;
        private Vector2 _scroll;

        private static bool _areStylesSetup;
        private static GUIStyle _h1Style;
        private static GUIStyle _h2Style;
        private static GUIStyle _h3Style;
        private static GUIStyle _codeBlockStyle;
        private static GUIStyle _linkStyle;
        private static GUIStyle _listStyle;
        private static GUIStyle _bodyStyle;

        private static readonly Regex _boldRegex = new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
        private static readonly Regex _italicRegex = new Regex(@"\*(.+?)\*", RegexOptions.Compiled);
        private static readonly Regex _inlineCodeSplit = new Regex("(`[^`]+`)", RegexOptions.Compiled);
        private static readonly Regex _codeFenceRegex = new Regex(@"^\s*```", RegexOptions.Compiled);
        private static readonly char[] _newlineDelimiter = new[] { '\n' };

        private void OnEnable()
        {
            if (!_areStylesSetup)
                SetupStyles();
        }

        public override void OnInspectorGUI()
        {
            DocumentationNote note = (DocumentationNote)target;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawComponentHeaderSection(note);
            DrawMarkdownSection(note);
            GUILayout.Space(8);
            DrawUrlSection(note);
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
                EditorUtility.SetDirty(note);
        }

        private static void SetupStyles()
        {
            _h1Style = new GUIStyle(EditorStyles.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.grey },
            };

            _h2Style = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.grey },
            };

            _h3Style = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.grey },
            };

            _codeBlockStyle = new GUIStyle(EditorStyles.textArea)
            {
                richText = false,
                normal = { textColor = Color.white },
                fontSize = 12,
                alignment = TextAnchor.UpperLeft,
                wordWrap = false,
            };

            _linkStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                clipping = TextClipping.Clip,
                wordWrap = false,
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft,
                contentOffset = new Vector2(0f, -1f)
            };

            _listStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = true,
                normal = { textColor = Color.grey },
            };

            _bodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = true,
                normal = { textColor = Color.grey },
            };

            _areStylesSetup = true;
        }

        private void DrawComponentHeaderSection(DocumentationNote note)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Documentation Note", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_isEditing ? "Done" : "Edit", GUILayout.MinWidth(40), GUILayout.MaxWidth(50)))
            {
                note.markdown = note.markdown.Trim();
                _isEditing = !_isEditing;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMarkdownSection(DocumentationNote note)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            if (_isEditing)
            {
                note.markdown = EditorGUILayout.TextArea(note.markdown, EditorStyles.wordWrappedLabel,
                    GUILayout.ExpandHeight(true));
                EditorGUILayout.LabelField("Use Markdown syntax to style text. No inline links yet.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                if (string.IsNullOrEmpty(note.markdown))
                    EditorGUILayout.LabelField("No documentation available.", EditorStyles.centeredGreyMiniLabel);
                else
                    RenderMarkdown(note.markdown);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawUrlSection(DocumentationNote note)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (_isEditing)
            {
                EditorGUILayout.LabelField("Add Link", EditorStyles.boldLabel, GUILayout.Width(54f));
                note.documentationUrl = EditorGUILayout.TextField(note.documentationUrl, GUILayout.MinWidth(50f),
                    GUILayout.MaxWidth(520f));
                if (GUILayout.Button("X", GUILayout.Width(30f)))
                    note.documentationUrl = string.Empty;
            }
            else
            {
                if (!string.IsNullOrEmpty(note.documentationUrl))
                {
                    EditorGUILayout.LabelField("Link ↗", EditorStyles.boldLabel, GUILayout.Width(38f));

                    // Make it expand to fill remaining space
                    if (GUILayout.Button(note.documentationUrl, _linkStyle, GUILayout.ExpandWidth(true)))
                    {
                        if (TryGetAbsoluteUri(note.documentationUrl, out var uri))
                        {
                            var toOpen = Uri.EscapeUriString(uri.AbsoluteUri);
                            Debug.Log($"Opening URL: {toOpen}");

                            // First try Unity’s API
                            try
                            {
                                Application.OpenURL(toOpen);
                            }
                            catch
                            {
#if UNITY_EDITOR_OSX
                                // Fallback for macOS if Unity fails with -50
                                System.Diagnostics.Process.Start("open", toOpen);
#elif UNITY_EDITOR_WIN
                    System.Diagnostics.Process.Start("cmd", $"/c start {toOpen}");
#else
                    Debug.LogError($"Could not open URL: {toOpen}");
#endif
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Invalid URL format: '{note.documentationUrl}'");
                        }
                    }

                    // Change cursor to link‑pointer when hovering
                    var lastRect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        #region Helpers
        private static void RenderMarkdown(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                EditorGUILayout.LabelField("No documentation available.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // Split lines while preserving code block sections
            string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
            bool inCodeBlock = false;
            StringBuilder codeBuffer = new StringBuilder();

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                // Handle code blocks with triple backticks
                if (_codeFenceRegex.IsMatch(line))
                {
                    if (inCodeBlock)
                    {
                        // End code block
                        EditorGUILayout.SelectableLabel(codeBuffer.ToString(), _codeBlockStyle,
                            GUILayout.ExpandHeight(true)
                        );
                        codeBuffer.Clear();
                        inCodeBlock = false;
                    }
                    else
                    {
                        inCodeBlock = true;
                    }

                    continue;
                }

                if (inCodeBlock)
                {
                    codeBuffer.AppendLine(rawLine);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    GUILayout.Space(4);
                    continue;
                }

                // Render headers
                if (line.StartsWith("### "))
                {
                    EditorGUILayout.LabelField(line.Substring(4), _h3Style);
                }
                else if (line.StartsWith("## "))
                {
                    EditorGUILayout.LabelField(line.Substring(3), _h2Style);
                }
                else if (line.StartsWith("# "))
                {
                    EditorGUILayout.LabelField(line.Substring(2), _h1Style);
                }

                // Render list items
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    string listItem = ConvertInlineMarkdownToRichText(line.Substring(2));
                    EditorGUILayout.LabelField($"• {listItem}", _listStyle);
                }
                else
                {
                    string converted = ConvertInlineMarkdownToRichText(line);
                    EditorGUILayout.LabelField(converted, _bodyStyle);
                }
            }

            // If file ends while still in code block
            if (inCodeBlock && codeBuffer.Length > 0)
            {
                EditorGUILayout.TextArea(codeBuffer.ToString(), _codeBlockStyle);
            }
        }

        private static string ConvertInlineMarkdownToRichText(string line)
        {
            line = _boldRegex.Replace(line, "<b>$1</b>");
            line = _italicRegex.Replace(line, "<i>$1</i>");
            line = Regex.Replace(line, @"`(.+?)`", "<color=#FFFFFF>$1</color>");
            return line;
        }

        private static bool TryGetAbsoluteUri(string raw, out Uri uri)
        {
            raw = raw.Trim();

            // 1. If it already has a scheme, try that first
            if (Uri.TryCreate(raw, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }

            // 2. Otherwise, prefix "http://"
            string withScheme = "http://" + raw;
            if (Uri.TryCreate(withScheme, UriKind.Absolute, out uri))
            {
                return true;
            }

            uri = null;
            return false;
        }
        #endregion
    }
}