using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SRXDStoryboard.Plugin;

public static class Compiler {
    public static Storyboard Compile(string path) {
        if (!TryParseFile(path, out var lines))
            return null;

        if (!TryCompileLines(lines, out var storyboard))
            return null;

        return storyboard;
    }

    private static void ThrowParseError(int lineIndex, int tokenIndex, string message) => Plugin.Logger.LogWarning($"Failed to parse storyboard line {lineIndex}, token {tokenIndex}: {message}");

    private static bool TryParseFile(string path, out List<List<object>> lines) {
        using var reader = new StreamReader(path);
        bool anyError = false;
        int index = 0;
        
        lines = new List<List<object>>();

        while (!reader.EndOfStream) {
            string line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var builder = new StringBuilder();
            var tokens = new List<object>();
            int quotesCount = 0;
            int parenthesisCount = 0;
            bool lineError = false;

            foreach (char c in line) {
                if (char.IsWhiteSpace(c) && quotesCount != 1 && parenthesisCount != 1) {
                    PopToken();
                    builder.Clear();

                    continue;
                }

                if (c == '\"')
                    quotesCount++;
                else if (c is '(' or ')')
                    parenthesisCount++;

                builder.Append(c);
            }

            PopToken();

            if (lineError)
                anyError = true;
            else
                lines.Add(tokens);

            index++;

            void PopToken() {
                if (builder.Length == 0)
                    return;

                string tokenString = builder.ToString();

                builder.Clear();

                if (string.IsNullOrWhiteSpace(tokenString))
                    return;

                object token = null;

                if (quotesCount > 0) {
                    if (quotesCount == 2 && tokenString[0] == '\"' && tokenString[tokenString.Length - 1] == '\"')
                        token = tokenString.Substring(1, tokenString.Length - 2);
                    else
                        ThrowParseError(index, tokens.Count, "Incorrectly formatted string");

                    quotesCount = 0;
                }
                else if (parenthesisCount > 0) {
                    if (parenthesisCount != 2 || tokenString[0] != '(' || tokenString[tokenString.Length - 1] != ')'
                        || !TryParseVector(tokenString.Substring(1, tokenString.Length - 2), builder, out token)) {
                        ThrowParseError(index, tokens.Count, "Incorrectly formatted vector");
                    }
                    
                    parenthesisCount = 0;
                }
                else if (Enum.TryParse<Keyword>(tokenString, out var keyword))
                    token = keyword;
                else if (bool.TryParse(tokenString, out bool boolVal))
                    token = boolVal;
                else if (int.TryParse(tokenString, out int intVal))
                    token = intVal;
                else if (float.TryParse(tokenString, out float floatVal))
                    token = floatVal;
                else if (!TryParseTimestamp(tokenString, builder, out token) && !TryParseVariable(tokenString, out token))
                    ThrowParseError(index, tokens.Count, "No valid format found");

                if (token == null)
                    lineError = true;
                
                tokens.Add(token);
            }
        }

        return anyError;
    }

    private static bool TryParseTimestamp(string token, StringBuilder builder, out object timestamp) {
        float beats = 0f;
        float ticks = 0f;
        float seconds = 0f;
        
        foreach (char c in token) {
            if (c is not ('b' or 't' or 's')) {
                builder.Append(c);
                
                continue;
            }

            if (!float.TryParse(builder.ToString(), out float value)) {
                timestamp = null;

                return false;
            }

            switch (c) {
                case 'b':
                    beats = value;
                    break;
                case 't':
                    ticks = value;
                    break;
                case 's':
                    seconds = value;
                    break;
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

    private static bool TryParseVector(string subString, StringBuilder builder, out object vector) {
        float[] values = new float[4];
        int count = 0;
        string token;
        
        foreach (char c in subString) {
            if (!char.IsWhiteSpace(c))
                builder.Append(c);
            else {
                token = builder.ToString();
                
                if (!string.IsNullOrWhiteSpace(token) && !TryPopValue()) {
                    vector = null;
                    
                    return false;
                }
                
                builder.Clear();
            }
        }
        
        token = builder.ToString();
        
        if (string.IsNullOrWhiteSpace(token) || !TryPopValue() || values.Length < 2) {
            vector = null;
                    
            return false;
        }

        switch (values.Length) {
            case 2:
                vector = new Vector2(values[0], values[1]);
                return true;
            case 3:
                vector = new Vector3(values[0], values[1], values[2]);
                return true;
            default:
                vector = new Vector4(values[0], values[1], values[2], values[3]);
                return true;
        }
        
        bool TryPopValue() {
            if (values.Length == 4 || !float.TryParse(token, out float value))
                return false;

            values[count] = value;
            count++;

            return true;
        }
    }

    private static bool TryParseVariable(string token, out object variable) {
        if (token[0] == '.' || token[token.Length - 1] == '.') {
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

    private static bool TryCompileLines(List<List<object>> lines, out Storyboard storyboard) {
        foreach (var line in lines) {
            
        }
    }
}