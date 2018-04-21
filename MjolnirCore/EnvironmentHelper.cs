using System;
using System.IO;
using System.Linq;

namespace MjolnirCore {
  public class EnvironmentHelper {
    public static string SolutionFolderPath {
      get {
        var potentialDir = Directory.GetCurrentDirectory();
        while (true) {
          if (Directory.GetFiles(potentialDir).Any(s => s.EndsWith(".sln")))
            return potentialDir;
          else if (Directory.GetParent(potentialDir) == null)
            throw new DirectoryNotFoundException("Cannot find a parent solution directory.");
          else
            potentialDir = Directory.GetParent(potentialDir).FullName;
        }
      }
    }
  }
}
