using System.CommandLine;
using System.IO;
using System.Runtime.CompilerServices;

var rootCommand = new RootCommand("root command file bundle CLI");
var bundleCommand = new Command("bundle", "Bundle code files into a single file");

var bundelOptionoutput = new Option<FileInfo>(new[] { "--output", "-o" }, "File path and name");
bundleCommand.AddOption(bundelOptionoutput);

var languagesOption = new Option<List<string>>(new[] { "--language", "-l" })
{
    IsRequired = true,
    Description = "A list of programming languages.",
};
bundleCommand.AddOption(languagesOption);

var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Add comments with file paths and names");
bundleCommand.AddOption(noteOption);

var sortOption = new Option<bool>(new[] { "--sort", "-s" }, "Sort files before bundling");
bundleCommand.AddOption(sortOption);

var removeEmptyLinesOption = new Option<bool>(new[] { "--remove", "-r" }, "Remove empty lines");
bundleCommand.AddOption(removeEmptyLinesOption);

var authorOption = new Option<string>(new[] { "--author", "-a" }, "Add author information");
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, language, note, sort, remove, author) => {
    if (output == null)
    {
        Console.WriteLine("Error: Output file path is required.");
        return;
    }
    try
    {
        // Create and open the output file
        using (var newFile = File.Create(output.FullName))
        using (var writer = new StreamWriter(newFile))
        {
            if (language == null)
            {
                Console.WriteLine("Error: Language list is required.");
                return;
            }
            try
            {
                string[] allFiles = Array.Empty<string>();
                if (language.Contains("all"))
                {
                    // Get all files in the directory except .rsp files
                    allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                        .Where(file => !file.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }
                else
                {
                    // Filter files based on the provided programming languages
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

                // Sort files if requested
                if (sort)
                    allFiles = allFiles.OrderBy(file => Path.GetExtension(file).ToLower()).ToArray();
                else
                    allFiles = allFiles.OrderBy(file => Path.GetFileName(file).ToLower()).ToArray();

                // Add author information if provided
                if (author != null)
                {
                    writer.WriteLine($"// {author}");
                }
                // Write the content of each file to the output file
                foreach (var file in allFiles)
                {
                    if (file != output.FullName) // Skip the output file itself
                    {
                        if (note) // Add comments with source file details
                        {
                            writer.WriteLine($"// Source file: {Path.GetFileName(file)}");
                            writer.WriteLine($"// Relative path: {Path.GetRelativePath(Directory.GetCurrentDirectory(), file)}");
                            writer.WriteLine();
                        }

                        using (var reader = new StreamReader(file))
                        {
                            string fileContent = reader.ReadToEnd();
                            // Remove empty lines if requested
                            if (remove)
                            {
                                fileContent = string.Join(
                                    Environment.NewLine,
                                    fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                        .Where(line => !string.IsNullOrWhiteSpace(line)));
                            }

                            writer.WriteLine(fileContent);
                        }
                        writer.WriteLine(); // Add a blank line between files
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        Console.WriteLine("File was created successfully.");
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: File path is invalid.");
    }
}, bundelOptionoutput, languagesOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

// Command to create a .rsp file with predefined options
var createRspCommand = new Command("create-rsp", "Create a response file with pre-filled command options.");

var rspOutputOption = new Option<FileInfo>(new[] { "--rsp-output", "-rsp" }, "Path to save the .rsp response file");
createRspCommand.AddOption(rspOutputOption);

createRspCommand.SetHandler((rspOutput) =>
{
    // Get user input for command options
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

    // Construct the .rsp command
    var rspCommand = $"bundle --output \"{outputPath}\" --language {string.Join(",", languages)}" +
                     (note ? " --note" : "") +
                     (sort ? " --sort" : "") +
                     (remove ? " --remove" : "") +
                     (author != null ? $" --author \"{author}\"" : "");
    if (rspOutput == null)
    {
        Console.WriteLine("Error: RSP output file path is required.");
        return;
    }
    try
    {
        File.WriteAllText(rspOutput.FullName, rspCommand);
        Console.WriteLine($"Response file created: {rspOutput.FullName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating response file: {ex.Message}");
    }
}, rspOutputOption);

// Add commands to the root command
rootCommand.AddCommand(createRspCommand);
rootCommand.AddCommand(bundleCommand);

// Execute the root command
rootCommand.InvokeAsync(args);
