using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StoryboardSystem;

internal static class Parser {
    private static readonly Regex MATCH_INDEXER = new (@"^(\w+)(\[.+\])?$");
    
    public static bool TryParseFile(string path, out List<Instruction> instructions) {
        using var reader = new StreamReader(path);
        var logger = StoryboardManager.Instance.Logger;
        bool success = true;
        int index = 1;
        
        instructions = new List<Instruction>();

        while (!reader.EndOfStream) {
            string line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line)) {
                index++;
                
                continue;
            }

            if (!TryTokenize(line, index, logger, out object[] tokens))
                success = false;
            else if (tokens[0] is not Opcode opcode) {
                logger.LogWarning(GetParseError(index, "No opcode found"));
                success = false;
            }
            else {
                object[] arguments = new object[tokens.Length - 1];
                
                if (arguments.Length > 0)
                    Array.Copy(tokens, 1, arguments, 0, arguments.Length);
                
                instructions.Add(new Instruction(opcode, arguments, index));
            }

            index++;
        }

        return success;
    }

    private static bool TryTokenize(string value, int index, ILogger logger, out object[] tokens) {
        var builder = new StringBuilder();
        var tokenList = new List<object>();
        int length = value.Length;

        tokens = null;

        for (int i = 0; i < length; i++) {
            char c = value[i];

            switch (c) {
                case '\"': {
                    if (builder.Length == 0) {
                        i++;

                        while (i < length) {
                            c = value[i];

                            if (c == '\"') {
                                tokenList.Add(builder.ToString());
                                builder.Clear();

                                break;
                            }

                            builder.Append(c);
                            i++;
                        }

                        if (i != length && (i == length - 1 || char.IsWhiteSpace(value[i + 1])))
                            continue;
                    }
                
                    logger.LogWarning(GetParseError(index, "Incorrectly formatted string"));
                    
                    return false;
                }
                case '{': {
                    if (builder.Length == 0) {
                        int depth = 1;

                        i++;

                        while (i < length) {
                            c = value[i];

                            if (c == '{')
                                depth++;
                            else if (c == '}') {
                                depth--;

                                if (depth == 0) {
                                    if (!TryTokenize(builder.ToString(), index, logger, out object[] arr))
                                        return false;

                                    tokenList.Add(arr);
                                    builder.Clear();

                                    break;
                                }
                            }

                            builder.Append(c);
                            i++;
                        }

                        if (i != length && (i == length - 1 || char.IsWhiteSpace(value[i + 1])))
                            continue;
                    }
                
                    logger.LogWarning(GetParseError(index, "Incorrectly formatted array"));
                    
                    return false;
                }
            }

            if (c == '/' && i < length - 1 && value[i + 1] == '/')
                break;

            if (char.IsWhiteSpace(c)) {
                if (!TryPopToken())
                    return false;
            }
            else
                builder.Append(c);
        }

        if (!TryPopToken() || tokenList.Count == 0)
            return false;

        tokens = tokenList.ToArray();

        return true;
        
        bool TryPopToken() {
            if (builder.Length == 0)
                return true;

            string str = builder.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return true;

            if (!TryParseToken(str, out object token)) {
                logger.LogWarning(GetParseError(index, "Incorrectly formatted token"));
                
                return false;
            }

            tokenList.Add(token);
            builder.Clear();

            return true;
        }
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
        
        if (!token.All(c => char.IsLetterOrDigit(c) || c is '.' or '[' or ']'))
            return false;

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
    
    private static string GetParseError(int index, string message) => $"Failed to parse storyboard line {index}: {message}";
}