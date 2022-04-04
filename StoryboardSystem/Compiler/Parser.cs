using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoryboardSystem;

internal static class Parser {
    private static readonly Regex MATCH_INDEXER = new (@"^(\w+)(\[.+\])?$");
    
    public static bool TryParseFile(string path, out List<Instruction> instructions) {
        using var reader = new StreamReader(path);
        var logger = StoryboardManager.Instance.Logger;
        bool success = true;
        int lineIndex = 1;
        
        instructions = new List<Instruction>();

        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            int commentIndex = line.IndexOf("//", StringComparison.Ordinal);

            if (commentIndex >= 0)
                line = line.Substring(0, commentIndex);

            if (string.IsNullOrWhiteSpace(line)) {
                lineIndex++;
                
                continue;
            }

            if (!TryTokenize(line, lineIndex, logger, out object[] tokens))
                success = false;
            else if (tokens[0] is not Opcode opcode) {
                logger.LogWarning(GetParseError(lineIndex, "First argument must be an opcode"));
                success = false;
            }
            else {
                object[] arguments = new object[tokens.Length - 1];
                
                if (arguments.Length > 0)
                    Array.Copy(tokens, 1, arguments, 0, arguments.Length);
                
                instructions.Add(new Instruction(opcode, arguments, lineIndex));
            }

            lineIndex++;
        }

        return success;
    }

    private static bool TryTokenize(string value, int lineIndex, ILogger logger, out object[] tokens) {
        var tokenList = new List<object>();
        int length = value.Length;
        int startIndex = 0;

        tokens = null;

        for (int i = 0; i <= length; i++) {
            if (i == length || char.IsWhiteSpace(value[i])) {
                string subString = value.Substring(startIndex, i - startIndex);
                
                startIndex = i + 1;
                
                if (string.IsNullOrWhiteSpace(subString))
                    continue;

                if (!TryParseToken(subString, out object token)) {
                    logger.LogWarning($"Incorrectly formatted token: {subString}");

                    return false;
                }
                
                tokenList.Add(token);

                continue;
            }
            
            switch (value[i]) {
                case '\"': {
                    if (i == startIndex && TryGetWithin(value, ref i, '\"', out string subString) && (i >= value.Length - 1 || char.IsWhiteSpace(value[i + 1]))) {
                        tokenList.Add(subString);
                        startIndex = i + 1;

                        continue;
                    }

                    logger.LogWarning(GetParseError(lineIndex, "Incorrectly formatted string"));
                        
                    return false;
                }
                case '{': {
                    if (i == startIndex && TryGetWithin(value, ref i, '{', '}', out string subString) && (i >= value.Length - 1 || char.IsWhiteSpace(value[i + 1]))) {
                        if (!TryTokenize(subString, lineIndex, logger, out object[] subTokens))
                            return false;

                        tokenList.Add(subTokens);
                        startIndex = i + 1;

                        continue;
                    }

                    logger.LogWarning(GetParseError(lineIndex, "Incorrectly formatted array"));
                        
                    return false;
                }
                case '(': {
                    string nameString = value.Substring(startIndex, i - startIndex);

                    foreach (char c in nameString) {
                        if (char.IsLetterOrDigit(c) || c == '_')
                            continue;
                        
                        logger.LogWarning(GetParseError(lineIndex, "Invalid expression name"));
                        
                        return false;
                    }
                    
                    if (!string.IsNullOrWhiteSpace(nameString) && TryGetWithin(value, ref i, '(', ')', out string subString) && (i >= value.Length - 1 || char.IsWhiteSpace(value[i + 1]))) {
                        if (!TryTokenize(subString, lineIndex, logger, out object[] arguments))
                            return false;

                        tokenList.Add(new Expression(new Name(nameString), arguments));
                        startIndex = i + 1;

                        continue;
                    }

                    logger.LogWarning(GetParseError(lineIndex, "Incorrectly formatted expression"));
                        
                    return false;
                }
            }
        }

        tokens = tokenList.ToArray();

        return true;
    }

    private static bool TryParseToken(string str, out object token) {
        if (TryParseTimestamp(str, out token) || TryParsePrimitive(str, out token)) { }
        else if (Enum.TryParse<Opcode>(str, true, out var opcode))
            token = opcode;
        else if (Enum.TryParse<InterpType>(str, true, out var interpType))
            token = interpType;
        else if (Enum.TryParse<AssetType>(str, true, out var assetType))
            token = assetType;
        else if (!TryParseNameOrChain(str, out token)) {
            token = null;

            return false;
        }

        return true;
    }

    private static bool TryParseTimestamp(string value, out object timestamp) {
        int beats = 0;
        float ticks = 0f;
        float seconds = 0f;
        var builder = new StringBuilder();
        
        builder.Clear();
        
        foreach (char c in value) {
            if (c is not ('b' or 't' or 's')) {
                builder.Append(c);
                
                continue;
            }

            string s = builder.ToString();

            switch (c) {
                case 'b' when int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out beats):
                case 't' when float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out ticks):
                case 's' when float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out seconds):
                    break;
                default:
                    timestamp = null;

                    return false;
            }

            builder.Clear();
        }

        if (builder.Length > 0) {
            timestamp = null;

            return false;
        }

        timestamp = new Timestamp(beats, ticks, seconds);

        return true;
    }

    private static bool TryParsePrimitive(string value, out object primitive) {
        if (bool.TryParse(value, out bool boolVal))
            primitive = boolVal;
        else if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
            primitive = intVal;
        else if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
            primitive = floatVal;
        else {
            primitive = null;

            return false;
        }

        return true;
    }

    private static bool TryParseNameOrChain(string token, out object nameOrChain) {
        nameOrChain = null;

        foreach (char c in token) {
            if (!char.IsLetterOrDigit(c) && c is not ('.' or '[' or ']' or '_'))
                return false;
        }

        string[] split = token.Split('.');

        if (split.Length == 0) {
            nameOrChain = null;

            return false;
        }

        var chain = new List<object>();

        foreach (string s in split) {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var match = MATCH_INDEXER.Match(s);

            if (!match.Success)
                return false;
            
            chain.Add(new Name(match.Groups[1].ToString()));

            string indexer = match.Groups[2].ToString();

            if (string.IsNullOrWhiteSpace(indexer))
                continue;
            
            if (TryParseToken(indexer, out object indexerToken))
                chain.Add(new Indexer(indexerToken));
            else
                return false;
        }
        
        if (chain.Count == 1)
            nameOrChain = new Name(split[0]);
        else
            nameOrChain = new Chain(chain.ToArray());

        return true;
    }
    
    private static bool TryGetWithin(string str, ref int index, char bounds, out string subString) {
        index++;
        
        int startIndex = index;

        while (index < str.Length && str[index] != bounds)
            index++;

        if (index == str.Length) {
            subString = null;

            return false;
        }

        subString = str.Substring(startIndex, index - startIndex);

        return true;
    }
    
    private static bool TryGetWithin(string str, ref int index, char start, char end, out string subString) {
        index++;
        
        int startIndex = index;
        int depth = 1;

        while (index < str.Length && depth > 0) {
            char c = str[index];

            if (c == start)
                depth++;
            else if (c == end)
                depth--;

            if (depth > 0)
                index++;
        }

        if (depth > 0) {
            subString = null;

            return false;
        }

        subString = str.Substring(startIndex, index - startIndex);

        return true;
    }
    
    private static string GetParseError(int line, string message) => $"Failed to parse storyboard line {line}: {message}";
}