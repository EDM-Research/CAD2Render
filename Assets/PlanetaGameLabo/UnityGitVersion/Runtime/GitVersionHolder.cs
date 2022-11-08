/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;

namespace PlanetaGameLabo.UnityGitVersion
{
    /// <summary>
    /// This is a scriptable to hold a version string in executables, and this is created in prebuild process and removed in post-build process.
    /// This object is included in executables and referred from scripts in executables.
    /// In editor, this object is not created and referred.
    /// </summary>
    public class GitVersionHolder : ScriptableObject
    {
        /// <summary>
        /// A name of the asset file to hold a version string.
        /// </summary>
        public const string assetName = "Version";

        /// <summary>
        /// A path of the asset file to hold a version string.
        /// This is referred when load version holder.
        /// </summary>
        public const string assetPath = GitVersion.resourceAssetDirectory + assetName;

        [SerializeField] public GitVersion.Version version = GitVersion.Version.GetInvalidVersion();
    }
}