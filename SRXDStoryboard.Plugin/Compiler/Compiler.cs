﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SRXDStoryboard.Plugin;

public class Compiler {
    private static readonly Regex MATCH_TOKEN = new(@"//.*|""[^""]*""|\([^()]*\)|[\w.-]+");
    private static readonly float[] PARSE_VECTOR_VALUES = new float[4];
    private static readonly StringBuilder PARSE_TIMESTAMP_BUILDER = new();

    private Dictionary<string, Variable> variables;

    private Compiler() { }

    private bool TryCompileInstructions(List<Instruction> instructions, out Storyboard storyboard) {
        variables = new Dictionary<string, Variable>();
        
        foreach (var instruction in instructions) {
            switch (instruction.Keyword) {
                
            }
        }
    }

    private bool TryGetArguments(object[] arguments) => arguments.Length == 0;
    private bool TryGetArguments<T>(object[] arguments, out T arg) {
        if (arguments.Length == 1 && TryResolveImmediateOrVariable(arguments[0], out arg))
            return true;

        arg = default;

        return false;
    }
    private bool TryGetArguments<T0, T1>(object[] arguments, out T0 arg0, out T1 arg1) {
        if (arguments.Length == 2
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[0], out arg1))
            return true;

        arg0 = default;
        arg1 = default;

        return false;
    }
    private bool TryGetArguments<T0, T1, T2>(object[] arguments, out T0 arg0, out T1 arg1, out T2 arg2) {
        if (arguments.Length == 3
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[1], out arg1)
            && TryResolveImmediateOrVariable(arguments[2], out arg2))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;

        return false;
    }
    private bool TryGetArguments<T0, T1, T2, T3>(object[] arguments, out T0 arg0, out T1 arg1, out T2 arg2, out T3 arg3) {
        if (arguments.Length == 4
            && TryResolveImmediateOrVariable(arguments[0], out arg0)
            && TryResolveImmediateOrVariable(arguments[1], out arg1)
            && TryResolveImmediateOrVariable(arguments[2], out arg2)
            && TryResolveImmediateOrVariable(arguments[3], out arg3))
            return true;

        arg0 = default;
        arg1 = default;
        arg2 = default;
        arg3 = default;

        return false;
    }

    private bool TryResolveImmediateOrVariable<T>(object argument, out T value) {
        if (argument is T cast0) {
            value = cast0;

            return true;
        }
        
        if (argument is not string[] hierarchy || !variables.TryGetValue(hierarchy[0], out var variable)) {
            value = default;

            return false;
        }

        for (int i = 1; i < hierarchy.Length; i++) {
            if (!variable.TryGetSubVariable(hierarchy[i], out var temp)) {
                value = default;

                return false;
            }

            variable = temp;
        }

        if (variable is T cast1) {
            value = cast1;

            return true;
        }

        if (variable.Value is T cast2) {
            value = cast2;

            return true;
        }

        value = default;

        return false;
    }

    public static bool TryCompileFile(string path, out Storyboard storyboard) {
        if (TryParseFile(path, out var instructions))
            return new Compiler().TryCompileInstructions(instructions, out storyboard);
        
        storyboard = null;
            
        return false;
    }

    private static void ThrowParseError(int lineIndex, int tokenIndex, string message)
        => Plugin.Logger.LogWarning($"Failed to parse storyboard line {lineIndex}, token {tokenIndex}: {message}");

    private static bool TryParseFile(string path, out List<Instruction> instructions) {
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
                ThrowParseError(index, shift, "No keyword found");
                anyError = true;
            }
            else if (tokens[shift] is not Keyword keyword) {
                ThrowParseError(index, shift, "Argument must be a keyword");
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
        else if (Enum.TryParse<Keyword>(value, true, out var keyword))
            token = keyword;
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
            if (float.TryParse(split[i], out PARSE_VECTOR_VALUES[i]))
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

            if (!float.TryParse(PARSE_TIMESTAMP_BUILDER.ToString(), out float floatVal)) {
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
        else if (int.TryParse(value, out int intVal))
            primitive = intVal;
        else if (float.TryParse(value, out float floatVal))
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