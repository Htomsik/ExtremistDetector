// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.Configuration;
using SamplesGenerator;

const string directory = "./Samples";
const int textSamplesCount = 20000;
const int imageSamplesCount = 5000;

var config = new ConfigurationBuilder()
    .AddJsonFile("Dictionary.json")
    .AddJsonFile("Templates.json")
    .Build();

var conf = config.GetSection("Words");
var words = conf.Get<List<string>>();

conf = config.GetSection("Templates");
var templates = conf.Get<List<string>>();

if (words == null || words.Count == 0) 
    throw new Exception("Violations doesn't exist");

if (templates == null || templates.Count == 0) 
    throw new Exception("Templates doesn't exist");

if(Directory.Exists(directory))
    Directory.Delete(directory, true);
Directory.CreateDirectory(directory);

var random = new Random();

//Generate text
for (int i = 0; i < textSamplesCount; i++)
{
    var template = templates[random.Next(templates.Count)];
    
    // 2 words add more variables
    var viol1 = words[random.Next(words.Count)];
    var viol2 = words[random.Next(words.Count)];
     
    var filePrefix = random.Next(3) switch {
        1 => "html",
        2 => "json",
        _ => "txt",
    };
    
    string generatedText = String.Format(template, viol1, viol2);
    string fileName = $"{directory}/{Guid.NewGuid()}.{filePrefix}";
    File.WriteAllText(fileName, $"{generatedText}");
}
Console.WriteLine("Text generated");

// Generate image
var imgGenerator = new ImageGenerator();
for (int i = 0; i < imageSamplesCount; i++)
{
    var template = templates[random.Next(templates.Count)];
    
    // 2 Violationы add more variables
    var viol1 = words[random.Next(words.Count)];
    var viol2 = words[random.Next(words.Count)];
     
    string generatedText = String.Format(template, viol1, viol2);
    string fileName = $"{directory}/{Guid.NewGuid()}.png";
    
    imgGenerator.Generate(fileName, generatedText);
}
Console.WriteLine("Image generated");