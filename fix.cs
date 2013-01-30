using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class Program
{
	public static void Main()
	{
		try
		{
			if(!File.Exists("config.xml"))
			{
				Console.WriteLine("No config.xml found. Writing default config to config.xml");
				WriteDefaultConfig();
			}
			else
			{
				try
				{
					Config config=ReadConfig();
					Process(config);
				}
				catch(ConfigError ex)
				{
					Console.WriteLine("Configuration error: check your config.xml file: "+ex.Message);
				}
			}
		}
		catch(Exception ex)
		{
			Console.WriteLine("ERROR: uncaught exception: {0}", ex);
		}
		Console.WriteLine("Press any key...");
		Console.ReadKey();
	}

	private static Config ReadConfig()
	{
		using(Stream file=File.Open("config.xml", FileMode.Open))
		{
			XmlSerializer xsr=new XmlSerializer(typeof(Config));
			Config config=(Config)xsr.Deserialize(file);
			config.Init();
			return config;
		}
	}

	public static void Process(Config config)
	{
		if(!Directory.Exists(config.output_folder))
		{
			Console.WriteLine("creating directory {0}", config.output_folder);
			Directory.CreateDirectory(config.output_folder);
		}

		foreach(String filename in config.files_to_fix)
		{
			Console.WriteLine("processing {0}", filename);

			XmlDocument doc=new XmlDocument();
			doc.PreserveWhitespace=true;
			doc.Load(filename);

			ResetCountsByTag(config.TagsToFix());

			foreach(XmlNode tag in doc.SelectNodes("//*"))
			{
				//do we have to treat that tag (according to config)?
				if(config.MustTreatTag(tag.Name))
				{
					//do we modify the name attribute or delete it?
					if(config.MustDeleteNameOfTag(tag.Name))
					{
						DeleteNameAttribute(tag);
					}
					else
					{
						ModifyNameAttribute(tag, config.GetNamePrefix(tag.Name));
					}
				}
			}

			string output = Path.Combine(config.output_folder, Path.GetFileName(filename));
			Console.WriteLine("saving file {0}", output);
			doc.Save(output);
		}
	}

	private static void WriteDefaultConfig()
	{
		XmlWriterSettings settings=new XmlWriterSettings();
		settings.Indent=true;
		using(Stream file=File.Open("config.xml", FileMode.Create))
		{
			using(XmlWriter writer=XmlWriter.Create(file, settings))
			{
				new XmlSerializer(typeof(Config)).Serialize(writer, new Config());
			}
		}
	}

	private static void DeleteNameAttribute(XmlNode tag)
	{
		//delete the nameAttribute if present
		XmlAttribute nameAttribute=tag.Attributes["name"];
		if(nameAttribute!=null)
		{
			//there is a nameAttribute, so delete it
			tag.Attributes.Remove(nameAttribute);
		}
		else
		{
			//there's no nameAttribute to remove
			Console.WriteLine("WARNING: tag {0} has no name attribute to delete", tag.Name);
		}
	}

	private static void ModifyNameAttribute(XmlNode tag, string namePrefix)
	{
		//is there a name attribute?
		if(tag.Attributes["name"]!=null)
		{
			//yes: increment tag count and modify name attribute
			_countsByTag[tag.Name]++;
			tag.Attributes["name"].Value=namePrefix+_countsByTag[tag.Name];
		}
		else
		{
			//there's no name attribute to modify
			Console.WriteLine("WARNING: tag {0} has no name attribute to modify", tag.Name);
		}
	}

	private static Dictionary<string, int> _countsByTag;
	private static void ResetCountsByTag(IEnumerable<string> tagsToFix)
	{
		_countsByTag=new Dictionary<string, int>();
		foreach(string tagName in tagsToFix)
		{
			_countsByTag[tagName]=0;
		}
	}

}

public class Config
{
	[XmlArrayItem("filename")]
	public string[] files_to_fix=new string[]{
		"example.xml",
	};

	[XmlArrayItem("replace")]
	public Replacement[] replacements=new Replacement[]{
		new Replacement("crosstab","Tb"),
		new Replacement("list","Lst"),
	};

	[XmlArrayItem("delete")]
	public string[] deletions=new string[]{
		"table",
		"image",
		"textItem",
	};

	public string output_folder="output";

	private Dictionary<string, string> _mappings;
	public void Init()
	{
		//prepare a hash table whose keys are the tag names to delete/modify
		//and values are the prefix of the new name attribute
		//if value is null: the name attribute must be deleted
		_mappings=new Dictionary<string, string>();
		foreach(Replacement replacement in replacements)
		{
			if(String.IsNullOrEmpty(replacement.TagName) || String.IsNullOrEmpty(replacement.NamePrefix))
			{
				throw new ConfigError("replace: tag or prefix cannot be null or empty");
			}
			if(_mappings.ContainsKey(replacement.TagName))
			{
				throw new ConfigError("duplicate tag to treat :'"+replacement.TagName+"'");
			}
			_mappings[replacement.TagName]=replacement.NamePrefix;
		}
		foreach(string tagToDelete in deletions)
		{
			if(String.IsNullOrEmpty(tagToDelete))
				throw new ConfigError("delete: cannot be empty");
			if(_mappings.ContainsKey(tagToDelete))
			{
				throw new ConfigError("duplicate tag to treat :'"+tagToDelete+"'");
			}
			_mappings[tagToDelete]=null;
		}
	}

	public bool MustTreatTag(string tagName)
	{
		return _mappings.ContainsKey(tagName);
	}

	public bool MustDeleteNameOfTag(string tagName)
	{
		return String.IsNullOrEmpty(_mappings[tagName]);
	}

	public string GetNamePrefix(string tagName)
	{
		return _mappings[tagName];
	}

	public IEnumerable<string> TagsToFix()
	{
		return _mappings.Keys;
	}
}

public class Replacement
{
	[XmlAttribute("tag")]
	public string TagName=String.Empty;

	[XmlAttribute("prefix")]
	public string NamePrefix=null;

	public Replacement(){}
	public Replacement(string tagName, string namePrefix)
	{
		TagName=tagName;
		NamePrefix=namePrefix;
	}
}

class ConfigError: Exception
{
	public ConfigError(string message):base(message){}
}
