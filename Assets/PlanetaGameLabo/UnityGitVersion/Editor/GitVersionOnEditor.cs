/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PlanetaGameLabo.UnityGitVersion.Editor
{
    /// <summary>
    /// A class including editor extension etc.
    /// </summary>
    internal class GitVersionOnEditor : IPreprocessBuildWithReport
    {
        public const string resourceDirectory = _resourceRootDirectory + GitVersion.resourceAssetDirectory;
        public const string versionHolderPath = resourceDirectory + GitVersionHolder.assetName + ".asset";
        public const string versionSettingPath = _gitVersionAssetRootDirectory + "Editor/UnityGitVersionSetting.asset";

        private const string _gitVersionAssetRootDirectory = "Assets/PlanetaGameLabo/UnityGitVersion/";
        private const string _resourceRootDirectory = _gitVersionAssetRootDirectory + "Resources/";

        int IOrderedCallback.callbackOrder => 0;

        /// <summary>
        /// Generate a version from git.
        /// </summary>
        /// <returns>Generated version</returns>
        public static GitVersion.Version GenerateVersionFromGit()
        {
            // 一時ファイルを無視するための.gitignoreを生成
            CreateGitIgnore();

            //設定の読み込み
            var setting = AssetDatabase.LoadAssetAtPath<GitVersionSetting>(versionSettingPath);
            if (!setting)
            {
                Debug.LogWarning("Failed to load GitVersion setting. Default setting will be used.");
                setting = ScriptableObject.CreateInstance<GitVersionSetting>();
            }

            // gitが利用不可能なら無効なバージョンを返す
            if (!GitOperator.CheckIfGitIsAvailable())
            {
                Debug.LogError("Git is not available. Please check if git is installed to your computer.");
                return GitVersion.Version.GetInvalidVersion();
            }

            // 各種情報の取得
            var commitId = GitOperator.GetLastCommitId(false);
            if (string.IsNullOrWhiteSpace(commitId))
            {
                Debug.LogError("Failed to generate version from git because commit ID is not available.");
                return GitVersion.Version.GetInvalidVersion();
            }

            var isModified = GitOperator.CheckIfRepositoryIsChangedFromLastCommit();
            var currentTag = GitOperator.GetTagFromCommitId(commitId);
            var diffHash = isModified ? GitOperator.GetHashOfChangesFromLastCommit(false) : "";

            string MatchEvaluator(Match match)
            {
                switch (match.Value)
                {
                    case "%c":
                        return GitOperator.GetLastCommitId(true);
                    case "%C":
                        return commitId;
                    case "%t":
                        if (string.IsNullOrWhiteSpace(currentTag))
                        {
                            Debug.LogWarning(
                                $"{match.Value} is not available when there are no tags for the last commit.");
                            return "";
                        }

                        return currentTag;
                    case "%d":
                        if (!isModified)
                        {
                            Debug.LogWarning(
                                $"{match.Value} is not available when there are no changes from the last commit.");
                            return "";
                        }

                        return GitOperator.GetHashOfChangesFromLastCommit(true);
                    case "%D":
                        if (!isModified)
                        {
                            Debug.LogWarning(
                                $"{match.Value} is not available when there are no changes from the last commit.");
                            return "";
                        }

                        return diffHash;
                    case "%x":
                        return GitOperator.GetDescription();
                    case "%%":
                        return "%";
                    default:
                        return match.Value;
                }
            }

            //バージョン情報の設定
            string versionString;
            if (isModified)
            {
                if (string.IsNullOrWhiteSpace(currentTag))
                {
                    versionString = setting.versionStringFormatWithDiff;
                }
                else
                {
                    versionString = setting.versionStringFormatWithTagAndDiff;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(currentTag))
                {
                    versionString = setting.versionStringFormat;
                }
                else
                {
                    versionString = setting.versionStringFormatWithTag;
                }
            }

            versionString = Regex.Replace(versionString, "%.", MatchEvaluator);
            return new GitVersion.Version(versionString, currentTag, commitId, diffHash);
        }

        /// <summary>
        /// Make directories if not exists.
        /// </summary>
        /// <param name="directoryPath">Directory path separated with "/".</param>
        public static void MakeAssetDirectoryRecursively(string directoryPath)
        {
            var directories = directoryPath.Split('/');
            var currentDirectory = "";
            foreach (var directory in directories)
            {
                var baseDirectory = currentDirectory;
                currentDirectory = Path.Combine(baseDirectory, directory);
                if (!AssetDatabase.IsValidFolder(currentDirectory))
                {
                    AssetDatabase.CreateFolder(baseDirectory, directory);
                }
            }
        }

        [MenuItem("Tools/GitVersion/Log VersionString")]
        private static void LogVersionString()
        {
            var versionString = GenerateVersionFromGit();
            Debug.Log(versionString.versionString);
        }

        private static void CreateVersionHolderAsset()
        {
            //内部バージョン情報アセットを作成
            var versionHolder = ScriptableObject.CreateInstance<GitVersionHolder>();
            versionHolder.version = GenerateVersionFromGit();
            //バージョンが有効な場合にのみ作成
            if (!versionHolder.version.isValid)
            {
                return;
            }

            //GitVersion用リソースディレクトリがなかったら作成
            MakeAssetDirectoryRecursively(resourceDirectory);
            //古い内部バージョンアセットが残っていたら削除
            AssetDatabase.DeleteAsset(versionHolderPath);
            //新しいアセットを保存
            AssetDatabase.CreateAsset(versionHolder, versionHolderPath);
            AssetDatabase.SaveAssets();
        }

        [InitializeOnLoadMethod]
        private static void CheckPlayModeState()
        {
            EditorApplication.playModeStateChanged += stateChange =>
            {
                if (stateChange == PlayModeStateChange.ExitingEditMode)
                {
                    CreateVersionHolderAsset();
                }
            };
        }

        /// <summary>
        /// Create .gitignore for GitVersion. If the file already exists, the file is overwritten.
        /// </summary>
        /// <remarks>
        /// Version information asset file saved in Resources directory should not be included to git because it is temporary information which changes time by time.
        /// So version information asset file should be added to .gitignore to be ignored by git .
        /// Easiest way it to add a name of the file to .gitignore in the project root directory but it affects not only UnityGitVersion but also all of the project.
        /// Because of this, it is better to add new .gitignore to UnityGitVersion directory.
        /// However files whose name starts with "." including .girtignore are not treated as asset by Unity and they cannot be included to unitypackage.
        /// Therefore .gitignore in UnityGitVersion directory won't imported when users download unitypakage and import them, and version information assert file won't ignored in this method.
        /// To avoid this, generate .gitignore dynamically in UnityGitVersion directory if it doesn't exist when UnityGitVersion runs, and make gitignore contain itself to ignore target of git.
        /// </remarks>
        private static void CreateGitIgnore()
        {
            if (!File.Exists(_gitVersionAssetRootDirectory + ".gitignore"))
            {
                Debug.Log($".gitignore is created to \"{_gitVersionAssetRootDirectory}\"");
            }

            using (var fs = File.CreateText(_gitVersionAssetRootDirectory + ".gitignore"))
            {
                fs.WriteLine("################");
                fs.WriteLine("# Generated by UnityGitVersion");
                fs.WriteLine("################");
                fs.WriteLine("");
                fs.WriteLine(".gitignore");
                fs.WriteLine($"{_resourceRootDirectory}*");
                fs.WriteLine($"{_resourceRootDirectory}Resources.meta");
            }
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            CreateVersionHolderAsset();
        }
    }
}