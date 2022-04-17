using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoryboardSystem;

internal static class Parser {
    private static readonly Regex MATCH_FUNC_CALL = new (@"^(\w+)\((.*)\)$");
    private static readonly Regex MATCH_CHAIN_ELEMENT = new (@"^(\w+)(\[(.+)\])?$");
    
    public static bool TryParseFile(string path, ILogger logger, out List<Instruction> instructions) {
        using var reader = new StreamReader(path);
        bool success = true;
        int lineIndex = 0;
        
        instructions = new List<Instruction>();

        while (!reader.EndOfStream) {
            lineIndex++;
            
            string line = reader.ReadLine();
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            int commentIndex = line.IndexOf("//", StringComparison.Ordinal);

            if (commentIndex >= 0)
                line = line.Substring(0, commentIndex);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!TryTokenize(line, lineIndex, logger, out var tokens))
                success = false;
            else if (tokens[0] is not Constant { Value: Opcode opcode }) {
                logger.LogWarning(GetParseError(lineIndex, "First argument must be an opcode"));
                success = false;
            }
            else {
                var arguments = new Token[tokens.Count - 1];

                for (int i = 0, j = 1; i < arguments.Length; i++, j++)
                    arguments[i] = tokens[j];

                instructions.Add(new Instruction(opcode, arguments, lineIndex));
            }
        }

        return success;
    }

    private static bool TryTokenize(string value, int lineIndex, ILogger logger, out List<Token> tokens) {
        tokens = new List<Token>();
        
        int length = value.Length;
        int startIndex = 0;

        for (int i = 0; i <= length; i++) {
            if (i == length || char.IsWhiteSpace(value[i])) {
                string subString = value.Substring(startIndex, i - startIndex);
                
                startIndex = i + 1;
                
                if (string.IsNullOrWhiteSpace(subString))
                    continue;

                if (!TryParseToken(subString, lineIndex, logger, out var token)) {
                    logger.LogWarning(GetParseError(lineIndex, $"Incorrectly formatted token: {subString}"));

                    return false;
                }
                
                tokens.Add(token);

                continue;
            }
            
            switch (value[i]) {
                case '\"' when !TrySkipTo(value, ref i, '\"'):
                    logger.LogWarning(GetParseError(lineIndex, "Could not find closing quote"));

                    return false;
                case '{' when !TrySkipTo(value, ref i, '{', '}'):
                    logger.LogWarning(GetParseError(lineIndex, "Could not find closing brace"));

                    return false;
                case '(' when !TrySkipTo(value, ref i, '(', ')'):
                    logger.LogWarning(GetParseError(lineIndex, "Could not find closing parenthesis"));

                    return false;
                case '[' when !TrySkipTo(value, ref i, '[', ']'):
                    logger.LogWarning(GetParseError(lineIndex, "Could not find closing bracket"));

                    return false;
                case '}':
                    logger.LogWarning(GetParseError(lineIndex, "Could not find opening brace"));

                    return false;
                case ')':
                    logger.LogWarning(GetParseError(lineIndex, "Could not find opening parenthesis"));

                    return false;
                case ']':
                    logger.LogWarning(GetParseError(lineIndex, "Could not find opening bracket"));

                    return false;
            }
        }

        return true;
    }

    private static bool TryParseToken(string str, int lineIndex, ILogger logger, out Token token) {
        token = default;
        
        if (str[0] == '\"' && str[str.Length - 1] == '\"') {
            string subString = str.Substring(1, str.Length - 2);

            if (subString.Contains("\"")) {
                return false;
            }
            
            token = new Constant(subString);
        }
        else if (TryParseTimestamp(str, out token)) { }
        else if (TryParsePrimitive(str, out token)) { }
        else if (Enum.TryParse<Opcode>(str, true, out var opcode))
            token = new Constant(opcode);
        else if (Enum.TryParse<InterpType>(str, true, out var interpType))
            token = new Constant(interpType);
        else if (!TryParseArray(str, lineIndex, logger, out token)
                 && !TryParseFuncCall(str, lineIndex, logger, out token)
                 && !TryParseChain(str, lineIndex, logger, out token)) {
            return false;
        }

        return true;
    }

    private static bool TryParseTimestamp(string value, out Token timestamp) {
        float measures = 0f;
        float beats = 0f;
        float ticks = 0f;
        float seconds = 0f;
        var builder = new StringBuilder();
        
        builder.Clear();
        
        foreach (char c in value) {
            if (c is not ('m' or 'b' or 't' or 's')) {
                builder.Append(c);
                
                continue;
            }

            string s = builder.ToString();
            string[] split = s.Split('/');
            float floatVal;

            switch (split.Length) {
                case 1 when float.TryParse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture, out floatVal):
                    break;
                case 2 when float.TryParse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float num)
                            && float.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float den):
                    floatVal = num / den;
                    break;
                default:
                    timestamp = default;
                    return false;
            }

            switch (c) {
                case 'm':
                    measures += floatVal;
                    break;
                case 'b':
                    beats += floatVal;
                    break;
                case 't':
                    ticks += floatVal;
                    break;
                case 's':
                    seconds += floatVal;
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

        timestamp = new Constant(new Timestamp(measures, beats, ticks, seconds));

        return true;
    }

    private static bool TryParsePrimitive(string value, out Token primitive) {
        if (bool.TryParse(value, out bool boolVal))
            primitive = new Constant(boolVal);
        else if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
            primitive = new Constant(intVal);
        else if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
            primitive = new Constant(floatVal);
        else {
            primitive = null;

            return false;
        }

        return true;
    }

    private static bool TryParseArray(string value, int lineIndex, ILogger logger, out Token array) {
        if (value[0] != '{' || value[value.Length - 1] != '}' || !TryTokenize(value.Substring(1, value.Length - 2), lineIndex, logger, out var tokens)) {
            array = null;

            return false;
        }

        bool isConstant = true;

        foreach (var token in tokens) {
            if (token is Constant)
                continue;

            isConstant = false;

            break;
        }

        if (isConstant) {
            object[] objArr = new object[tokens.Count];

            for (int i = 0; i < objArr.Length; i++)
                objArr[i] = ((Constant) tokens[i]).Value;

            array = new Constant(objArr);
        }
        else
            array = new ArrayT(tokens.ToArray());

        return true;
    }

    private static bool TryParseFuncCall(string value, int lineIndex, ILogger logger, out Token funcCall) {
        var match = MATCH_FUNC_CALL.Match(value);

        if (match.Success && Enum.TryParse<FuncName>(match.Groups[1].Value, true, out var name) && TryTokenize(match.Groups[2].Value, lineIndex, logger, out var tokens)) {
            funcCall = new FuncCall(name, tokens.ToArray());

            return true;
        }

        funcCall = null;

        return false;
    }

    private static bool TryParseChain(string token, int lineIndex, ILogger logger, out Token chain) {
        chain = null;

        string[] split = token.Split('.');

        if (split.Length == 0)
            return false;

        var chainList = new List<Token>();

        foreach (string s in split) {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var match = MATCH_CHAIN_ELEMENT.Match(s);

            if (!match.Success)
                return false;

            chainList.Add(new Name(match.Groups[1].Value));

            string indexer = match.Groups[3].Value;

            if (string.IsNullOrWhiteSpace(indexer))
                continue;
            
            if (TryParseToken(indexer, lineIndex, logger, out var indexerToken))
                chainList.Add(new Indexer(indexerToken));
            else
                return false;
        }
        
        chain = new Chain(chainList.ToArray());

        return true;
    }
    
    private static bool TrySkipTo(string str, ref int index, char bounds) {
        do {
            index++;
            
            if (str[index] == bounds)
                return true;
        } while (index < str.Length);

        return false;
    }
    
    private static bool TrySkipTo(string str, ref int index, char start, char end) {
        int depth = 0;

        while (index < str.Length) {
            char c = str[index];

            if (c == start)
                depth++;
            else if (c == end)
                depth--;

            if (depth == 0)
                return true;

            index++;
        }

        return false;
    }
    
    private static string GetParseError(int line, string message) => $"Failed to parse storyboard line {line}: {message}";
}