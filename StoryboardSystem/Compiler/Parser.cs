using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoryboardSystem;

public static class Parser {
    private static readonly Regex MATCH_FUNC_CALL = new (@"^(\w+)\((.*)\)$");
    private static readonly Regex MATCH_CHAIN_ELEMENT = new (@"^(\w+)(\[(.+)\])?$");
    
    public static bool TryParseFile(string path, out List<Instruction> instructions) {
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

            if (!TryTokenize(new StringRange(line), lineIndex, StoryboardManager.Instance.Logger, false, out var tokens, out _))
                success = false;
            else if (tokens[0] is not OpcodeT opcode) {
                StoryboardManager.Instance.Logger.LogWarning(GetParseError(lineIndex, "First argument must be an opcode"));
                success = false;
            }
            else {
                var arguments = new Token[tokens.Count - 1];

                for (int i = 0, j = 1; i < arguments.Length; i++, j++)
                    arguments[i] = tokens[j];

                instructions.Add(new Instruction(opcode.Opcode, arguments, lineIndex));
            }
        }

        return success;
    }

    public static bool TryTokenize(StringRange value, int lineIndex, ILogger logger, bool format, out List<Token> tokens, out string formattedText) {
        tokens = new List<Token>();
        formattedText = string.Empty;
        
        int length = value.Length;
        int startIndex = 0;
        var builder = new StringBuilder();
        bool success = true;

        for (int i = 0; i <= length; i++) {
            if (i == length || char.IsWhiteSpace(value[i])) {
                var subString = value.Substring(startIndex, i - startIndex);
                
                startIndex = i + 1;
                
                if (StringRange.IsNullOrWhiteSpace(subString)) {
                    if (!format)
                        continue;
                    
                    builder.Append(subString);

                    if (i < length)
                        builder.Append(value[i]);

                    continue;
                }

                if (!TryParseToken(subString, lineIndex, logger, format, out var token))
                    success = false;

                tokens.Add(token);

                if (!token.Success)
                    success = false;
                
                if (!format)
                    continue;

                builder.Append(token.FormattedText);
                
                if (i < length)
                    builder.Append(value[i]);

                continue;
            }
            
            switch (value[i]) {
                case '\"' when !TrySkipTo(value, ref i, '\"'):
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find closing quote"));
                    success = false;
                    break;
                case '{' when !TrySkipTo(value, ref i, '{', '}'):
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find closing brace"));
                    success = false;
                    break;
                case '(' when !TrySkipTo(value, ref i, '(', ')'):
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find closing parenthesis"));
                    success = false;
                    break;
                case '[' when !TrySkipTo(value, ref i, '[', ']'):
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find closing bracket"));
                    success = false;
                    break;
                case '}':
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find opening brace"));
                    success = false;
                    break;
                case ')':
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find opening parenthesis"));
                    success = false;
                    break;
                case ']':
                    logger?.LogWarning(GetParseError(lineIndex, "Could not find opening bracket"));
                    success = false;
                    break;
            }
        }

        if (format)
            formattedText = builder.ToString();

        return success;
    }

    public static bool TryParseToken(StringRange str, int lineIndex, ILogger logger, bool format, out Token token) {
        if (str[0] == '\"' && str[str.Length - 1] == '\"') {
            var subString = str.Substring(1, str.Length - 2);

            if (subString.Contains('\"'))
                token = new InvalidToken();
            else {
                token = new Constant(subString.ToString());

                if (format)
                    token.FormattedText = $"<color=#D08080FF>{str}</color>";
            }
        }
        else if (TryParseTimestamp(str, out token) || TryParsePrimitive(str, out token)) { }
        else if (Enum.TryParse<Opcode>(str.ToString(), true, out var opcode)) {
            token = new OpcodeT(opcode);

            if (format) {
                switch (opcode) {
                    case Opcode.Bind:
                    case Opcode.Bundle:
                    case Opcode.Curve:
                    case Opcode.In:
                    case Opcode.Inst:
                    case Opcode.InstA:
                    case Opcode.Load:
                    case Opcode.Out:
                    case Opcode.Post:
                        token.FormattedText = $"<color=#FFFF00FF>{str}</color>";
                        break;
                    case Opcode.Call:
                    case Opcode.Loop:
                    case Opcode.Key:
                        token.FormattedText = $"<color=#0080FFFF>{str}</color>";
                        break;
                    case Opcode.Proc:
                        token.FormattedText = $"<color=#00FFFFFF>{str}</color>";
                        break;
                    case Opcode.Set:
                    case Opcode.SetA:
                    case Opcode.SetG:
                        token.FormattedText = $"<color=#FF80FFFF>{str}</color>";
                        break;
                    default:
                        token.FormattedText = str.ToString();
                        break;
                }
            }
        }
        else if (Enum.TryParse<InterpType>(str.ToString(), true, out var interpType))
            token = new Constant(interpType);
        else if (TryParseArray(str, lineIndex, logger, format, out token)
                 || TryParseFuncCall(str, lineIndex, logger, format, out token)
                 || TryParseChain(str, lineIndex, logger, format, out token)) { }
        else {
            token = new InvalidToken();
            logger?.LogWarning(GetParseError(lineIndex, $"Incorrectly formatted token: {str}"));
        }

        if (!format)
            return token.Success;

        token.Range = str;

        if (token.Type == TokenType.Invalid) {
            token.FormattedText = $"<color=#FF0000FF>{str}</color>";

            return false;
        }

        if (string.IsNullOrWhiteSpace(token.FormattedText))
            token.FormattedText = str.ToString();

        return token.Success;
    }
    
    public static bool TrySkipTo(StringRange str, ref int index, char bounds) {
        while (true) {
            index++;
            
            if (index == str.Length)
                return false;
            
            if (str[index] == bounds)
                return true;
        }
    }
    
    public static bool TrySkipTo(StringRange str, ref int index, char start, char end) {
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

    public static IEnumerable<string> Split(StringRange value) {
        int length = value.Length;
        int startIndex = 0;
        int commentIndex = value.IndexOf("//");

        for (int i = 0; i <= length; i++) {
            if (i == length || char.IsWhiteSpace(value[i]) || i == commentIndex) {
                var subString = value.Substring(startIndex, i - startIndex);
                
                startIndex = i + 1;
                
                if (!StringRange.IsNullOrWhiteSpace(subString))
                    yield return subString.ToString();

                if (i != commentIndex)
                    continue;

                yield return value.Substring(i, length - i).ToString();
                yield break;
            }
            
            switch (value[i]) {
                case '\"':
                    TrySkipTo(value, ref i, '\"');

                    continue;
                case '{':
                    TrySkipTo(value, ref i, '{', '}');

                    continue;
                case '(':
                    TrySkipTo(value, ref i, '(', ')');

                    continue;
                case '[':
                    TrySkipTo(value, ref i, '[', ']');

                    continue;
            }
        }
    }

    private static bool TryParseTimestamp(StringRange value, out Token timestamp) {
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
                    timestamp = null;
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

        timestamp = new Constant(new Timestamp((Fixed) measures, (Fixed) beats, (Fixed) ticks, (Fixed) seconds));

        return true;
    }

    private static bool TryParsePrimitive(StringRange value, out Token primitive) {
        if (bool.TryParse(value.ToString(), out bool boolVal))
            primitive = new Constant(boolVal);
        else if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
            primitive = new Constant(intVal);
        else if (float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal))
            primitive = new Constant(floatVal);
        else {
            primitive = null;

            return false;
        }

        return true;
    }

    private static bool TryParseArray(StringRange value, int lineIndex, ILogger logger, bool format, out Token array) {
        if (value[0] != '{' || value[value.Length - 1] != '}') {
            array = null;

            return false;
        }

        bool success = TryTokenize(value.Substring(1, value.Length - 2), lineIndex, logger, format, out var tokens, out string formattedText);
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

        array.Success = success;

        if (format)
            array.FormattedText = $"{{{formattedText}}}";

        return true;
    }

    private static bool TryParseFuncCall(StringRange value, int lineIndex, ILogger logger, bool format, out Token funcCall) {
        var match = MATCH_FUNC_CALL.Match(value);
        var groups = match.Groups;

        if (!match.Success || !Enum.TryParse<FuncName>(groups[1].Value, true, out var name)) {
            funcCall = null;
            
            return false;
        }

        bool success = TryTokenize(groups[2].ToStringRange(value), lineIndex, logger, format, out var tokens, out string formattedText);

        funcCall = new FuncCall(name, tokens.ToArray());
        funcCall.Success = success;

        if (format)
            funcCall.FormattedText = $"{name}({formattedText})";

        return true;
    }

    private static bool TryParseChain(StringRange value, int lineIndex, ILogger logger, bool format, out Token chain) {
        var split = value.Split('.');

        if (split.Length == 0) {
            chain = null;
            
            return false;
        }

        var chainList = new List<Token>();
        bool success = true;

        foreach (var s in split) {
            if (StringRange.IsNullOrWhiteSpace(s)) {
                chainList.Add(new InvalidToken());
                success = false;
                
                continue;
            }

            var match = MATCH_CHAIN_ELEMENT.Match(s);

            if (!match.Success) {
                var invalid = new InvalidToken();

                if (format) {
                    invalid.Range = s;
                    invalid.FormattedText = $"<color=#FF0000FF>{s}</color>";
                }
                
                chainList.Add(invalid);
                success = false;
                
                continue;
            }

            var groups = match.Groups;
            var name = new Name(groups[1].Value);

            if (format) {
                name.Range = groups[1].ToStringRange(s);
                name.FormattedText = $"<color=#A0FFFFFF>{name}</color>";
            }

            chainList.Add(name);

            var indexerRange = groups[3].ToStringRange(s);

            if (StringRange.IsNullOrWhiteSpace(indexerRange))
                continue;

            if (!TryParseToken(indexerRange, lineIndex, logger, format, out var indexerToken))
                success = false;
            
            var indexer = new Indexer(indexerToken);

            if (format) {
                indexer.Range = indexerRange;
                indexer.FormattedText = $"[{indexerToken.FormattedText}]";
            }
                
            chainList.Add(indexer);
        }
        
        chain = new Chain(chainList.ToArray());
        chain.Success = success;

        if (!format)
            return true;

        var builder = new StringBuilder();

        for (int i = 0; i < chainList.Count; i++) {
            var token = chainList[i];
            
            if (i > 0 && token.Type != TokenType.Indexer)
                builder.Append('.');

            builder.Append(token.FormattedText);
        }

        chain.FormattedText = builder.ToString();

        return true;
    }
    
    private static string GetParseError(int line, string message) => $"Failed to parse storyboard line {line}: {message}";
}