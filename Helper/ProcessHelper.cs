using System;
using System.Diagnostics;

namespace Xevle.IO.Helper
{
	public static class ProcessHelper
	{
		public static bool StartProcess(string filename, string arguments = "", bool waitForExit = false)
		{
			try
			{
				Process process = new Process();

				process.EnableRaisingEvents = false;
				process.StartInfo.FileName = filename;
				process.StartInfo.Arguments = arguments;

				process.Start();
				if (waitForExit) process.WaitForExit();
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}