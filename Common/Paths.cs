using System;
using System.IO;

namespace StudioManette
{
    namespace Utils
    {
        // Named "Paths" so there is no collision with System.IO.Path
        public static class Paths
        {
            // From the given string input, retrieve a string suitable for a file name
            // For instance this string: Toto"a>b<c
            // will yield: Toto_a_b_c
            public static string SanitiseFilename(string input)
            {
                return String.Join("_", input.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            }

            // Attemps to retrieve the path "toPath" relative from "fromPath"
            //
            // Typical example
            // with:
            // fromPath = Application.dataPath (= "D:/00_MANETTE/Edna_main/Assets")
            // toPath = "D:/00_MANETTE/Edna_main/Assets/Renderer/Textures/HDRI/HDRI_01.psd"
            // it yields:
            // "Assets\Renderer\Textures\HDRI\HDRI_01.psd"
            //
            // Beware, it can get tricky!
            //
            // GOTCHA: beware of trailing slashes!
            // With:
            // fromPath = "D:/00_MANETTE/Edna_main/Assets/" (exactly the same as the above example + a trailing "/")
            // toPath = "D:/00_MANETTE/Edna_main/Assets/Renderer/Textures/HDRI/HDRI_01.psd"
            // it yields:
            // "Renderer\Textures\HDRI\HDRI_01.psd"
            //
            // GOTCHA: matching seemingly unrelated paths
            // With:
            // fromPath = Application.dataPath (= "D:/00_MANETTE/Edna_main/Assets")
            // toPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Texture/BlueNoise16/L/LDR_LLL1_25.png"
            // it yields:
            // "Library\PackageCache\com.unity.render-pipelines.high-definition@12.1.6\Runtime\RenderPipelineResources\Texture\BlueNoise16\L\LDR_LLL1_25.png"
            public static string GetRelativePath(string fromPath, string toPath)
            {
                // Yay, yet another shoutout to https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
                if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
                if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

                Uri fromUri = new Uri(Path.GetFullPath(fromPath));
                Uri toUri = new Uri(Path.GetFullPath(toPath));

                if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                {
                    relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                return relativePath;
            }
        }
    }
}