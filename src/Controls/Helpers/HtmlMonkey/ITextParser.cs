// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
public interface ITextParser<T> {
    /// <summary>
    /// Gets or sets the current position within the text being parsed. Safely
    /// handles attempts to set to an invalid position.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Returns <c>true</c> if the current position is at the end of the text being parsed.
    /// Otherwise, false.
    /// </summary>
    public bool EndOfText { get; }

    /// <summary>
    /// Returns the value currently being parsed.
    /// </summary>
    public T BackingValue { get; }

    /// <summary>
    /// Sets the value to be parsed and sets the current position to the start of that value.
    /// </summary>
    /// <param name="value">The value to be parsed. Can be <c>null</c>.</param>
    public void Reset(T? value);

    /// <summary>
    /// Returns the character at the current position, or <see cref="NullChar"/>
    /// if the current position was at the end of the text being parsed.
    /// </summary>
    /// <returns>The character at the current position.</returns>
    public char Peek();

    /// <summary>
    /// Returns the character at the specified number of characters ahead of the
    /// current position, or <see cref="NullChar"></see> if the specified position
    /// is not valid. Does not change the current position.
    /// </summary>
    /// <param name="count">Specifies the position of the character to read as the number
    /// of characters ahead of the current position. May be a negative number.</param>
    /// <returns>The character at the specified position.</returns>
    public char Peek(int count);

    /// <summary>
    /// Returns the character at the current position and increments the current position.
    /// Returns <see cref="NullChar"/> if the current position was at the end of the text
    /// being parsed.
    /// </summary>
    /// <returns>The character at the current position.</returns>
    public char Get();

    /// <summary>
    /// Moves the current position ahead one character.
    /// </summary>
    public void Next();

    /// <summary>
    /// Moves the current position to the next character that causes <paramref name="predicate"/>
    /// to return <c>false</c>.
    /// </summary>
    /// <param name="predicate">Function to return test each character and return <c>true</c>
    /// for each character that should be skipped.</param>
    public void SkipWhile(Func<char, bool> predicate);

    /// <summary>
    /// Moves the current position to the next character that is not a whitespace character.
    /// </summary>
    public void SkipWhiteSpace();

    /// <summary>
    /// Moves the current position to the next character that is one of the specified characters
    /// and returns <c>true</c> if a match was found. If none of the specified characters are
    /// found, this method moves the current position to the end of the text being parsed and
    /// returns <c>false</c>.
    /// </summary>
    /// <param name="chars">Characters to skip to.</param>
    /// <returns>True if any of the specified characters were found. Otherwise, false.</returns>
    public bool SkipTo(params char[] chars);

    /// <summary>
    /// Moves the current position to the next occurrence of the specified string and returns
    /// <c>true</c> if a match was found. If the specified string is not found, this method
    /// moves the current position to the end of the text being parsed and returns <c>false</c>.
    /// </summary>
    /// <param name="s">String to skip to.</param>
    /// <param name="includeToken">If <c>true</c> and a match is found, the matching string is
    /// also skipped.</param>
    /// <returns>True if the specified string was found. Otherwise, false.</returns>
    public bool SkipTo(string s, bool includeToken = false);

    /// <summary>
    /// Moves the current position to the next occurrence of the specified string and returns
    /// <c>true</c> if a match was found. If the specified string is not found, this method
    /// moves the current position to the end of the text being parsed and returns <c>false</c>.
    /// </summary>
    /// <param name="s">String to skip to.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for
    /// search.</param>
    /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
    /// also skipped.</param>
    /// <returns>True if the specified string was found. Otherwise, false.</returns>
    public bool SkipTo(string s, StringComparison comparison, bool includeToken = false);

    /// <summary>
    /// Parses a single character and increments the current position. Returns an empty string
    /// if the current position was at the end of the text being parsed.
    /// </summary>
    /// <returns>A string that contains the parsed character, or an empty string if the current
    /// position was at the end of the text being parsed.</returns>
    public string ParseCharacter();

    /// <summary>
    /// Parses characters until the next character for which <paramref name="predicate"/>
    /// returns <c>false</c>, and returns the parsed characters. Can return an empty string.
    /// </summary>
    /// <param name="predicate">Function to test each character. Should return <c>true</c>
    /// for each character that should be parsed.</param>
    /// <returns>A string with the parsed characters.</returns>
    public string ParseWhile(Func<char, bool> predicate);

    /// <summary>
    /// Parses quoted text. The character at the current position is assumed to be the starting quote
    /// character. This method parses text up until the matching end quote character. Returns the parsed
    /// text without the quotes and sets the current position to the character following the
    /// end quote. If the text contains two quote characters together, the pair is handled as a
    /// single quote literal and not the end of the quoted text.
    /// </summary>
    /// <returns>Returns the text within the quotes.</returns>
    public string ParseQuotedText();

    /// <summary>
    /// Parses characters until the next occurrence of any one of the specified characters and
    /// returns a string with the parsed characters. If none of the specified characters are found,
    /// this method parses all character up to the end of the text being parsed. Can return an empty
    /// string.
    /// </summary>
    /// <param name="chars">The characters that cause parsing to end.</param>
    /// <returns>A string with the parsed characters.</returns>
    public string ParseTo(params char[] chars);

    /// <summary>
    /// Parses characters until the next occurrence of the specified string and returns a
    /// string with the parsed characters. If the specified string is not found, this method parses
    /// all character to the end of the text being parsed. Can return an empty string.
    /// </summary>
    /// <param name="s">Text that causes parsing to end.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules for
    /// comparing the specified string.</param>
    /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
    /// also parsed.</param>
    /// <returns>A string with the parsed characters.</returns>
    public string ParseTo(string s, StringComparison comparison, bool includeToken = false);

    /// <summary>
    /// Returns <c>true</c> if the given string matches the characters at the current position, or
    /// <c>false</c> otherwise.
    /// </summary>
    /// <param name="s">String to compare.</param>
    /// <param name="comparison">One of the enumeration values that specifies the rules to use in the
    /// comparison.</param>
    /// <returns>Returns <c>true</c> if the given string matches the characters at the current position,
    /// of <c>false</c> otherwise.</returns>
    public bool MatchesCurrentPosition(string? s, StringComparison comparison);

    /// <summary>
    /// Extracts a substring from the specified range of the text being parsed.
    /// </summary>
    /// <param name="start">0-based position of first character to be extracted.</param>
    /// <param name="end">0-based position of the character that follows the last
    /// character to be extracted.</param>
    /// <returns>Returns the extracted string.</returns>
    public string Extract(int start, int end);

    /// <summary>
    /// Extracts a substring from the specified index of the text being parsed.
    /// </summary>
    /// <param name="start">0-based position of first character to be extracted.</param>
    /// <param name="length">Length of the string to extract.</param>
    /// <returns>Returns the extracted string.</returns>
    public string Substring(int start, int length);
}
