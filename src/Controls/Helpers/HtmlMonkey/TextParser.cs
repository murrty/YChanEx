// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
[DebuggerDisplay("{InternalIndex}")]
public class TextParser : ITextParser<string> {
    private int InternalIndex;

    /// <summary>
    /// Returns the text currently being parsed.
    /// </summary>
    public string Text { get; private set; }

    public string BackingValue => Text;

    /// <summary>
    /// Constructs a new <see cref="ParsingHelper"></see> instance. Sets the text to be parsed
    /// and sets the current position to the start of that text.
    /// </summary>
    /// <param name="text">The text to be parsed. Can be <c>null</c>.</param>
    /// all methods that use regular expressions.</param>
    public TextParser(string? text) {
        Reset(text);
    }

    [MemberNotNull(nameof(Text))]
    public void Reset(string? text) {
        Text = text ?? string.Empty;
        InternalIndex = 0;
    }

    public int Index {
        get => InternalIndex;
        set {
            InternalIndex = value;
            if (InternalIndex < 0) {
                InternalIndex = 0;
            }
            else if (InternalIndex > Text.Length) {
                InternalIndex = Text.Length;
            }
        }
    }

    public bool EndOfText => InternalIndex >= Text.Length;

    public char Peek() {
        Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
        return (InternalIndex < Text.Length) ? Text[InternalIndex] : HtmlParser.NullChar;
    }

    public char Peek(int count) {
        int index = (InternalIndex + count);
        return (index >= 0 && index < Text.Length) ? Text[index] : HtmlParser.NullChar;
    }

    public char Get() {
        Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
        if (InternalIndex < Text.Length) {
            return Text[InternalIndex++];
        }
        return HtmlParser.NullChar;
    }

    public void Next() {
        Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
        if (InternalIndex < Text.Length) {
            InternalIndex++;
        }
    }

    public void SkipWhile(Func<char, bool> predicate) {
        Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
        while (InternalIndex < Text.Length && predicate(Text[InternalIndex])) {
            InternalIndex++;
        }
    }

    public void SkipWhiteSpace() => SkipWhile(char.IsWhiteSpace);

    public bool SkipTo(params char[] chars) {
        InternalIndex = Text.IndexOfAny(chars, InternalIndex);
        if (InternalIndex >= 0) {
            return true;
        }
        InternalIndex = Text.Length;
        return false;
    }

    public bool SkipTo(string s, bool includeToken = false) {
        InternalIndex = Text.IndexOf(s, InternalIndex);
        if (InternalIndex >= 0) {
            if (includeToken) {
                InternalIndex += s.Length;
            }
            return true;
        }
        InternalIndex = Text.Length;
        return false;
    }

    public bool SkipTo(string s, StringComparison comparison, bool includeToken = false) {
        InternalIndex = Text.IndexOf(s, InternalIndex, comparison);
        if (InternalIndex >= 0) {
            if (includeToken) {
                InternalIndex += s.Length;
            }
            return true;
        }
        InternalIndex = Text.Length;
        return false;
    }

    public string ParseCharacter() {
        Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
        if (InternalIndex < Text.Length) {
            return Text[InternalIndex++].ToString();
        }
        return string.Empty;
    }

    public string ParseWhile(Func<char, bool> predicate) {
        int start = InternalIndex;
        SkipWhile(predicate);
        return Extract(start, InternalIndex);
    }

    public string ParseQuotedText() {
        StringBuilder builder = new();

        // Get and skip quote character
        char quote = Get();

        // Parse quoted text
        while (!EndOfText) {
            // Parse to next quote
            builder.Append(ParseTo(quote));
            // Skip quote
            Next();
            // Two consecutive quotes treated as quote literal
            if (Peek() == quote) {
                builder.Append(quote);
                Next();
            }
            else {
                break; // Done if single closing quote or end of text
            }
        }
        return builder.ToString();
    }

    public string ParseTo(params char[] chars) {
        int start = InternalIndex;
        SkipTo(chars);
        return Extract(start, InternalIndex);
    }

    public string ParseTo(string s, StringComparison comparison, bool includeToken = false) {
        int start = InternalIndex;
        SkipTo(s, comparison, includeToken);
        return Extract(start, InternalIndex);
    }

    public bool MatchesCurrentPosition(string? s, StringComparison comparison) => !string.IsNullOrWhiteSpace(s) &&
        string.Compare(Text, InternalIndex, s, 0, s!.Length, comparison) == 0;

    public string Extract(int start, int end) {
        if (start < 0 || start > Text.Length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        if (end < start || end > Text.Length) {
            throw new ArgumentOutOfRangeException(nameof(end));
        }
        return Text[start..end];
    }

    public string Substring(int start, int length) {
        return Text.Substring(start, length);
    }
}
