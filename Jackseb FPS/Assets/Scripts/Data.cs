using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Com.Jackseb.FPS
{
	public class Data : MonoBehaviour
	{
		public static void SaveProfile(ProfileData p_profile)
		{
			try
			{
				string path = Application.persistentDataPath + "/profile.dt";

				if (File.Exists(path)) File.Delete(path);

				FileStream fs = File.Create(path);

				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, p_profile);
				fs.Close();
			}
			catch
			{
				Debug.Log("SOMETHING WENT TERRIBLY WRONG");
			}
		}

		public static ProfileData LoadProfile()
		{
			ProfileData ret = new ProfileData();

			try
			{
				string path = Application.persistentDataPath + "/profile.dt";

				if (File.Exists(path))
				{
					FileStream fs = File.Open(path, FileMode.Open);
					BinaryFormatter bf = new BinaryFormatter();
					ret = (ProfileData)bf.Deserialize(fs);
				}
			}
			catch
			{
				Debug.Log("FILE WASN'T FOUND");
			}

			return ret;
		}
	}
}