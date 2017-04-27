using System;
using System.Collections.Generic;
using Xevle.Core.Collections;

namespace Xevle.IO
{
	public class PersistenceKeyValueStorage
	{
		public class PersistenceValue
		{
			public DateTime Timestamp
			{
				get;
				internal set;
			}

			public string Value
			{
				get;
				internal set;
			}

			public PersistenceValue(DateTime timestamp, String value)
			{
				Timestamp = timestamp;
				Value = value;
			}

		}

		Dictionary<String, List<PersistenceValue>> keyValueStorage;

		String filename;

		String filenameBackup;

		Object ioLock;

		public PersistenceKeyValueStorage(String filename)
		{
			this.filename = filename;
			this.filenameBackup = filename + "~";
			ioLock = new Object();
			Load();
		}

		public bool Add(String key, String value, bool uniqueKey = true)
		{
			return Add(key, value, uniqueKey, DateTime.Now);
		}

		public bool Add(String key, String value, bool uniqueKey, DateTime timestamp)
		{
			// Create key if not exists
			if (!keyValueStorage.ContainsKey(key))
			{
				keyValueStorage.Add(key, new List<PersistenceValue>());
			}

			// Get value from key
			List<PersistenceValue> persistenceValues = keyValueStorage [key];

			if (uniqueKey)
			{
				persistenceValues.Clear();
			}

			persistenceValues.Add(new PersistenceValue(timestamp, value));

			Save();

			return true;
		}

		public List<String> GetKeys()
		{
			List<String> ret = new List<String>();
			ret.AddRange(keyValueStorage.Keys);
			return ret;
		}

		/// <summary>
		/// Returns the newest value of an key.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="key">Key.</param>
		public PersistenceValue GetValue(String key)
		{
			List < PersistenceValue > values = keyValueStorage [key];

			PersistenceValue newestValue = null;

			foreach (PersistenceValue value in values)
			{
				if (newestValue == null)
				{
					newestValue = value;
					continue;
				}

				if (newestValue.Timestamp.Ticks < value.Timestamp.Ticks)
				{
					newestValue = value;
				}
			}

			return newestValue;
		}

		/// <summary>
		/// Returns all values of a key.
		/// </summary>
		/// <returns>The values.</returns>
		/// <param name="key">Key.</param>
		public List<PersistenceValue> GetValues(String key)
		{
			return keyValueStorage [key];
		}

		void Load()
		{
			lock (ioLock)
			{
				keyValueStorage = new Dictionary<string, List<PersistenceValue>>();

				// Use backup
				if (FileOperations.ExistsFile(filenameBackup))
				{
					FileOperations.RemoveFile(filename);
					FileOperations.MoveFile(filenameBackup, filename);
				}

				// Load storage file
				if (FileOperations.ExistsFile(filename))
				{
					Parameters pdlFile = Parameters.FromPDLFile(filename);

					foreach (String key in pdlFile.Keys)
					{
						List<Parameters> valuesParameters = pdlFile.GetParametersList(key);
						List<PersistenceValue> persistenceValues = new List<PersistenceValue>();

						foreach (Parameters parameters in valuesParameters)
						{
							DateTime timestamp = new DateTime(parameters.GetInt64("timestamp"));
							String value = parameters.GetString("value");

							persistenceValues.Add(new PersistenceValue(timestamp, value));
						}

						keyValueStorage.Add(key, persistenceValues);
					}
				}
			}
		}

		void Save()
		{
			lock (ioLock)
			{
				FileOperations.MoveFile(filename, filenameBackup);
				
				Parameters parameters = new Parameters();

				foreach (KeyValuePair<String, List<PersistenceValue>> entry in keyValueStorage)
				{
					List<Parameters> valuesParameters = new List<Parameters>();

					foreach (PersistenceValue persistenceValue in entry.Value)
					{
						Parameters valueParameters = new Parameters();
						valueParameters.Add("timestamp", persistenceValue.Timestamp.Ticks);
						valueParameters.Add("value", persistenceValue.Value);

						valuesParameters.Add(valueParameters);
					}

					parameters.Add(entry.Key, valuesParameters);
				}

				parameters.SaveToPDLFile(filename, true);

				FileOperations.RemoveFile(filename + filenameBackup);
			}
		}
	}
}
