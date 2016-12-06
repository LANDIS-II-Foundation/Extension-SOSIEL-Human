using System;
using System.Diagnostics;

namespace Demo
{
    class IISAgent
    {
        Process process;

        public void Start(string arguments)
        {
            Environment.SpecialFolder programFiles = Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles;
            string programFilesPath = Environment.GetFolderPath(programFiles);

            ProcessStartInfo info = new ProcessStartInfo($@"{programFilesPath}\IIS Express\iisexpress.exe", arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden
                //WindowStyle= ProcessWindowStyle.Minimized
            };

            process = Process.Start(info);
        }

        public void Stop()
        {
            if (process?.HasExited == false)
            {
                process.Kill();
            }
        }

    }
}
