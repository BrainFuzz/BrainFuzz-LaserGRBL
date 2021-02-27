//Copyright (c) 2016-2021 Diego Settimi - https://github.com/arkypita/

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GPLv3 General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. using System;

namespace LaserGRBL {
  /// <summary>
  /// Description of Settings.
  /// </summary>
  public static class Settings {
	private static System.Collections.Generic.Dictionary<string, object> dictionary;

	static string Filename {
	  get {
		string basename = "LaserGRBL.Settings.bin";
		string fullname = System.IO.Path.Combine(GrblCore.DataPath, basename);

		if (!System.IO.File.Exists(fullname) && System.IO.File.Exists(basename))
		  System.IO.File.Copy(basename, fullname);

		return fullname;
	  }
	}


	static Settings() {
	  try {
		if (System.IO.File.Exists(Filename)) {
          System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter {
            AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
          };

          using (System.IO.FileStream fileStream = new System.IO.FileStream(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None)) {
			dictionary = (System.Collections.Generic.Dictionary<string, object>) binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
		  }
		}

	  } catch { }

	  if (dictionary == null)
		dictionary = new System.Collections.Generic.Dictionary<string, object>();
	}


	public static T GetObject<T>(string key, T defval) {
	  try {
		if (dictionary.ContainsKey(key)) {
		  object obj = dictionary[key];
		  if (obj != null && obj.GetType() == typeof(T))
			return (T) obj;
		}
	  } catch {
	  }
	  return defval;
	}


	public static object GetAndDeleteObject(string key, object defval) {
	  object rv = dictionary.ContainsKey(key) && dictionary[key] != null ? dictionary[key] : defval;
	  DeleteObject(key);
	  return rv;
	}


	public static void SetObject(string key, object value) {
	  if (dictionary.ContainsKey(key))
		dictionary[key] = value;
	  else
		dictionary.Add(key, value);
	}


	public static void Save() {
	  try {
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter {
          AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        };

        using (System.IO.FileStream fileStream = new System.IO.FileStream(Filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None)) {
		  binaryFormatter.Serialize(fileStream, dictionary);
		  fileStream.Close();
		}
	  } catch { }
	}

	internal static void DeleteObject(string key) {
	  if (dictionary.ContainsKey(key))
		dictionary.Remove(key);
	}

	internal static bool ExistObject(string key) {
	  return dictionary.ContainsKey(key);
	}
  }
}