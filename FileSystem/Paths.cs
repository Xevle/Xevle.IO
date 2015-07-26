using System;
using System.IO;

namespace Xevle.IO
{
	public static class Paths
	{
		#region Static variables
		/// <summary>
		/// The path delimiter of current system.
		/// </summary>
		static char pathDelimiter = System.IO.Path.DirectorySeparatorChar;

		static char[] illegalChars = new char[] { '<', '>', ':', '"', '|', '\0', '\x1', '\x2', '\x3', '\x4', '\x5', '\x6', '\x7', '\x8', '\x9', '\xA', '\xB', '\xC', '\xD', '\xE', '\xF', '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F', '\xfffd' };
		#endregion

		#region Standard and system paths
		/// <summary>
		/// Gets the application path.
		/// </summary>
		/// <value>The application path.</value>
		public static string ApplicationPath
		{
			get
			{
				FileInfo fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
				return fi.DirectoryName + pathDelimiter;
			}
		}

		/// <summary>
		/// Gets the application data path.
		/// </summary>
		/// <value>The application data path.</value>
		public static string ApplicationDataPath
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).TrimEnd(pathDelimiter) + pathDelimiter;
			}
		}

		/// <summary>
		/// Gets the application filename.
		/// </summary>
		/// <value>The application filename.</value>
		public static string ApplicationFilename
		{
			get
			{
				FileInfo fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
				return fi.FullName;
			}
		}

		/// <summary>
		/// Gets the temp path of current system.
		/// </summary>
		/// <value>The temp path.</value>
		public static string TempPath
		{
			get
			{
				return Path.GetTempPath().TrimEnd(pathDelimiter) + pathDelimiter;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Determines if is path the specified filename.
		/// </summary>
		/// <returns><c>true</c> if is path the specified filename; otherwise, <c>false</c>.</returns>
		/// <param name="filename">Filename.</param>
		public static bool IsPath(string filename)
		{
			char[] trimchars = new char[2];
			trimchars[0] = '/';
			trimchars[1] = '\\';

			filename = filename.TrimEnd(trimchars);

			if (filename.IndexOf('\\') != -1 || filename.IndexOf('/') != -1) return true;
			else return false;
		}

		/// <summary>
		/// Determines if is absolute the specified path.
		/// </summary>
		/// <returns><c>true</c> if is absolute the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		public static bool IsAbsolute(string path)
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
					{
						if (path.Length < 3) return false;	// no D:\ or so

						if (!Char.IsLetter(path[0]))
						{	//Test auf UNC Pfad
							if (path[0] != '\\') return false;	// network path
							if (path[1] != '\\') return false;
							return true;
						}

						if (path[1] != ':') return false;
						if (path[2] != '\\' && path[2] != '/') return false;
						return true;
					}
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					{
						if (path.Length > 0 && path[0] == '/') return true;
						else return false;
					}
				default:
					{
						throw new NotImplementedException();
					}
			}
		}

		/// <summary>
		/// Determines if is local path the specified path.
		/// </summary>
		/// <returns><c>true</c> if is local path the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		public static bool IsLocalPath(string path)
		{
			if (IsNetworkPath(path)) return false;
			if (IsAbsolute(path)) return true;

			if (path.Length < 1) return false;

			if (path.IndexOfAny(illegalChars) != -1) return false;
			return true;
		}

		/// <summary>
		/// Determines if is network path the specified path.
		/// </summary>
		/// <returns><c>true</c> if is network path the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		public static bool IsNetworkPath(string path)
		{
			if (path.Length < 3) return false;	// kein "c:\" oder �hnliches
			if (!Char.IsLetter(path[0]))
			{	//Test auf UNC Pfad
				if (path[0] != '\\') return false;	// z.B. "\\FOO\myMusic"
				if (path[1] != '\\') return false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines if is root the specified path.
		/// </summary>
		/// <returns><c>true</c> if is root the specified path; otherwise, <c>false</c>.</returns>
		/// <param name="path">Path.</param>
		public static bool IsRoot(string path)
		{
			path = path.TrimEnd(pathDelimiter).ToLower();

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
					{
						if (path.Length == 2 && path[0] >= 'a' && path[0] <= 'z' && path[1] == ':') return true;
						break;
					}
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					{
						if (path.Length > 0 && path[0] == '/') return true;
						break;
					}
				default:
					{
						throw new NotImplementedException();
					}
			}

			return false;
		}

		/// <summary>
		/// Determines if is filename valid the specified filename.
		/// </summary>
		/// <returns><c>true</c> if is filename valid the specified filename; otherwise, <c>false</c>.</returns>
		/// <param name="filename">Filename.</param>
		public static bool IsFilenameValid(string filename)
		{
			if (filename.IndexOfAny(illegalChars) != -1) return false;
			return true;
		}
		#endregion

		#region Path operations
		/// <summary>
		/// Gets the path.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="filename">Filename.</param>
		/// <param name="stringMethod">If set to <c>true</c> string method.</param>
		/// <param name="pathDelimiterAtEnd">If set to <c>true</c> path delimiter at end.</param>
		public static string GetPath(string filename, bool stringMethod=false, bool pathDelimiterAtEnd=true)
		{
			if (stringMethod)
			{
				if (filename[filename.Length - 1] == pathDelimiter) return filename;

				int idx = -1;

				for (int i = filename.Length - 1; i >= 0; i--)
				{
					if (filename[i] == '\\' || filename[i] == '/')
					{
						idx = i; 
						break;
					}
				}

				if (idx == -1) return "";
				string path = filename.Substring(0, idx);

				if (pathDelimiterAtEnd) return path + pathDelimiter;
				else return path;
			}
			else
			{
				FileInfo ret = new FileInfo(filename);
				if (ret.DirectoryName.Length == 3) return ret.DirectoryName;

				if (pathDelimiterAtEnd) return ret.DirectoryName + pathDelimiter;
				else return ret.DirectoryName;
			}
		}

		/// <summary>
		/// Gets the absolute path.
		/// </summary>
		/// <returns>The absolute path.</returns>
		/// <param name="path">Path.</param>
		public static string GetAbsolutePath(string path)
		{
			FileInfo ret = new FileInfo(path);
			return ret.FullName;
		}

		/// <summary>
		/// Gets the relative path.
		/// </summary>
		/// <returns>The relative path.</returns>
		/// <param name="completePath">Complete path.</param>
		/// <param name="basePath">Base path.</param>
		/// <param name="caseSensitive">If set to <c>true</c> case sensitive.</param>
		public static string GetRelativePath(string completePath, string basePath, bool caseSensitive=false)
		{
			if (!IsAbsolute(completePath) && !IsNetworkPath(completePath)) throw new ArgumentException("completePath is not absolute.");

			if (!IsAbsolute(basePath) && !IsNetworkPath(basePath)) throw new ArgumentException("basePath is not absolute.");

			if (!caseSensitive)
			{
				completePath = completePath.ToLower();
				basePath = basePath.ToLower();
			}

			completePath = GetAbsolutePath(completePath);
			basePath = GetAbsolutePath(basePath);

			// Canonize path
			if (basePath[basePath.Length - 1] != pathDelimiter) basePath += pathDelimiter;

			string relativePath = "";

			int baseStart = completePath.IndexOf(basePath);

			while (baseStart != 0 && basePath.Length > 3)
			{
				basePath = GetPath(basePath.Substring(0, basePath.Length - 1));

				if (basePath[basePath.Length - 1] != pathDelimiter) basePath += pathDelimiter;

				relativePath += ".." + pathDelimiter;
				baseStart = completePath.IndexOf(basePath);
			}

			if (baseStart == 0) relativePath += completePath.Substring(basePath.Length);
			else relativePath = completePath;

			return relativePath;
		}

		/// <summary>
		/// Gets the path with path delimiter.
		/// </summary>
		/// <returns>The path with path delimiter.</returns>
		/// <param name="path">Path.</param>
		public static string GetPathWithPathDelimiter(string path)
		{
			return path.TrimEnd(pathDelimiter) + pathDelimiter;
		}

		/// <summary>
		/// Gets the path filename without extension.
		/// </summary>
		/// <returns>The path filename without extension.</returns>
		/// <param name="filename">Filename.</param>
		public static string GetPathFilenameWithoutExtension(string filename)
		{
			return GetPath(filename) + GetFilenameWithoutExtension(filename);
		}

		/// <summary>
		/// Gets the filename.
		/// </summary>
		/// <returns>The filename.</returns>
		/// <param name="filename">Filename.</param>
		/// <param name="stringMethod">If set to <c>true</c> string method.</param>
		public static string GetFilename(string filename, bool stringMethod=false)
		{
			if (stringMethod)
			{
				if (filename[filename.Length - 1] == pathDelimiter) return filename;

				int idx = -1;

				for (int i = filename.Length - 1; i >= 0; i--)
				{
					if (filename[i] == '\\' || filename[i] == '/')
					{
						idx = i;
						break;
					}
				}

				if (idx == -1) return filename;
				idx++;

				return filename.Substring(idx, filename.Length - idx);
			}
			else
			{
				FileInfo ret = new FileInfo(filename);
				return ret.Name;
			}
		}

		/// <summary>
		/// Gets the filename without extension.
		/// </summary>
		/// <returns>The filename without extension.</returns>
		/// <param name="filename">Filename.</param>
		public static string GetFilenameWithoutExtension(string filename)
		{
			FileInfo ret = new FileInfo(filename);
			return ret.Name.Substring(0, ret.Name.Length - ret.Extension.Length);
		}

		/// <summary>
		/// Gets the extension.
		/// </summary>
		/// <returns>The extension.</returns>
		/// <param name="filename">Filename.</param>
		public static string GetExtension(string filename)
		{
			FileInfo ret = new FileInfo(filename);
			return ret.Extension.TrimStart('.');
		}

		/// <summary>
		/// Gets the filename with another extension.
		/// </summary>
		/// <returns>The filename with another extension.</returns>
		/// <param name="filename">Filename.</param>
		/// <param name="newExtension">New extension.</param>
		public static string GetFilenameWithAnotherExtension(string filename, string newExtension)
		{
			string fnWithoutExt = GetFilenameWithoutExtension(filename);
			newExtension = newExtension.Replace(".", "");
			return fnWithoutExt + "." + newExtension;
		}

		/// <summary>
		/// Gets a valid filename.
		/// </summary>
		/// <returns>The valid filename.</returns>
		/// <param name="filename">Filename.</param>
		public static string GetValidFilename(string filename)
		{
			int ind;
			while ((ind = filename.IndexOfAny(illegalChars)) != -1) filename = filename.Remove(ind, 1);
			return filename;
		}
		#endregion
	}
}

