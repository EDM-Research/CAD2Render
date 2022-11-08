/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEditor;

namespace PlanetaGameLabo.UnityGitVersion.Editor
{
    /// <summary>
    /// Editor window to set configurations about GitVersion.
    /// </summary>
    internal class GitVersionSettingWindow : EditorWindow
    {
        private GitVersionSetting _gitVersionSetting;
        private bool _isAssetCreated;

        [MenuItem("Tools/GitVersion/Setting")]
        private static void ShowWindow()
        {
            var window = GetWindow(typeof(GitVersionSettingWindow), false, "GitVersion");
            window.Show();
        }

        private void OnFocus()
        {
            if (_gitVersionSetting)
            {
                return;
            }

            _gitVersionSetting =
                AssetDatabase.LoadAssetAtPath<GitVersionSetting>(GitVersionOnEditor.versionSettingPath);
            _isAssetCreated = true;
            if (_gitVersionSetting)
            {
                return;
            }

            _gitVersionSetting = CreateInstance<GitVersionSetting>();
            _isAssetCreated = false;
        }

        private void OnLostFocus()
        {
            if (!_isAssetCreated)
            {
                GitVersionOnEditor.MakeAssetDirectoryRecursively(GitVersionOnEditor.resourceDirectory);
                AssetDatabase.CreateAsset(_gitVersionSetting, GitVersionOnEditor.versionSettingPath);
                _isAssetCreated = true;
            }

            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Version String Formats", EditorStyles.boldLabel);
            _gitVersionSetting.versionStringFormat =
                EditorGUILayout.TextField("Standard", _gitVersionSetting.versionStringFormat);
            _gitVersionSetting.versionStringFormatWithDiff =
                EditorGUILayout.TextField("With Diff", _gitVersionSetting.versionStringFormatWithDiff);
            _gitVersionSetting.versionStringFormatWithTag =
                EditorGUILayout.TextField("With Tag", _gitVersionSetting.versionStringFormatWithTag);
            _gitVersionSetting.versionStringFormatWithTagAndDiff = EditorGUILayout.TextField("With Tag and Diff",
                _gitVersionSetting.versionStringFormatWithTagAndDiff);
            EditorGUILayout.LabelField("Others", EditorStyles.boldLabel);
        }
    }
}