using System.IO;

using UnityEngine;

namespace MachinMachines
{
    namespace Utils
    {
        // Packages versions have to be a semantic version, hence this helper class
        [System.Serializable]
        public struct SemanticVersion
        {
            public int major;
            public int minor;
            public int patch;

            public static SemanticVersion FromString(string input)
            {
                string[] tokens = input.Split('.');
                return new SemanticVersion { major = int.Parse(tokens[0]), minor = int.Parse(tokens[1]), patch = int.Parse(tokens[2]) };
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{patch}";
            }
        }

        [System.Serializable]
        public class PackageManifest : ScriptableObject
        {
            // Hiding the inherited member "name", hence this "new"
            new public string name;
            public string displayName;
            // The version has to be a semantic version
            public string version;
            // Our own tambouille: this allows to store custom data
            // The main use is to have a decoupling so Tupac does not need to know EVERYTHING
            // that goes here - the sending and receiving systems are fully reponsible
            public string additionalData;

            public SemanticVersion SemanticVersion
            {
                get
                {
                    return SemanticVersion.FromString(version);
                }
                set
                {
                    version = value.ToString();
                }
            }

            // Simple JSON deserialisation for the package at the given path
            public static PackageManifest GetAtPath(string manifestPath)
            {
                PackageManifest result = ScriptableObject.CreateInstance<PackageManifest>();
                using (StreamReader stream = new StreamReader(manifestPath))
                {
                    string data = stream.ReadToEnd();
                    JsonUtility.FromJsonOverwrite(data, result);
                }
                return result;
            }
        }
    }
}
