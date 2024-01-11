#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
internal static class SelectorParsing {
    private readonly static Dictionary<char, string> SpecialCharacters = new() {
        { '#', "id" },
        { '.', "class" },
        { ':', "type" },
    };
    private static readonly char[] SpaceSeparator = [ ' ' ];

    public static Selector ParseSelector(string? selectorText) {
        TextParser parser = new(selectorText);
        return ReadSelector(parser);
    }

    public static SelectorCollection ParseSelectors(string? selectorText, SelectorCollection selectors) {
        if (!string.IsNullOrWhiteSpace(selectorText)) {
            TextParser parser = new(selectorText);

            while (!parser.EndOfText) {
                Selector readSelector = ReadSelector(parser);
                selectors.Add(readSelector);
            }
        }
        selectors.RemoveEmptySelectors();
        return selectors;
    }

    private static Selector ReadSelector(TextParser parser, Selector? initSelector = null) {
        parser.SkipWhiteSpace();
        Selector selector = initSelector = new();

        bool inSelector = false;
        bool setSelectorAsImmediateChild = false;

        while (!parser.EndOfText) {
            // Test next character
            char ch = parser.Peek();
            if (IsNameCharacter(ch) || ch == '*') {
                // Parse tag name
                //Selector selector = selectors.GetLastSelector();
                if (ch == '*') {
                    selector.Tag = null;    // Match all tags
                }
                else {
                    selector.Tag = parser.ParseWhile(IsNameCharacter);
                }
                inSelector = true;
            }
            else if (SpecialCharacters.TryGetValue(ch, out string? name)) {
                // Parse special attributes
                parser.Next();
                string value = parser.ParseWhile(IsValueCharacter);
                if (value.Length > 0) {
                    AttributeSelector attribute = new(name, value) {
                        Mode = AttributeSelectorMode.Contains
                    };

                    //Selector selector = selectors.GetLastSelector();
                    selector.Attributes.Add(attribute);
                }
            }
            else if (ch == '[') {
                // Parse attribute selector
                parser.Next();
                parser.SkipWhiteSpace();
                name = parser.ParseWhile(IsNameCharacter);
                if (name.Length > 0) {
                    AttributeSelector attribute = new(name);

                    // Parse attribute assignment operator
                    parser.SkipWhiteSpace();
                    if (parser.Peek() == '=') {
                        if (parser.Peek(1) == '=') {
                            attribute.Mode = AttributeSelectorMode.Match;
                            parser.Index += 2;
                        }
                        else {
                            attribute.Mode = parser.Peek(1) == ']' ?
                                AttributeSelectorMode.ExistsWithValue :
                                AttributeSelectorMode.Contains;
                            parser.Next();
                        }
                    }
                    else if (parser.Peek(1) == '=') {
                        Debug.Assert(parser.Peek() is ':' or '?');
                        if (parser.Peek() == ':') {
                            attribute.Mode = AttributeSelectorMode.RegEx;
                            parser.Index += 2;
                        }
                        else if (parser.Peek() == '?') {
                            attribute.Mode = AttributeSelectorMode.ContainsAny;
                            parser.Index += 2;
                        }
                    }
                    else {
                        attribute.Mode = AttributeSelectorMode.ExistsOnly;
                    }

                    // Parse attribute value
                    if (attribute.Mode < AttributeSelectorMode.ExistsOnly) {
                        parser.SkipWhiteSpace();

                        if (HtmlRules.IsQuoteChar(parser.Peek())) {
                            if (attribute.Mode == AttributeSelectorMode.ContainsAny) {
                                Debug.Assert(HtmlRules.IsQuoteChar(parser.Peek()));
                                attribute.Values = parser.ParseQuotedText().Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                            }
                            else {
                                attribute.Value = parser.ParseQuotedText();
                            }
                        }
                        //else if (attribute.Mode == AttributeSelectorMode.ContainsAny) {
                        //    Debug.Fail("ContainsAny requires quoted value.");
                        //    //Debug.Assert(HtmlRules.IsQuoteChar(parser.Peek()));
                        //    //attribute.Values = parser.ParseWhile(IsValueCharacter).Trim().Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                        //}
                        else {
                            attribute.Value = parser.ParseWhile(IsValueCharacter).Trim();
                        }

                        // Set the value to 'null' if there's no value.
                        if (string.IsNullOrWhiteSpace(attribute.Value)) {
                            attribute.Value = null;
                        }
                    }

                    //Selector selector = selectors.GetLastSelector();
                    selector.Attributes.Add(attribute);
                }

                // Close out attribute selector
                parser.SkipWhiteSpace();
                Debug.Assert(parser.Peek() == ']');
                if (parser.Peek() == ']') {
                    parser.Next();
                    if (setSelectorAsImmediateChild) {
                        if (selector.ParentSelector == null) {
                            selector.ImmediateChildOnly = true;
                        }
                        setSelectorAsImmediateChild = false;
                    }
                }
            }
            else if (ch == ',') {
                // Multiple selectors
                parser.Next();
                parser.SkipWhiteSpace();
                inSelector = false;
                setSelectorAsImmediateChild = false;
                //selectors.Add(new Selector());
                break;
            }
            else if (ch == '>') {
                // Whitespace indicates child selector
                parser.Next();
                parser.SkipWhiteSpace();
                // If the selector has no parent.
                if (selector.ParentSelector == null && !inSelector) {
                    setSelectorAsImmediateChild = true;
                }
                else {
                    //Debug.Assert(selectors.Any());
                    //Selector selector = selectors.AddChildSelector();
                    Selector childSelector = new(selector) {
                        ImmediateChildOnly = true
                    };
                    selector.ChildSelector = childSelector;
                    selector = childSelector;
                    setSelectorAsImmediateChild = false;
                }
            }
            else if (char.IsWhiteSpace(ch)) {
                // Handle whitespace
                parser.SkipWhiteSpace();
                // ',' and '>' change meaning of whitespace
                if (parser.Peek() != ',' && parser.Peek() != '>') {
                    //selectors.AddChildSelector();
                    Selector childSelector = new(selector);
                    selector.ChildSelector = childSelector;
                    selector = childSelector;
                }
            }
            else {
                Debug.Fail($"Unknown syntax '{ch}'");
                parser.Next();
            }
        }

        //selectors.RemoveEmptySelectors();
        return initSelector;
    }

    private static bool IsNameCharacter(char c) => char.IsLetterOrDigit(c) || c == '-';

    private static bool IsValueCharacter(char c) => IsNameCharacter(c);
}
