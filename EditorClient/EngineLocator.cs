using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace EditorClient
{
	class EngineLocator
	{
		public Instance GetInstance(string path)
		{
			var instances = getInstances(path);
			return instances.Where(x => isInstance(x.Key, path) && canConnectTo(x))
				.OrderByDescending(x => x.Key.Length)
				.FirstOrDefault();
		}

		private bool isInstance(string key, string path)
		{
            var osPath = "";
            var osKey = "";
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX) {
                osKey = key;
                osPath = path;
			} else {
                osKey = key.ToLower();
                osPath = path.ToLower();
            }
            return osKey == osPath || osPath.StartsWith(osKey+Path.DirectorySeparatorChar.ToString());
		}
		
		private IEnumerable<Instance> getInstances(string path)
		{
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Replace(Path.DirectorySeparatorChar.ToString(), "-");
            var filepattern = string.Format("*.EditorEngine.{0}.pid", user);
            foreach (var file in Directory.GetFiles(FS.GetTempDir(), filepattern))
            {
                var instance = Instance.Get(file, File.ReadAllLines(file));
                if (instance != null)
                    yield return instance;
            }
		}
		
		private bool canConnectTo(Instance info)
		{
			var client = new Client();
			client.Connect(info.Port, (s) => {});
			var connected = client.IsConnected;
			client.Disconnect();
			if (!connected)
				File.Delete(info.File);
			return connected;
		}
	}

	class FS
	{
		public static string GetTempFilePath()
		{
			var tmpfile = Path.GetTempFileName();
			if (OS.IsOSX)
				tmpfile = Path.Combine("/tmp", Path.GetFileName(tmpfile));
			return tmpfile;
		}

		public static string GetTempDir()
		{
			if (OS.IsOSX)
				return "/tmp";
			return Path.GetTempPath();
		}
	}

	static class OS
    {
        private static bool? _isWindows;
        private static bool? _isUnix;
        private static bool? _isOSX;

        public static bool IsWindows {
            get {
                if (_isWindows == null) {
                    _isWindows = 
                        Environment.OSVersion.Platform == PlatformID.Win32S ||
                        Environment.OSVersion.Platform == PlatformID.Win32NT ||
                        Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                        Environment.OSVersion.Platform == PlatformID.WinCE ||
                        Environment.OSVersion.Platform == PlatformID.Xbox;
                }
                return (bool) _isWindows;
            }
        }

        public static bool IsPosix {
            get {
                return IsUnix || IsOSX;
            }
        }

        public static bool IsUnix {
            get {
                if (_isUnix == null)
                    setUnixAndLinux();
                return (bool) _isUnix;
            }
        }

        public static bool IsOSX {
            get {
                if (_isOSX == null)
                    setUnixAndLinux();
                return (bool) _isOSX;
            }
        }

        private static void setUnixAndLinux()
        {
            try
            {
                if (IsWindows) {
                    _isOSX = false;
                    _isUnix = false;
                } else  {
                    var process = new Process
                                      {
                                          StartInfo =
                                              new ProcessStartInfo("uname", "-a")
                                                  {
                                                      RedirectStandardOutput = true,
                                                      WindowStyle = ProcessWindowStyle.Hidden,
                                                      UseShellExecute = false,
                                                      CreateNoWindow = true
                                                  }
                                      };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    _isOSX = output.Contains("Darwin Kernel");
                    _isUnix = !_isOSX;
                }
            }
            catch
            {
                _isOSX = false;
                _isUnix = false;
            }
        }
    }
}

