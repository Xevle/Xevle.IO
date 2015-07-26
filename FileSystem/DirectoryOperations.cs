using System;
using System.IO;
using System.Collections.Generic;

namespace Xevle.IO
{
	public static class DirectoryOperations
	{
		#region Static variables
		/// <summary>
		/// The path delimiter of current system.
		/// </summary>
		static char pathDelimiter = System.IO.Path.DirectorySeparatorChar;
		#endregion

		#region Read only operations
		/// <summary>
		/// Tests if the specified path exists.
		/// </summary>
		/// <param name="path">Path of directory.</param>
		public static bool Exists(string path)
		{
			return System.IO.Directory.Exists(path);
		}

		/// <summary>
		/// Gets the sub directories of path.
		/// </summary>
		/// <returns>The directories.</returns>
		/// <param name="path">Path.</param>
		/// <param name="filter">Filter.</param>
		public static List<string> GetDirectories(string path, string filter="*")
		{
			try
			{
				List<string> ret = new List<string>();
				ret.AddRange(Directory.GetDirectories(path, filter));
				return ret;
			}
			catch (Exception)
			{
				return new List<string>();
			}
		}
		#endregion

		#region Operations
		/// <summary>
		/// Creates the directory.
		/// </summary>
		/// <returns><c>true</c>, if directory was created, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="throwException">If set to <c>true</c> throw exception.</param>
		public static bool CreateDirectory(string path, bool throwException=false)
		{
			bool ret = true;

			char[] sep = new char[] { pathDelimiter };
			string[] pathParts = path.Split(sep, StringSplitOptions.RemoveEmptyEntries);

			string currentPath = "";

			foreach (string i in pathParts)
			{
				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Win32NT:
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
						{
							currentPath += i + pathDelimiter;
							if (Paths.IsRoot(i)) continue;
							break;
						}
					case PlatformID.MacOSX:
					case PlatformID.Unix:
						{
							if (path.Length > 0 && path[0] == '/') // root
							{
								if (currentPath.Length > 0)
								{
									currentPath += i + pathDelimiter;
								}
								else
								{
									currentPath += pathDelimiter + i + pathDelimiter;
								}
							}
							else
							{
								currentPath += i + pathDelimiter; // relative
							}
							break;
						}
					default:
						{
							throw new NotImplementedException();
						}
				}

				if (!Directory.Exists(currentPath))
				{
					try
					{
						Directory.CreateDirectory(currentPath);
					}
					catch (Exception e)
					{
						if (throwException) throw e;
						return false;
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Removes the directory.
		/// </summary>
		/// <returns><c>true</c>, if directory was removed, <c>false</c> otherwise.</returns>
		/// <param name="path">Path.</param>
		/// <param name="recursive">If set to <c>true</c> recursive.</param>
		/// <param name="throwException">If set to <c>true</c> throw exception.</param>
		public static bool RemoveDirectory(string path, bool recursive=true, bool throwException=false)
		{
			try
			{ 
				Directory.Delete(path, recursive);
			}
			catch (Exception e)
			{
				if (throwException) throw e;
				else return false;
			}

			return true;
		}

		public static bool CopyDirectory(string source, string destination, bool recursiv=true, List<string> excludedFolders=null, List<string> excludedFiles=null, bool ignoreExistingFiles=false)
		{
			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(source);

			bool result = true;
			if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

			foreach (FileInfo fi in sourceDirectoryInfo.GetFiles())
			{
				string destFile = destination + pathDelimiter + fi.Name;

				bool breakOut = false;

				foreach (string i in excludedFiles)
				{
					if (fi.Name == i)
					{
						breakOut = true;
						continue;
					}
				}

				if (breakOut) continue;

				if (ignoreExistingFiles)
				{
					if (!File.Exists(destFile))
					{
						result = result && (fi.CopyTo(destFile, false) != null);
					}
				}
				else
				{
					result = result && (fi.CopyTo(destFile, false) != null);
				}
			}

			if (recursiv)
			{
				foreach (DirectoryInfo subDirectory in sourceDirectoryInfo.GetDirectories())
				{
					bool make = true;

					foreach (string i in excludedFolders)
					{
						if (subDirectory.Name == i)
						{
							make = false;
							break;
						}
					}

					if (make)
					{
						string destTmp = subDirectory.FullName.Replace(sourceDirectoryInfo.FullName, destination);
						result = result && CopyDirectory(subDirectory.FullName, destTmp, true, excludedFolders, excludedFiles, ignoreExistingFiles);
					}
				}
			}

			return result;
		}
		/// <summary>
		/// Moves the directory.
		/// </summary>
		/// <returns><c>true</c>, if directory was moved, <c>false</c> otherwise.</returns>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public static bool MoveDirectory(string source, string destination)
		{
			if (CopyDirectory(source, destination) && RemoveDirectory(source, true)) return true;
			else return false;
		}
		#endregion

		#region Special functions
		/// <summary>
		/// Gets the current work directory.
		/// </summary>
		/// <returns>The current work directory.</returns>
		public static string GetCurrentWorkDirectory()
		{
			return Environment.CurrentDirectory.TrimEnd(pathDelimiter) + pathDelimiter;
		}
			
		/// <summary>
		/// Sets the current work directory.
		/// </summary>
		/// <returns><c>true</c>, if current work directory was set, <c>false</c> otherwise.</returns>
		/// <param name="directory">Directory.</param>
		public static bool SetCurrentWorkDirectory(string directory)
		{
			Environment.CurrentDirectory = directory.TrimEnd(pathDelimiter) + pathDelimiter;
			return GetCurrentWorkDirectory() == directory;
		}
		#endregion
	}
}

