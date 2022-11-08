/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlanetaGameLabo.UnityGitVersion.Editor
{
    /// <summary>
    /// A class to operate git and get information.
    /// </summary>
    public static class GitOperator
    {
        /// <summary>
        /// Check if git is installed and available.
        /// </summary>
        /// <returns>True if git is available.</returns>
        public static bool CheckIfGitIsAvailable()
        {
            try
            {
                return ExecuteCommand("--version").exitCode != 0;
            }
            catch (CommandExecutionErrorException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Check if there are any changes in the repository from the last commit.
        /// </summary>
        /// <returns>True if there are changes.</returns>
        public static bool CheckIfRepositoryIsChangedFromLastCommit()
        {
            try
            {
                var result = ExecuteGitCommand("status --short");
                return !string.IsNullOrWhiteSpace(result.Replace("\n", string.Empty));
            }
            catch (GitCommandExecutionError e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Get current tag of the specified commit.
        /// </summary>
        /// <param name="commitId">An id of the target commit</param>
        /// <returns>Tag. If there are no tags for the commit ID or git is not available, this function returns empty string.</returns>
        public static string GetTagFromCommitId(string commitId)
        {
            try
            {
                var tag = ExecuteGitCommand("tag -l --contains " + commitId).Replace("\n", string.Empty);
                return string.IsNullOrEmpty(tag) ? "" : tag;
            }
            catch (GitCommandExecutionError e)
            {
                Debug.LogException(e);
                return "";
            }
        }

        /// <summary>
        /// Get current a last commit id of the current branch.
        /// </summary>
        /// <param name="shortVersion">Returns short commit id if this is true.</param>
        /// <returns>Commit ID. If there are no commits or git is not available, this function returns empty string.</returns>
        public static string GetLastCommitId(bool shortVersion)
        {
            try
            {
                var commitId = ExecuteGitCommand($"rev-parse{(shortVersion ? " --short" : "")} HEAD")
                    .Replace("\n", string.Empty);
                if (!string.IsNullOrEmpty(commitId))
                {
                    return commitId;
                }

                Debug.LogError(
                    "Failed to get commit id. Check if git is installed and the directory of this project is initialized as a git repository.");
                return "";
            }
            catch (GitCommandExecutionError e)
            {
                Debug.LogException(e);
                return "";
            }
        }

        /// <summary>
        /// Get a hash of the difference between current repository and last commit.
        /// </summary>
        /// <param name="shortVersion">Returns short hash with 7 characters if this is true.</param>
        /// <returns>A SHA1 hash of diff between current repository and last commit</returns>
        public static string GetHashOfChangesFromLastCommit(bool shortVersion)
        {
            try
            {
                // コミットしていない変更がある場合は最新コミットとの差分のハッシュを生成しバージョンに加える。
                // 異なる編集の存在するものは違うバージョン、同じ状態のものは同じバージョンであると保証するため
                ExecuteGitCommand("add -N .");
                var diff = ExecuteGitCommand("diff HEAD");
                // gitに合わせてSHA1ハッシュを生成
                var hash = GetHashString<SHA1CryptoServiceProvider>(diff);
                // 短縮版の場合はgitに合わせて7文字にする
                return shortVersion ? hash.Substring(0, 7) : hash;
            }
            catch (GitCommandExecutionError e)
            {
                Debug.LogException(e);
                return "";
            }
        }

        /// <summary>
        /// Get a result of git describe
        /// </summary>
        /// <returns>A result of git describe</returns>
        public static string GetDescription()
        {
            try
            {
                return ExecuteGitCommand("describe").Replace("\n", string.Empty);
            }
            catch (GitCommandExecutionError e)
            {
                Debug.LogException(e);
                return "";
            }
        }

        private static string ExecuteGitCommand(string arguments, float timeOutSeconds = 10)
        {
            try
            {
                // ページ送りによるユーザー入力待機を発生させないために--no-pagerオプションを使用
                var (standardOutput, standardError, exitCode) =
                    ExecuteCommand($"git --no-pager {arguments}", timeOutSeconds);
                if (exitCode != 0)
                {
                    throw new GitCommandExecutionError(arguments, exitCode, standardError);
                }

                return standardOutput;
            }
            catch (CommandExecutionErrorException e)
            {
                throw new GitCommandExecutionError(arguments, e.Message);
            }
        }

        private static (string standardOutput, string standardError, int exitCode) ExecuteCommand(string command,
            float timeOutSeconds = 10)
        {
            //Processオブジェクトを作成
            using (var process = new System.Diagnostics.Process())
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        //ComSpec(cmd.exe)のパスを取得して、FileNameプロパティに指定
                        var cmdPath = Environment.GetEnvironmentVariable("ComSpec");
                        if (cmdPath == null)
                        {
                            throw new CommandExecutionErrorException(command,
                                "Command Prompt is not found because environment variable \"ComSpec\" doesn'T exist.");
                        }

                        process.StartInfo.FileName = cmdPath;
                        process.StartInfo.Arguments = "/c " + command;
                        break;
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.LinuxEditor:
                        process.StartInfo.FileName = "/bin/bash";
                        process.StartInfo.Arguments = "-c \" " + command + "\"";
                        break;
                    default:
                    {
                        throw new CommandExecutionErrorException(command,
                            $"Command execution is not supported in current platform ({Application.platform}).");
                    }
                }

                //出力を読み取れるようにする
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = false;
                //ウィンドウを表示しないようにする
                process.StartInfo.CreateNoWindow = true;

                //起動
                process.Start();

                // 出力を読み取る
                // 出力ストリームのバッファがいっぱいになりブロックされるのを防ぐため、非同期で出力ストリームを読み込みながらコマンドの終了を待つ
                using (var standardOutputTask = Task.Run(async () => await process.StandardOutput.ReadToEndAsync()))
                using (var standardErrorTask = Task.Run(async () => await process.StandardError.ReadToEndAsync()))
                {
                    //プロセス終了まで待機する
                    if (!process.WaitForExit((int) (timeOutSeconds * 1000)))
                    {
                        // タイムアウトになった場合はプロセスを中断して終了
                        process.Kill();
                        process.WaitForExit();
                        standardOutputTask.Wait();
                        standardErrorTask.Wait();
                        throw new CommandExecutionErrorException(command, "Timeout");
                    }

                    standardOutputTask.Wait();
                    standardErrorTask.Wait();
                    return (standardOutputTask.Result.Replace("\r\n", "\n"),
                        standardErrorTask.Result.Replace("\r\n", "\n"), process.ExitCode);
                }
            }
        }

        private static string GetHashString<T>(string text) where T : HashAlgorithm, new()
        {
            var data = Encoding.UTF8.GetBytes(text);
            using (var algorithm = new T())
            {
                var hashBytes = algorithm.ComputeHash(data);
                // バイト型配列を16進数文字列に変換
                var result = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    result.Append(hashByte.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }

    /// <summary>
    /// An exception class for git command execution error.
    /// </summary>
    public sealed class GitCommandExecutionError : Exception
    {
        public GitCommandExecutionError(string arguments, int exitCode, string standardError) : base(
            $"Failed to execute git command with arguments \"{arguments}\" and exit code \"{exitCode}\". \"{standardError}\"")
        {
        }

        public GitCommandExecutionError(string arguments, string standardError) : base(
            $"Failed to execute git command with arguments \"{arguments}\". \"{standardError}\"")
        {
        }
    }

    internal sealed class CommandExecutionErrorException : Exception
    {
        public CommandExecutionErrorException(string command, string reason) : base(
            $"Failed to execute command \"{command}\" due to \"{reason}\"")
        {
        }
    }
}