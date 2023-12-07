using PluginAPI.Core;
using System;
using System.IO;

namespace SCP575.API.Extensions
{
    /// <summary>
    /// A utility class containing audio-related extension methods.
    /// </summary>
    public static class AudioExtensions
    {

        /// <summary>
        /// Checks for .ogg files in the specified sounds folder.
        /// </summary>
        /// <returns>
        /// True if .ogg files are found; otherwise, false.
        /// </returns>
        public static bool AudioFilesExist()
        {
            try
            {
                string pathToAudios = EntryPoint.Instance.Config.PathToAudios;

                // Check if the folder exists before attempting to retrieve files.
                if (Directory.Exists(pathToAudios))
                {
                    // Get files with the .ogg extension.
                    string[] files = Directory.GetFiles(pathToAudios, "*.ogg");

                    // Check if .ogg files were found.
                    return files.Length > 0;
                }

                // The folder doesn't exist, so there are no .ogg files.
                return false;
            }
            catch (Exception e)
            {
                // Handle any exceptions that may occur during the process.
                Log.Error($"Error checking audio files: {e} -- {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a random audio file from the specified sounds folder.
        /// </summary>
        /// <returns>
        /// The path of a random .ogg file, or null if no files are found.
        /// </returns>
        public static string? GetRandomAudioFile()
        {
            try
            {
                string[] files = Directory.GetFiles(EntryPoint.Instance.Config.PathToAudios, "*.ogg");

                // Return a random .ogg file, or null if no files are found.
                return files.Length > 0 ? files[UnityEngine.Random.Range(0, files.Length)] : null;
            }
            catch (Exception e)
            {
                // Handle any exceptions that may occur during the process.
                Log.Error($"Error getting random audio file: {e} -- {e.Message}");
                return null;
            }
        }
    }
}
