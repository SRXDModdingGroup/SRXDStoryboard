using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StoryboardSystem; 

public class StringRange : IEnumerable<char> {
    public string String { get; }
    
    public int Index { get; }
    
    public int Length { get; }

    private string subString;

    public StringRange(string s) : this(s, 0, s.Length) { }

    public StringRange(string s, int index, int length) {
        if (index < 0 || length < 0 || index + length > s.Length)
            throw new ArgumentOutOfRangeException();
        
        String = s;
        Index = index;
        Length = length;
    }

    public char this[int index] => String[Index + index];

    public bool Contains(char c) {
        for (int i = 0; i < Length; i++) {
            if (String[Index + 1] == c)
                return true;
        }

        return false;
    }

    public int IndexOf(string target) {
        int result = String.IndexOf(target, Index, Length, StringComparison.Ordinal);

        if (result < 0)
            return -1;

        return result + Index;
    }

    public StringRange Substring(int startIndex, int length) => new(String, Index + startIndex, length);

    public StringRange Trim() {
        int index = Index;
        int length = Length;
        
        while (index < String.Length - 1 && char.IsWhiteSpace(String[index])) {
            index++;
            length--;
        }

        while (length > 0 && char.IsWhiteSpace(String[Index + length - 1]))
            length--;

        return new StringRange(String, index, length);
    }

    public StringRange[] Split(char c) {
        int count = 0;

        for (int i = 0; i < Length; i++) {
            if (String[Index + i] == c)
                count++;
        }

        var arr = new StringRange[count + 1];
        int arrIndex = 0;
        int startIndex = 0;
        
        for (int i = 0; i <= Length; i++) {
            if (i < Length && String[Index + i] != c)
                continue;

            arr[arrIndex] = Substring(startIndex, i - startIndex);
            arrIndex++;
            startIndex = i + 1;
        }

        return arr;
    }

    public IEnumerator<char> GetEnumerator() {
        for (int i = 0; i < Length; i++)
            yield return String[Index + i];
    }

    public override string ToString() {
        subString ??= String.Substring(Index, Length);
        
        return subString;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool IsNullOrWhiteSpace(StringRange str) {
        if (string.IsNullOrEmpty(str.String))
            return true;
        
        for (int i = 0; i < str.Length; i++) {
            if (!char.IsWhiteSpace(str.String[str.Index + i]))
                return false;
        }

        return true;
    }
}

public static class StringRangeExtensions {
    public static Match Match(this Regex regex, StringRange str) => regex.Match(str.String, str.Index, str.Length);

    public static StringRange ToStringRange(this Group group, StringRange matchedRange) => new(matchedRange.String, group.Index, group.Length);
}