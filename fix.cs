using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class Config
{
	public string[] Files=new string[]{
		"example.xml",
	};
	public string OutputFolder="output";
	public bool AddMissingNameAttribute=false;
	public int StartCounterValue = 1;
	public List<string> Tags=new List<string>(new string[]{
		"crosstab",
		"image",
		"text",
		"table",
	});
}

public class Program
{
	public static void Main()
	{
		try
		{
			XmlSerializer xsr=new XmlSerializer(typeof(Config));
			Config config=new Config();
			if(File.Exists("config.xml"))
			{
				Console.WriteLine("Read config from config.xml");
				using(Stream file=File.Open("config.xml", FileMode.Open))
				{
					config=(Config)xsr.Deserialize(file);
				}
				Process(config);
				Console.WriteLine("done");
			}
			else
			{
				Console.WriteLine("No config found. Writing example to config.xml");
				XmlWriterSettings settings=new XmlWriterSettings();
				settings.Indent=true;
				using(Stream file=File.Open("config.xml", FileMode.Create))
				{
					using(XmlWriter writer=XmlWriter.Create(file, settings))
					{
						xsr.Serialize(writer, config);
					}
				}
			}
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex);
		}
		Console.WriteLine("Press any key...");
		Console.ReadKey();
	}

	public static void Process(Config config)
	{
		if(!Directory.Exists(config.OutputFolder))
		{
			Console.WriteLine("creating directory {0}", config.OutputFolder);
			Directory.CreateDirectory(config.OutputFolder);
		}
		foreach(String filename in config.Files)
		{
			Console.WriteLine("processing {0}", filename);
			XmlDocument doc=new XmlDocument();
			doc.Load(filename);
			Dictionary<string, int> countsByTag=new Dictionary<string, int>();
			XmlNodeList nodes=doc.SelectNodes("//*");
			foreach(XmlNode node in nodes)
			{
				if(config.Tags.Contains(node.Name))
				{
					XmlAttribute name=node.Attributes["name"];
					if(config.AddMissingNameAttribute && name==null)
					{
						name=doc.CreateAttribute("name");
						node.Attributes.Append(name);
					}
					if(name!=null)
					{
						int count;
						if(!countsByTag.TryGetValue(node.Name, out count))
						{
							count=config.StartCounterValue - 1;
							countsByTag.Add(node.Name, count);
						}
						count++;
						node.Attributes["name"].Value=node.Name+" "+count;
						countsByTag[node.Name]=count;
					}
				}
			}
			string output = Path.Combine(config.OutputFolder, Path.GetFileName(filename));
			Console.WriteLine("saving file {0}", output);
			doc.Save(output);
		}
	}
}
