using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PastaEngine
{
    public class DialogueLine
    {
        public string SpeakerName;
        public string Text;
    }

    public static class DialogueLoader
    {
        // Returns a Dictionary. Key = "INTRO_MEETING", Value = List of lines.
        public static Dictionary<string, List<DialogueLine>> LoadFromFile(string path)
        {
            var conversations = new Dictionary<string, List<DialogueLine>>();

            if (!File.Exists(path)) return conversations;

            string[] lines = File.ReadAllLines(path);
            string currentID = "";

            foreach (string line in lines)
            {
                string cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine)) continue;

                // 1. Detect Header (Starts with #)
                if (cleanLine.StartsWith("#"))
                {
                    currentID = cleanLine.Substring(1).Trim(); // Remove #
                    conversations[currentID] = new List<DialogueLine>();
                }
                // 2. Detect Dialogue (Contains :)
                else if (cleanLine.Contains(":") && !string.IsNullOrEmpty(currentID))
                {
                    var parts = cleanLine.Split(new[] { ':' }, 2); // Split only on first ':'

                    var dialogueNode = new DialogueLine
                    {
                        SpeakerName = parts[0].Trim(),
                        Text = parts[1].Trim()
                    };

                    conversations[currentID].Add(dialogueNode);
                }
            }

            return conversations;
        }
    }
}