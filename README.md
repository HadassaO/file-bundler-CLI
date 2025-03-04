# File Bundle CLI

A command-line tool for bundling code files into a single file with customizable options. The CLI supports filtering files by programming language, sorting, removing empty lines, adding author information, and generating response (.rsp) files for predefined commands.

## Features

- **Bundle Files**: Merge multiple code files into a single output file.
- **Language Filtering**: Select specific programming languages or bundle all code files.
- **Sorting**: Option to sort files before bundling.
- **Remove Empty Lines**: Clean up files by removing empty lines.
- **Add Comments**: Include file paths and names as comments.
- **Author Information**: Add author details to the output file.
- **Response Files (.rsp)**: Generate response files for pre-filled command options.

## Installation

### Prerequisites
- .NET 6.0 SDK or higher

### Clone the Repository

```sh
git clone https://github.com/HadassaO/file-bundler-CLI.git
cd cli-project
```

### Build and Run

```sh
dotnet run -- [command] [options]
```

## Usage

### Bundling Files

```sh
dotnet run -- bundle --output output.txt --language c#,python --note --sort --remove --author "Your Name"
```

#### Options:
| Option | Alias | Description |
|--------|-------|-------------|
| `--output` | `-o` | Specify the output file path (required). |
| `--language` | `-l` | Comma-separated list of programming languages to include (e.g., `c#`, `python`, `java`). Use `all` to include all files. |
| `--note` | `-n` | Include file paths and names as comments. |
| `--sort` | `-s` | Sort files before bundling. |
| `--remove` | `-r` | Remove empty lines from files. |
| `--author` | `-a` | Add author information at the top of the output file. |

### Creating a Response File (.rsp)

A response file stores command-line arguments for reuse.

```sh
dotnet run -- create-rsp --rsp-output config.rsp
```

#### Options:
| Option | Alias | Description |
|--------|-------|-------------|
| `--rsp-output` | `-rsp` | Specify the path to save the .rsp response file. |

#### Example Response File Content:
```
bundle --output "output.txt" --language c#,python --note --sort --remove --author "Your Name"
```
To use the response file:
```sh
dotnet run @config.rsp
```

## Error Handling
- If the output file path is missing, an error message is displayed.
- If no programming languages are provided, an error is returned.
- Invalid directory paths or file operations are caught and reported.

## License
This project is licensed under the MIT License.

