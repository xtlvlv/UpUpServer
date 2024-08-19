namespace UpUpServer;

using System;
using System.IO;

public class NamespaceProcess
{
    static string rootPath = @"D:\Unity\ConsoleProject\ConsoleApp2"; // 替换为你的项目路径
    static string targetNamespace = "UpUpServer"; // 替换为你的目标命名空间
    static List<string> ingoreDirs = new List<string>() { "obj", "bin", }; // 替换为你的忽略目录

    public static void NamespaceBatchChange()
    {
        Console.WriteLine("将会对文件进行批量添加命名空间操作，此操作不可逆，请注意备份，是否继续？(y/n)");
        var key = Console.ReadKey();
        Console.WriteLine();
        if (key.KeyChar == 'y')
        {
            Console.WriteLine("开始批量添加命名空间...");
            InsertNamespaceToFiles();
            Console.WriteLine("批量添加命名空间完成");
        }
        else
        {
            Console.WriteLine("已取消批量添加命名空间操作");
            return;
        }
    }

    static void InsertNamespaceToFiles()
    {
        string[] files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            bool isIgnore = false;
            ingoreDirs.ForEach(x =>
            {
                if (file.Contains(x))
                    isIgnore = true;
            });
            if (isIgnore)
            {
                Log.Info($"忽略文件：{file}");
                continue;
            }

            string content = File.ReadAllText(file);

            if (!content.Contains($"namespace {targetNamespace};"))
            {
                content = $"namespace {targetNamespace};\n\n{content}";

                File.WriteAllText(file, content);
            }
        }
    }
}