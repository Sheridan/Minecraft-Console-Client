using System;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;  

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/// <summary>
		/// Saving and reading options for current bot
		/// This code serialize structures to JSON and can deserialize it later
		/// 
		/// When developing robot you have to fill in once the structure and save it. 
		/// Then it is already possible to correct the values directly in the file
		/// 
		/// Example use:
		/// public struct Opts
		/// {
		/// 	string Name;
		/// 	Location location;
		/// }
		/// ...
		/// Opts o = new Opts();
		/// o.Name = "Me";
		/// o.location = new Location(1,2,3);
		/// SaveSettings(o);
		/// ...
		/// Opts r = ReadSettings<Opts>();
		/// </summary>

		/// <summary>
		/// Serialize structure to file
		/// </summary>
		/// <param name="obj">Structure to serialize</param>

		protected void SaveSettings(object obj)
		{
			try
			{
				FileStream stream = new FileStream(GetType().Name + "_settings.json", FileMode.OpenOrCreate);
				DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
				ser.WriteObject (stream, obj);
				stream.Close ();
			}
			catch (Exception e) 
			{
				LogToConsole ("Error while write options: " + e.Message);
			}
		}

		/// <summary>
		/// Deserialize structure from file.
		/// </summary>
		/// <param name="T">Structure type to deserialize</param>
		/// <returns>Filled or empty structure</returns>

		protected T ReadSettings<T>() where T : new()
		{
			T obj = (T)Activator.CreateInstance(typeof(T));
			try
			{
				DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
				string fileContent=File.ReadAllText(GetType().Name + "_settings.json");
				obj = (T)json.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(fileContent))); 
			}
			catch (Exception e) 
			{
				LogToConsole ("Error while loading options: " + e.Message);
			}
			return obj;
		}

	}
}

