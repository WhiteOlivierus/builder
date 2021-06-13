using Newtonsoft.Json.Linq;
using RootsBuilder;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace builder
{
    class Program
    {
        private const string BUILDER_TOOLS = "BuilderTools";
        private const string NODE_JS = "NodeJs";
        private const string NODE_JS_URL = "https://nodejs.org/dist/v14.16.1/node-v14.16.1-win-x64.zip";


        private const string APP_TEMPLATE = "AppTemplate";
        private const string APP = "App";
        private const string APP_TEMPLATE_GIT = "https://api.github.com/repos/WhiteOlivierus/app-template/zipball";
        private const string PROJECT_NAME = "Project";

        static void Main(string[] args)
        {
            /*
                Setting up NodeJs 
            */
            Console.WriteLine("Downloading NodeJs");
            if (!Directory.Exists(BUILDER_TOOLS))
            {
                WebClient webClient = new();
                webClient.DownloadFile(NODE_JS_URL, $".\\{BUILDER_TOOLS}.zip");
                Console.WriteLine("Downloaded NodeJs");

                Console.WriteLine("Extracting NodeJs");
                ZipFile.ExtractToDirectory($".\\{BUILDER_TOOLS}.zip", BUILDER_TOOLS, true);

                // Renaming the folder in the builder tools so its easier accessible
                DirectoryInfo node = new($".\\{BUILDER_TOOLS}");
                if (!Directory.Exists($"{node.FullName}\\{NODE_JS}"))
                {
                    Directory.Move(node.GetDirectories()[0].FullName, $"{node.FullName}\\{NODE_JS}");
                    Directory.Delete(node.GetDirectories()[0].FullName);
                }

                File.Delete($".\\{BUILDER_TOOLS}.zip");
            }
            Console.WriteLine("Extracted NodeJs");

            /*
                Setting up App template
            */
            Console.WriteLine("Downloading App template");
            if (!Directory.Exists(APP_TEMPLATE))
            {
                using WebClient client = new();
                client.Headers.Add("user-agent", "Anything");
                client.DownloadFile(APP_TEMPLATE_GIT, $".\\{APP_TEMPLATE}.zip");
                Console.WriteLine("Downloaded App template");

                Console.WriteLine("Extracting App template");
                ZipFile.ExtractToDirectory($"{APP_TEMPLATE}.zip", APP_TEMPLATE, true);

                DirectoryInfo app = new($".\\{APP_TEMPLATE}");
                if (!Directory.Exists($"{app.FullName}\\{APP}"))
                {
                    Directory.Move(app.GetDirectories()[0].FullName, $"{app.FullName}\\{APP}");
                    Directory.Delete(app.GetDirectories()[0].FullName);
                }

                //Clean up
                File.Delete($"{APP_TEMPLATE}.zip");
            }
            Console.WriteLine("Extracted App template");

            /*
                Move the project files to the app template and overwrite if they allready exist
            */
            Console.WriteLine("Setup App template");
            if (!Directory.Exists($".\\{APP_TEMPLATE}\\{APP}\\src\\img"))
                Directory.CreateDirectory($".\\{APP_TEMPLATE}\\{APP}\\src\\img");

            CopyFilesRecursively(".\\img", $".\\{APP_TEMPLATE}\\{APP}\\src\\img");

            if (File.Exists($".\\{APP_TEMPLATE}\\{APP}\\src\\projectData.json"))
                File.Delete($".\\{APP_TEMPLATE}\\{APP}\\src\\projectData.json");

            File.Copy(".\\projectData.json", $".\\{APP_TEMPLATE}\\{APP}\\src\\projectData.json");
            Console.WriteLine("Setup done App template");

            /*
                Setup the package.json of the app
            */
            Console.WriteLine("Configure App template");
            string path = $".\\{APP_TEMPLATE}\\{APP}\\package.json";

            string json = File.ReadAllText(path);
            JObject package = JObject.Parse(json);

            SaveJsonKeyEdit(package, "author", "dutchskull");
            SaveJsonKeyEdit(package, "description", "A exported executable from the web");
            SaveJsonKeyEdit(package, "name", PROJECT_NAME);

            File.WriteAllText(path, package.ToString());
            Console.WriteLine("Configure done App template");

            /*
                The building step
            */
            Console.WriteLine("Building App template");
            CommandExecuter.ExecuteCommand($"cd .\\{APP_TEMPLATE}\\{APP} && .\\..\\..\\{BUILDER_TOOLS}\\{NODE_JS}\\npm.cmd i");

            CommandExecuter.ExecuteCommand($"cd .\\{APP_TEMPLATE}\\{APP} && .\\..\\..\\{BUILDER_TOOLS}\\{NODE_JS}\\npm.cmd link engine");

            CommandExecuter.ExecuteCommand($"cd .\\{APP_TEMPLATE}\\{APP}\\ && .\\..\\..\\{BUILDER_TOOLS}\\{NODE_JS}\\npm.cmd run make");
            Console.WriteLine("Build App template");

            /*
                Move the build to the root of the project
            */
            DirectoryInfo buildOutputPath = new($".\\{APP_TEMPLATE}\\{APP}\\out\\make\\squirrel.windows\\x64\\");
            if (File.Exists($"./{PROJECT_NAME}.exe"))
                File.Delete($"./{PROJECT_NAME}.exe");
            File.Move(buildOutputPath.GetFiles().Where(file => file.Name.Contains(".exe")).FirstOrDefault().FullName, $"./{PROJECT_NAME}.exe");

            if (File.Exists($"{PROJECT_NAME}.exe"))
                Process.Start("explorer.exe", $"{ PROJECT_NAME}.exe");
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        private static void SaveJsonKeyEdit(JObject package, string key, object value)
        {
            if (package.ContainsKey(key))
                package[key] = value.ToString();
            else
                package.Add(key, value.ToString());
        }
    }
}
