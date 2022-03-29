using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SRXDStoryboard.Plugin;

public static class Parser {
    private static readonly Regex MATCH_TOKEN = new(@"//.*|""[^""]*""|\([^()]*\)|[\w.-]+");
    private static readonly float[] PARSE_VECTOR_VALUES = new float[4];
    private static readonly StringBuilder PARSE_TIMESTAMP_BUILDER = new();

    public static bool TryParseFile(string path, out List<Instruction> instructions) {
        using var reader = new StreamReader(path);
        bool anyError = false;
        int index = 0;
        
        instructions = new List<Instruction>();

        while (!reader.EndOfStream) {
            string line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var matches = MATCH_TOKEN.Matches(line);
            int count = 0;

            foreach (Match match in matches) {
                if (match.Value.StartsWith("//"))
                    break;

                count++;
            }

            if (count == 0)
                continue;

            object[] tokens = new object[count];
            bool parseError = false;

            for (int i = 0; i < count; i++) {
                string match = matches[i].Value;

                if (TryParseToken(match, index, i, out object token))
                    tokens[i] = token;
                else
                    parseError = true;
            }

            if (parseError) {
                anyError = true;
                instructions.Add(new Instruction());
                
                continue;
            }

            int shift;
            Timestamp timestamp;

            if (tokens[0] is Timestamp newTimestamp) {
                shift = 1;
                timestamp = newTimestamp;
            }
            else {
                shift = 0;
                timestamp = Timestamp.Zero;
            }

            if (shift >= tokens.Length) {
                ThrowParseError(index, shift, "No opcode found");
                anyError = true;
            }
            else if (tokens[shift] is not Opcode keyword) {
                ThrowParseError(index, shift, "Argument must be an opcode");
                anyError = true;
            }
            else {
                object[] arguments = new object[tokens.Length - shift - 1];
                
                if (arguments.Length > 0)
                    Array.Copy(tokens, shift + 1, arguments, 0, arguments.Length);
                
                instructions.Add(new Instruction(timestamp, keyword, arguments));
            }

            index++;
        }

        return anyError;
    }

    private static void ThrowParseError(int lineIndex, int tokenIndex, string message)
        => Plugin.Logger.LogWarning($"Failed to parse storyboard line {lineIndex}, token {tokenIndex}: {message}");

    private static bool TryParseToken(string value, int line, int index, out object token) {
        if (value[0] == '\"' && value[value.Length - 1] == '\"')
            token = value.Substring(1, value.Length - 2);
        else if (value[0] == '(' && value[value.Length - 1] == ')') {
            if (TryParseVector(value, out token))
                return true;
            
            ThrowParseError(line, index, "Incorrectly formatted vector");
            token = null;

            return false;
        }
        else if (Enum.TryParse<Opcode>(value, true, out var opcode))
            token = opcode;
        else if (Enum.TryParse<InterpType>(value, true, out var interpType))
            token = interpType;
        else if (!TryParseTimestamp(value, out token)
                 && !TryParsePrimitive(value, out token)
                 && !TryParseVariable(value, out token)) {
            ThrowParseError(line, index, "Incorrectly formatted token");
            token = null;
            
            return false;
        }

        return true;
    }

    private static bool TryParseVector(string value, out object vector) {
        string[] split = value.Substring(1, value.Length - 1).Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

        if (split.Length is < 2 or > 4) {
            vector = null;

            return false;
        }

        for (int i = 0; i < split.Length; i++) {
            if (float.TryParse(split[i], NumberStyles.Any, CultureInfo.InvariantCulture, out PARSE_VECTOR_VALUES[i]))
                continue;
            
            vector = null;

            return false;
        }

        switch (split.Length) {
            case 2:
                vector = new Vector2(PARSE_VECTOR_VALUES[0], PARSE_VECTOR_VALUES[1]);
                return true;
            case 3:
                vector = new Vector3(PARSE_VECTOR_VALUES[0], PARSE_VECTOR_VALUES[1], PARSE_VECTOR_VALUES[2]);
                return true;
            default:
                vector = new Vector4(PARSE_VECTOR_VALUES[0], PARSE_VECTOR_VALUES[1], PARSE_VECTOR_VALUES[2], PARSE_VECTOR_VALUES[3]);
                return true;
        }
    }

    private static bool TryParseTimestamp(string value, out object timestamp) {
        float beats = 0f;
        float ticks = 0f;
        float seconds = 0f;
        
        PARSE_TIMESTAMP_BUILDER.Clear();
        
        foreach (char c in value) {
            if (c is not ('b' or 't' or 's')) {
                PARSE_TIMESTAMP_BUILDER.Append(c);
                
                continue;
            }

            if (!float.TryParse(PARSE_TIMESTAMP_BUILDER.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float floatVal)) {
                timestamp = null;

                return false;
            }

            switch (c) {
                case 'b':
                    beats = floatVal;
                    break;
                case 't':
                    ticks = floatVal;
                    break;
                case 's':
                    seconds = floatVal;
                    break;
            }

            PARSE_TIMESTAMP_BUILDER.Clear();
        }

        if (PARSE_TIMESTAMP_BUILDER.Length > 0) {
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

    private static bool TryParseVariable(string token, out object variable) {
        if (token.Contains("+") || token.Contains("-")) {
            variable = null;

            return false;
        }

        string[] split = token.Split('.');

        if (split.Length == 0) {
            variable = null;

            return false;
        }

        foreach (string s in split) {
            if (!string.IsNullOrWhiteSpace(s))
                continue;
            
            variable = null;

            return false;
        }

        variable = split;

        return true;
    }
}