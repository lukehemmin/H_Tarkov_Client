/* GameStarter.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 * reider123
 * Merijn Hendriks
 */


using Aki.Launcher.Helpers;
using Aki.Launcher.MiniCommon;
using Aki.Launcher.Models.Launcher;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aki.Launcher.Controllers;
using Aki.Launcher.Interfaces;
using System.Runtime.InteropServices;

namespace Aki.Launcher
{
    public class GameStarter
    {
        private readonly IGameStarterFrontend _frontend;
        private readonly bool _showOnly;
        private readonly string _originalGamePath;
        private readonly string _gamePath;
        private readonly string[] _excludeFromCleanup;
        private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";

        private const string registrySettings = @"Software\Battlestate Games\EscapeFromTarkov";

        public GameStarter(IGameStarterFrontend frontend, string gamePath = null, string originalGamePath = null,
            bool showOnly = false, string[] excludeFromCleanup = null)
        {
            _frontend = frontend;
            _showOnly = showOnly;
            _gamePath = gamePath ?? LauncherSettingsProvider.Instance.GamePath ?? Environment.CurrentDirectory;
            _originalGamePath = originalGamePath ??= DetectOriginalGamePath();
            _excludeFromCleanup = excludeFromCleanup ?? LauncherSettingsProvider.Instance.ExcludeFromCleanup;
        }

        private static string DetectOriginalGamePath()
        {
            // We can't detect the installed path on non-Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            var uninstallStringValue = Registry.LocalMachine.OpenSubKey(registryInstall, false)
                ?.GetValue("UninstallString");
            var info = (uninstallStringValue is string key) ? new FileInfo(key) : null;
            return info?.DirectoryName;
        }

        public async Task<GameStarterResult> LaunchGame(ServerInfo server, AccountInfo account)
        {
            // setup directories
            if (IsInstalledInLive())
            {
                LogManager.Instance.Warning("Failed installed in live check");
                return GameStarterResult.FromError(-1);
            }

            SetupGameFiles();

            if (!ValidationUtil.Validate())
            {
                LogManager.Instance.Warning("Failed validation check");
                return GameStarterResult.FromError(-2);
            }

            if (account.wipe)
            {
                RemoveRegistryKeys();
                CleanTempFiles();
            }

            // check game path
            var clientExecutable = Path.Join(_gamePath, "EscapeFromTarkov.exe");

            if (!File.Exists(clientExecutable))
            {
                LogManager.Instance.Warning($"Could not find {clientExecutable}");
                return GameStarterResult.FromError(-6);
            }

            // apply patches
            ProgressReportingPatchRunner patchRunner = new ProgressReportingPatchRunner(_gamePath);

            try
            {
                await _frontend.CompletePatchTask(patchRunner.PatchFiles());
            }
            catch (TaskCanceledException)
            {
                LogManager.Instance.Warning("Failed to apply assembly patch");
                return GameStarterResult.FromError(-4);
            }
            
            //start game
            var args =
                $"-force-gfx-jobs native -token={account.id} -config={Json.Serialize(new ClientConfig(server.backendUrl))}";

            if (_showOnly)
            {
                Console.WriteLine($"{clientExecutable} {args}");
            }
            else
            {
                var clientProcess = new ProcessStartInfo(clientExecutable)
                {
                    Arguments = args,
                    UseShellExecute = false,
                    WorkingDirectory = _gamePath,
                };

                Process.Start(clientProcess);
            }

            return GameStarterResult.FromSuccess();
        }

        bool IsInstalledInLive()
        {
            var isInstalledInLive = false;

            try
            {
                var files = new FileInfo[]
                {
                    // aki files
                    new FileInfo(Path.Combine(_originalGamePath, @"Aki.Launcher.exe")),
                    new FileInfo(Path.Combine(_originalGamePath, @"Aki.Server.exe")),
                    new FileInfo(Path.Combine(_originalGamePath, @"EscapeFromTarkov_Data\Managed\Aki.Build.dll")),
                    new FileInfo(Path.Combine(_originalGamePath, @"EscapeFromTarkov_Data\Managed\Aki.Common.dll")),
                    new FileInfo(Path.Combine(_originalGamePath, @"EscapeFromTarkov_Data\Managed\Aki.Reflection.dll")),

                    // bepinex files
                    new FileInfo(Path.Combine(_originalGamePath, @"doorstep_config.ini")),
                    new FileInfo(Path.Combine(_originalGamePath, @"winhttp.dll")),

                    // licenses
                    new FileInfo(Path.Combine(_originalGamePath, @"LICENSE-BEPINEX.txt")),
                    new FileInfo(Path.Combine(_originalGamePath, @"LICENSE-ConfigurationManager.txt")),                                    
                    new FileInfo(Path.Combine(_originalGamePath, @"LICENSE-Launcher.txt")),
                    new FileInfo(Path.Combine(_originalGamePath, @"LICENSE-Modules.txt")),
                    new FileInfo(Path.Combine(_originalGamePath, @"LICENSE-Server.txt"))
                };
                var directories = new DirectoryInfo[]
                {
                    new DirectoryInfo(Path.Combine(_originalGamePath, @"Aki_Data")),
                    new DirectoryInfo(Path.Combine(_originalGamePath, @"BepInEx"))
                };

                foreach (var file in files)
                {
                    if (File.Exists(file.FullName))
                    {
                        File.Delete(file.FullName);
                        isInstalledInLive = true;
                    }
                }

                foreach (var directory in directories)
                {
                    if (Directory.Exists(directory.FullName))
                    {
                        RemoveFilesRecurse(directory);
                        isInstalledInLive = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
            }

            return isInstalledInLive;
        }

        void SetupGameFiles()
        {
            var files = new []
            {
                GetFileForCleanup("BattlEye"),
                GetFileForCleanup("Logs"),
                GetFileForCleanup("ConsistencyInfo"),
                GetFileForCleanup("EscapeFromTarkov_BE.exe"),
                GetFileForCleanup("Uninstall.exe"),
                GetFileForCleanup("UnityCrashHandler64.exe"),
                GetFileForCleanup("WinPixEventRuntime.dll")
            };

            foreach (var file in files)
            {
                if (file == null)
                {
                    continue;
                }

                if (Directory.Exists(file))
                {
                    RemoveFilesRecurse(new DirectoryInfo(file));
                }

                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private string GetFileForCleanup(string fileName)
        {
            if (_excludeFromCleanup.Contains(fileName))
            {
                LogManager.Instance.Info($"Excluded {fileName} from file cleanup");
                return null;
            }
            
            return Path.Combine(_gamePath, fileName);
        }

        /// <summary>
        /// Remove the registry keys
        /// </summary>
        /// <returns>returns true if the keys were removed. returns false if an exception occured</returns>
		public bool RemoveRegistryKeys()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(registrySettings, true);

                foreach (var value in key.GetValueNames())
                {
                    key.DeleteValue(value);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clean the temp folder
        /// </summary>
        /// <returns>returns true if the temp folder was cleaned succefully or doesn't exist. returns false if something went wrong.</returns>
		public bool CleanTempFiles()
        {
            var rootdir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), @"Battlestate Games\EscapeFromTarkov"));

            if (!rootdir.Exists)
            {
                return true;
            }

            return RemoveFilesRecurse(rootdir);
        }

        bool RemoveFilesRecurse(DirectoryInfo basedir)
        {
            if (!basedir.Exists)
            {
                return true;
            }

            try
            {
                // remove subdirectories
                foreach (var dir in basedir.EnumerateDirectories())
                {
                    RemoveFilesRecurse(dir);
                }

                // remove files
                var files = basedir.GetFiles();

                foreach (var file in files)
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }

                // remove directory
                basedir.Delete();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                return false;
            }

            return true;
        }
    }
}
