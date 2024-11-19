using System.Text;
using System.Text.RegularExpressions;

class Program
{
    public static List<string> formatWhitelist = new List<string>
    {  ".cs",".html",".ts",".cpp",".h"
    };
    static string[] drives = Directory.GetLogicalDrives();

    static void Main()
    {
        string path;
        Console.WriteLine("Enter full path");
        Console.WriteLine("Path example:\nD:\\Projects\\c#\\Project1");
        path = Console.ReadLine();

        List<string> foundFolders = new();

        foundFolders.Add(@path);
        //foundFolders.Add(@"D:\Projects\c++c#\NewsSender");

        StringBuilder fileContentBuilder = new StringBuilder();
        // Read files from found folders
        List<FileContent> fileContents = ReadFolderFiles(foundFolders, formatWhitelist);
        foreach (var fileContent in fileContents)
        {
            string formattedContent = FormatFileContent(fileContent);
            fileContentBuilder.Append(formattedContent + "\n\n\n");
            Console.WriteLine(formattedContent + "\n\n\n");
        }
        SaveToFile(fileContentBuilder.ToString());
    }

    public static List<string> SearchDrives(string folderName, string[] drives)
    {
        List<string> foundFolders = new List<string>();
        foreach (string drive in drives)
        {
            SearchDirectory(drive, folderName, foundFolders);
        }
        Console.WriteLine("Found Folders:");
        foreach (var folder in foundFolders)
        {
            Console.WriteLine(folder);
        }
        return foundFolders;
    }

    /// <summary>
    /// Read files from found folders
    /// </summary>
    /// <param name="folderPaths"></param>
    /// <param name="formatWhitelist"></param>
    /// <returns></returns>
    static List<FileContent> ReadFolderFiles(List<string> folderPaths, List<string> formatWhitelist)
    {
        List<FileContent> fileContents = new List<FileContent>();

        foreach (var folderPath in folderPaths)
        {
            ReadFilesRecrs(folderPath, fileContents, formatWhitelist);
        }
        return fileContents;
    }

    static void ReadFilesRecrs(
        string path,
        List<FileContent> fileContents,
        List<string> formatWhitelist
    )
    {
        foreach (var file in Directory.GetFiles(path))
        {
            if (formatWhitelist.Contains(Path.GetExtension(file).ToLower()))
            {
                string content = File.ReadAllText(file);
                fileContents.Add(new FileContent { Content = content, Format = Path.GetExtension(file).ToLower(), Name = Path.GetFileName(file) }
                );
            }
        }
        foreach (var directory in Directory.GetDirectories(path))
        {
            ReadFilesRecrs(directory, fileContents, formatWhitelist);
        }
    }

    static void SearchDirectory(string path, string folderName, List<string> foundFolders)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                if (Path.GetFileName(directory).Equals(folderName, StringComparison.OrdinalIgnoreCase))
                {
                    foundFolders.Add(directory);
                }
                //search all subdirectoryes
                SearchDirectory(directory, folderName, foundFolders);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }


    }
    static string FormatFileContent(FileContent content)
    {
        var text = content.Content;
        switch (content.Format)
        {
            case ".html":
                text = FormatHTML(text);
                break;
            case ".cs":
                text = FormatCS(text);
                break;
            case ".ts":
                text = FormatCS(text);
                break;
            case ".cpp":
                text = FormatCS(text);
                break;
            case ".h":
                text = FormatCS(text);
                break;
            default: break;
        }

        return $"<{content.Name}>\n" + text + $"\n</{content.Name}>";
    }

    private static string FormatCS(string content)
    {
        // Сохраняем строковые литералы
        var stringLiterals = new Dictionary<string, string>();
        content = PreserveStringLiterals(content, stringLiterals);
        // Удаление однострочных комментариев
        content = Regex.Replace(content, @"//.*?$", "", RegexOptions.Multiline);

        // Удаление многострочных комментариев формата /* */
        content = Regex.Replace(content, @"/\*.*?\*/", "", RegexOptions.Singleline);

        // Удаление многострочных комментариев формата <!--  -->
        content = Regex.Replace(content, @"<!--.*?-->", "", RegexOptions.Singleline);

        // Удаление пробелов в начале и конце строк
        content = Regex.Replace(content, @"^\s+|\s+$", "", RegexOptions.Multiline);

        // Удаление пустых строк
        content = Regex.Replace(content, @"^\s*$\n|\r", "", RegexOptions.Multiline);

        // Замена нескольких пробелов одним
        content = Regex.Replace(content, @"\s+", " ");

        // Удаление пробелов вокруг скобок, запятых и точек с запятой
        content = Regex.Replace(content, @"\s*([{}()[\],;])\s*", "$1");

        // Удаление пробелов вокруг операторов
        //text = Regex.Replace(text, @"\s*([-+*/%=<>!&|^~])\s*", "$1");

        // Удаление пробелов вокруг операторов, кроме "=>"
        content = Regex.Replace(content, @"\s*((?<!=>)[-+*/%=<>!&|^~])\s*", "$1");

        // Обработка оператора "=>"
        content = Regex.Replace(content, @"\s*=>\s*", "=>");
        // Удаление пробелов после запятых в списках параметров
        content = Regex.Replace(content, @",\s+", ",");
        content = content.Trim();
        content = RestoreStringLiterals(content, stringLiterals);
        return content;
    }

    private static string FormatHTML(string content)
    {
        var stringLiterals = new Dictionary<string, string>();
        content = PreserveStringLiterals(content, stringLiterals);

        // Удаляем комментарии HTML
        content = Regex.Replace(content, @"<!--.*?-->", "", RegexOptions.Singleline);

        // Удаляем пробелы между тегами
        content = Regex.Replace(content, @">\s+<", "><");

        // Удаляем пробелы в начале и конце значений атрибутов
        content = Regex.Replace(content, @"(\w+)\s*=\s*""(\s*)(.+?)(\s*)""", "$1=\"$3\"");

        // Удаляем пробелы вокруг знаков равенства в атрибутах
        content = Regex.Replace(content, @"\s*=\s*", "=");

        // Удаляем пробелы в начале и конце строк
        content = Regex.Replace(content, @"^\s+|\s+$", "", RegexOptions.Multiline);

        // Удаляем переносы строк
        content = Regex.Replace(content, @"\r?\n", "");

        // Заменяем множественные пробелы одним
        content = Regex.Replace(content, @"\s+", " ");

        content = RestoreStringLiterals(content, stringLiterals);

        return content;
    }

    private static string PreserveStringLiterals(string code, Dictionary<string, string> stringLiterals)
    {
        int counter = 0;
        return Regex.Replace(code, @"@?""(?:\\.|[^""\\])*""",
            m =>
            {
                string placeholder = $"__STRING__{counter}__";
                stringLiterals[placeholder] = m.Value;
                counter++;
                return placeholder;
            }
        );
    }

    private static string RestoreStringLiterals(string code, Dictionary<string, string> stringLiterals)
    {
        foreach (var literal in stringLiterals)
        {
            code = code.Replace(literal.Key, literal.Value);
        }
        return code;
    }

    static void SaveToFile(string content)
    {
        try
        {
            File.WriteAllText(OutputFilePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static readonly string OutputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "minified_code.txt");
}

class FileContent
{
    public string Name { get; set; }
    public string Format { get; set; }
    public string Content { get; set; }
}
