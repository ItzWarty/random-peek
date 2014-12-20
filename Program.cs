using System;
using System.Collections.Generic;
using ItzWarty;
using ItzWarty.Collections;
using System.IO;
using System.Linq;

namespace random_peek {
   class Program {
      private const string kRepositoriesRoot = "c:/my-repositories";
      private static readonly IReadOnlySet<string> kCodeFileExtensions = ImmutableSet.Of("cpp", "c", "hpp", "cs");
      static void Main(string[] args) {
         var linesToPrint = 5;
         var chopLongLines = false;

         for (var i = 0; i < args.Length; i++) {
            switch (args[i]) {
               case "-n":
                  linesToPrint = int.Parse(args[++i]);
                  break;
               case "-c":
                  chopLongLines = true;
                  break;
            }
         }

         var repositoryNames = GetRepositoryNames();
         var documents = new List<Document>();
         foreach (var repositoryName in repositoryNames) {
            var repositoryPath = Path.Combine(kRepositoriesRoot, repositoryName);
            foreach (var filePath in Directory.GetFiles(repositoryPath, "*", SearchOption.AllDirectories)) {
               var fileInfo = new FileInfo(filePath);
               if (kCodeFileExtensions.Contains(fileInfo.Extension.Trim('.').ToLower())) {
                  documents.Add(new Document(repositoryName, filePath, File.ReadAllText(filePath).Split('\n')));
               }
            }
         }
         var loc = documents.Sum(x => x.Lines.Length - linesToPrint);
         var startLine = new Random((int)DateTime.Now.ToFileTime()).Next(loc);
         foreach (var document in documents) {
            if (startLine < document.Lines.Length) {
               Console.WriteLine("Line {0} of {1}".F(startLine + 1, document.FilePath));
               for (int i = 0, line = startLine; i < linesToPrint && line < document.Lines.Count(); i++, line++) {
                  var preline = (line + 1) + ": ";
                  var lineBody = document.Lines[line];
                  if (chopLongLines && lineBody.Length > Console.BufferWidth - preline.Length) {
                     lineBody = lineBody.Substring(0, Console.BufferWidth - preline.Length);
                     Console.Write(preline + lineBody);
                  } else {
                     Console.WriteLine(preline + lineBody);
                  }
               }
               break;
            } else {
               startLine -= document.Lines.Length - linesToPrint;
            }
         }
      }

      private static string[] GetRepositoryNames() {
         var scriptLines = File.ReadAllText(Path.Combine(kRepositoriesRoot, "dargon-developer-scripts/dargon.sh")).Split('\n');
         var repositoryNamesLine = scriptLines.First(x => x.Contains("declare") && x.Contains("(") && x.Contains(")"));
         var arrayStart = repositoryNamesLine.IndexOf('(');
         var arrayEnd = repositoryNamesLine.LastIndexOf(')');
         var arrayContents = repositoryNamesLine.Substring(arrayStart + 1, arrayEnd - (arrayStart + 1));
         return arrayContents.Trim().QASS(' ');
      }

      private class Document {
         private readonly string repositoryName;
         private readonly string filePath;
         private readonly string[] lines;

         public Document(string repositoryName, string filePath, string[] lines) {
            this.repositoryName = repositoryName;
            this.filePath = filePath;
            this.lines = lines;
         }

         public string RepositoryName { get { return repositoryName; } }
         public string FilePath { get { return filePath; } }
         public string[] Lines { get { return lines; } }
      }
   }
}
