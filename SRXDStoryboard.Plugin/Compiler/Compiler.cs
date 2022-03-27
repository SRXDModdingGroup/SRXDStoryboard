using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SRXDStoryboard.Plugin;

public static class Compiler {
    public static Storyboard Compile(string path) {
        using var reader = new StreamReader(path);
        
        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var builder = new StringBuilder();
            var tokens = new List<object>();
            int quotesCount = 0;
            int index = 0;

            foreach (char c in line) {
                if (char.IsWhiteSpace(c)) {
                    PopToken();
                    
                    continue;
                }

                if (c == '\"')
                    quotesCount++;

                builder.Append(c);
            }
            
            PopToken();
            
            index++;

            void PopToken() {
                if (builder.Length == 0)
                    return;
                
                string token = builder.ToString();

                builder.Clear();
                
                if (string.IsNullOrWhiteSpace(token))
                    return;

                if (quotesCount > 0) {
                    if (quotesCount == 2 && token[0] == '\"' && token[token.Length - 1] == '\"')
                        tokens.Add(token.Substring(1, token.Length - 2));
                    else
                        ThrowError("Incorrectly formatted string");

                    quotesCount = 0;

                    return;
                }
                
                ThrowError("No valid format found");
            }

            void ThrowError(string message) => Plugin.Logger.LogWarning($"Failed to parse storyboard line {index}, token {tokens.Count}: {message}");
        }
    }
}