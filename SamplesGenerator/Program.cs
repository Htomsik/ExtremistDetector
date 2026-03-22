// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.Configuration;
using SamplesGenerator;
using SamplesGenerator.Models;


const int textSamplesCount = 10000;
const int imageSamplesCount = 2000;
const int garbageSamplesCount = 600;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("Dictionary.json")
    .AddJsonFile("Templates.json")
    .AddEnvironmentVariables() // For Docker 
    .Build();

var appSettings = config.GetSection("Settings").Get<Settings>();
if(appSettings == null)
    appSettings = new Settings();

var conf = config.GetSection("Words");
var words = conf.Get<List<string>>();

conf = config.GetSection("Templates");
var templates = conf.Get<List<string>>();

if (words == null || words.Count == 0) 
    throw new Exception("Violations doesn't exist");

if (templates == null || templates.Count == 0) 
    throw new Exception("Templates doesn't exist");

if(!Directory.Exists(appSettings.WorkDirectory))
    Directory.CreateDirectory(appSettings.WorkDirectory);

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
    string fileName = Path.Combine(appSettings.WorkDirectory, $"{Guid.NewGuid()}.{filePrefix}");
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
    string fileName = Path.Combine(appSettings.WorkDirectory, $"{Guid.NewGuid()}.png");
    
    imgGenerator.Generate(fileName, generatedText);
}
Console.WriteLine("Image generated");


// Garbage
for (int i = 0; i < garbageSamplesCount; i++)
{
    var filePrefix = random.Next(3) switch {
        1 => "html",
        2 => "json",
        _ => "txt",
    };
    
    var fileName = Path.Combine(appSettings.WorkDirectory, $"{Guid.NewGuid()}.{filePrefix}");

    switch (new Random().Next(1,5))
    {
        // Zero Broken File
        case 1:
            File.Create(fileName).Dispose();
            break;
        
        // Non valid file
        case 2:
            File.WriteAllText(Path.Combine(appSettings.WorkDirectory, $"{Guid.NewGuid()}.exe"), "I am NOT a virus, trust me BROOOOOO");
            break;
        
        // Lock files
        case 3:
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            _ = Task.Delay(5000).ContinueWith(_ => fs.Dispose());
            break;
    }
}

Console.WriteLine("trash generated");