using System.CommandLine;
using System.IO;
using System.Runtime.CompilerServices;

var rootCommand = new RootCommand("root command fule bundel CLI");
var bundleCommand = new Command("bundle", "bundel code files to a singel file");

var bundelOptionoutput = new Option<FileInfo>(new[] { "--output", "-o" }, "file path and name");
bundleCommand.AddOption(bundelOptionoutput);

var languagesOption = new Option<List<string>>(new[] { "--language", "-l" }) { 
    IsRequired = true,
    Description = "A list of programming languages.",    
};
bundleCommand.AddOption(languagesOption);

var noteOption = new Option<bool>(new[] { "--note", "-n" }, "note file path and name");
bundleCommand.AddOption(noteOption);

var sortOption = new Option<bool>(new[] { "--sort", "-s" }, "sort files when copeing");
bundleCommand.AddOption(sortOption);

var removeEmptyLinesOption = new Option<bool>(new[] { "--remove", "-r" }, "remove Empty Lines Option");
bundleCommand.AddOption(removeEmptyLinesOption);

var authorOption = new Option<string>(new[] { "--author", "-a" }, "author Option");
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, language, note,sort,remove, author) => {
    if (output != null)
    {
        try {
            //create and open file
            using (var newFile = File.Create(output.FullName))
            using (var writer = new StreamWriter(newFile))
            {
                if (language != null)
                {
                    try
                    {
                        string[] allFiles = Array.Empty<string>();
                        if (language.Contains("all"))
                        {
                            //get all files in directory except rsp file
                            allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                            .Where(file => !file.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        }
                        else
                        {
                            foreach (var l in language)
                            {
                                var fileExtensions = l switch
                                {
                                    "c#" => new[] { "*.cs" },
                                    "c++" => new[] { "*.cpp" },
                                    "c" => new[] { "*.c" },
                                    "html" => new[] { "*.html" },
                                    "java" => new[] { "*.java" },
                                    "python" => new[] { "*.py" },
                                    "javascript" => new[] { "*.js" },
                                    "typescript" => new[] { "*.ts" },
                                    _ => Array.Empty<string>()
                                };

                                foreach (var ext in fileExtensions)
                                    allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), ext, SearchOption.AllDirectories);
                            }
                        }
                        if (sort)
                        {
                            allFiles = allFiles.OrderBy(file => Path.GetExtension(file).ToLower()).ToArray();
                        } else 
                            allFiles = allFiles.OrderBy(file => Path.GetFileName(file).ToLower()).ToArray();

                        if(author != null)
                        {
                            writer.WriteLine($"// {author}");
                        }
                        foreach (var file in allFiles)
                        {
                            // copy all files exsept the new file
                            if (file != output.FullName)
                            {
                                // אם המשתמש בחר באפשרות "note", הוספת הערה
                                if (note)
                                {
                                    writer.WriteLine($"// Source file: {Path.GetFileName(file)}");
                                    writer.WriteLine($"// Relative path: {Path.GetRelativePath(Directory.GetCurrentDirectory(), file)}");
                                    writer.WriteLine();
                                }
                                //copy cur file to new file
                                using (var reader = new StreamReader(file))
                                {
                                    string fileContent = reader.ReadToEnd();
                                    if (remove)
                                    {
                                        fileContent = string.Join(
                                        Environment.NewLine,
                                            fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                                .Where(line => !string.IsNullOrWhiteSpace(line))
                                        );

                                    }

                                    writer.WriteLine(fileContent);
                                }
                                writer.WriteLine(); // הוספת רווח נוסף בין קבצים
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }

            }
            Console.WriteLine("file was created");
        }
         catch (DirectoryNotFoundException ex) {
            Console.WriteLine("Eroor: file path is invaild");
        } 
    }
   
}, bundelOptionoutput, languagesOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

// פקודת create-rsp
var createRspCommand = new Command("create-rsp", "Create a response file with pre-filled command options.");

var rspOutputOption = new Option<FileInfo>(new[] { "--rsp-output", "-rsp" }, "Path to save the .rsp response file");
createRspCommand.AddOption(rspOutputOption);

createRspCommand.SetHandler((rspOutput) =>
{
    Console.WriteLine("Please enter the programming languages (comma separated):");
    string languagesInput = Console.ReadLine();
    var languages = languagesInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(lang => lang.Trim())
                                  .ToList();

    Console.WriteLine("Please enter the output file path:");
    string outputPath = Console.ReadLine();

    Console.WriteLine("Do you want to add a note (yes/no)?");
    bool note = Console.ReadLine()?.ToLower() == "yes";

    Console.WriteLine("Do you want to sort the files (yes/no)?");
    bool sort = Console.ReadLine()?.ToLower() == "yes";

    Console.WriteLine("Do you want to remove empty lines (yes/no)?");
    bool remove = Console.ReadLine()?.ToLower() == "yes";

    Console.WriteLine("Please enter the author name (optional):");
    string author = Console.ReadLine();

    var rspCommand = $"fib bundle --output \"{outputPath}\" --language {string.Join(",", languages)}" +
                     (note ? " --note" : "") +
                     (sort ? " --sort" : "") +
                     (remove ? " --remove" : "") +
                     (author != null ? $" --author \"{author}\"" : "");

    // Save the command to the .rsp file
    if (rspOutput != null)
    {
        try
        {
            File.WriteAllText(rspOutput.FullName, rspCommand);  // תיקון כאן
            Console.WriteLine($"Response file created: {rspOutput.FullName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating response file: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Response file path is invalid.");
    }
},rspOutputOption);

rootCommand.AddCommand(createRspCommand);
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);
