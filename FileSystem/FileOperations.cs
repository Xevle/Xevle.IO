using System;
using System.IO;
using System.Collections.Generic;

namespace Xevle.IO
{
	public static class FileOperations
	{
		#region Read only operations
		/// <summary>
		/// Existses the file.
		/// </summary>
		/// <returns><c>true</c>, if file was existsed, <c>false</c> otherwise.</returns>
		/// <param name="filename">Filename.</param>
		public static bool ExistsFile(string filename)
		{
			return System.IO.File.Exists(filename);
		}

		/// <summary>
		/// Determines if is file the specified filename.
		/// </summary>
		/// <returns><c>true</c> if is file the specified filename; otherwise, <c>false</c>.</returns>
		/// <param name="filename">Filename.</param>
		public static bool IsFile(string filename)
		{
			try
			{
				FileAttributes attr = File.GetAttributes(filename);
				return (attr & FileAttributes.Directory) == 0;
			}
			catch (Exception)
			{
			}

			return false;
		}

		/// <summary>
		/// Determines if is file read only the specified filename.
		/// </summary>
		/// <returns><c>true</c> if is file read only the specified filename; otherwise, <c>false</c>.</returns>
		/// <param name="filename">Filename.</param>
		public static bool IsFileReadOnly(string filename)
		{
			try
			{
				FileAttributes attr = File.GetAttributes(filename);
				return (attr & FileAttributes.ReadOnly) != 0;
			}
			catch (Exception)
			{
			}
			return false;
		}

		/// <summary>
		/// Gets the filesize.
		/// </summary>
		/// <returns>The filesize.</returns>
		/// <param name="filename">Filename.</param>
		public static long GetFilesize(string filename)
		{
			if (!IsFile(filename)) throw new FileNotFoundException("", filename);
			FileInfo fi = new FileInfo(filename);
			return fi.Length;
		}

		/// <summary>
		/// Gets the file date time.
		/// </summary>
		/// <returns>The file date time.</returns>
		/// <param name="filename">Filename.</param>
		public static DateTime GetFileDateTime(string filename)
		{
			if (!IsFile(filename)) throw new FileNotFoundException("", filename);
			FileInfo fi = new FileInfo(filename);
			return fi.LastWriteTime;
		}

		/// <summary>
		/// Gets the files.
		/// </summary>
		/// <returns>The files.</returns>
		/// <param name="path">Path.</param>
		/// <param name="recursive">If set to <c>true</c> recursive.</param>
		/// <param name="filter">Filter.</param>
		public static List<string> GetFiles(string path, bool recursive = true, string filter = "*")
		{
			try
			{
				List<string> ret = new List<string>();

				string[] subdirs = Directory.GetDirectories(path);
				string[] files = Directory.GetFiles(path, filter);

				ret.AddRange(files);

				if (recursive)
				{
					foreach (string dir in subdirs)
					{
						List<string> subfiles = GetFiles(dir, recursive, filter);
						ret.AddRange(subfiles);
					}
				}

				return ret;
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion

		#region Operations
		/// <summary>
		/// Removes the file.
		/// </summary>
		/// <returns><c>true</c>, if file was removed, <c>false</c> otherwise.</returns>
		/// <param name="filename">Filename.</param>
		public static bool RemoveFile(string filename)
		{
			try
			{
				File.Delete(filename);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Removes the files.
		/// </summary>
		/// <returns><c>true</c>, if files was removed, <c>false</c> otherwise.</returns>
		/// <param name="filenames">Filenames.</param>
		public static bool RemoveFiles(List<string> filenames)
		{
			bool ret = true;

			foreach (string i in filenames)
			{
				if (RemoveFile(i) == false) ret = false;
			}

			return ret;
		}

		/// <summary>
		/// Copies the file.
		/// </summary>
		/// <returns><c>true</c>, if file was copyed, <c>false</c> otherwise.</returns>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="overwrite">If set to <c>true</c> overwrite.</param>
		public static bool CopyFile(string source, string destination, bool overwrite = false)
		{
			try
			{
				File.Copy(source, destination, overwrite);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Copies the files.
		/// </summary>
		/// <returns><c>true</c>, if files was copyed, <c>false</c> otherwise.</returns>
		/// <param name="source">Source.</param>
		/// <param name="target">Target.</param>
		/// <param name="filter">Filter.</param>
		/// <param name="ExcludeFiles">Exclude files.</param>
		/// <param name="throwException">If set to <c>true</c> throw exception.</param>
		public static bool CopyFiles(string source, string target, string filter = "*", List<string> ExcludeFiles = null, bool throwException = false)
		{
			List<string> files = GetFiles(source, false, filter);

			foreach (string file in files)
			{
				string filename = Paths.GetFilename(file);

				try
				{
					bool breakOut = false;

					foreach (string excludedFile in ExcludeFiles)
					{
						if (filename == excludedFile)
						{
							breakOut = true;
							continue;
						}
					}

					if (breakOut) continue;

					File.Copy(file, target + filename);
				}
				catch (Exception e)
				{
					if (throwException) throw e;
					else return false;
				}
			}

			return true;
		}

		public static bool MoveFile(string source, string destination)
		{
			try
			{
				File.Move(source, destination);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
		#endregion
	}
}

