using System;
using System.Text;
using System.Text.RegularExpressions;
using ScrewTurn.Wiki.PluginFramework;
using System.Collections.Generic;

namespace BlockQuoteWithFormatting
{
    /// <summary>
    /// So much of this class was stolen from
    /// Greenicicle ScrewTurn Syntax Highlighter
    /// http://code.google.com/p/greeniciclescrewturnsyntaxhighlighter/source/browse/trunk/%20greeniciclescrewturnsyntaxhighlighter/src/SyntaxHighlightPlugin/SyntaxHighlightFormatProvider.cs
    /// I give them much props - thank you.
    /// </summary>
    public class BlockQuote : IFormatterProviderV30
    {
        private const string LogName = "WrappingQuoteBlock";
        private const int NotFound = -1;
        private IHostV30 _host;
        private string _config;
        private bool _enableLogging;

        public bool PerformPhase1
        {
            get { return false; }
        }

        public bool PerformPhase2
        {
            get { return true; }
        }

        public bool PerformPhase3
        {
            get { return false; }
        }

        public int ExecutionPriority
        {
            get { return 50; }
        }

        public string Format(string raw, ContextInformation context, FormattingPhase phase)
        {
            if (context.Context != FormattingContext.PageContent) return raw;

            var targetText = new StringBuilder();
            string openingTag = "{quote}";
            string closingTag = "{/quote}";
            var pattern = new Regex(openingTag + ".*?" + closingTag, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var match = pattern.Match(raw);

            while (match.Success)
            {
                if (match.Index > 0) targetText.Append(raw.Substring(0, match.Index));

                // Remove the part before the found code block, and the code block, from the remaining 
                // source text
                raw = raw.Substring(match.Index + match.Length);

                // Get the content of the found code block
                string content = match.Value;

                // The RegEx match still contains the opening and closing tags. Remove them so we get only the 
                // text within the tag.
                int openingTagLen = openingTag.Length;
                int closingTagLen = closingTag.Length;
                int contentLen = content.Length - closingTagLen - openingTagLen;
                content = content.Substring(openingTagLen, contentLen);

                if (!String.IsNullOrWhiteSpace(content))
                {
                    var closings = FindAnySingleClosingTags(ref content);

                    // Add an opening "pre" tag with a language ("brush") definition...
                    targetText.AppendFormat(
                        "<blockquote style=\"display: block; background: #F4F5F7; border: 1px dashed #CCC; margin: 5px 10px 10px 20px; padding: 8px 12px 8px 10px\"> \n");
                    // ... the content...
                    targetText.Append(content);
                    // ... and a closing tag.
                    targetText.Append("</blockquote>");

                    closings.ForEach(x =>
                        {
                            targetText.Append(x);
                        });
                }

                // Get the next code block.
                match = pattern.Match(raw);
            }

            // Append rest of source text to target.
            targetText.Append(raw);

            return targetText.ToString();
        }

        public List<string> FindAnySingleClosingTags(ref string content)
        {
            var result = new List<string>();
            var seenBefore = new List<string>();

            if (content.IndexOf("quote}") != NotFound)
                _host.LogEntry("We should not be finding *quote} in here.", LogEntryType.Warning, LogName, this);

            int i = 0;
            while (i < content.Length)
            {
                var lookingAt = content.Substring(i);

                var closingTag = "</";
                if (lookingAt.StartsWith(closingTag))
                {
                    var ending = IndexOfClosingBracket(lookingAt);

                    if (ending != NotFound)
                    {
                        var tagName = lookingAt.Substring(closingTag.Length, ending - closingTag.Length);

                        if (seenBefore.Count > 0 && seenBefore[0] == tagName) seenBefore.RemoveAt(0);
                        else
                        {
                            _host.LogEntry("Found a closing tag of " + tagName, LogEntryType.General, LogName, this);
                            result.Add(string.Format("</{0}>", tagName));

                            //1 is for including the closing bracket
                            content = content.Remove(i, ending+1);
                            continue;
                        }
                    }
                }
                else if (lookingAt.StartsWith("<"))
                {
                    int ending = 0;

                    var spaceEnd = lookingAt.IndexOf(' ');
                    int bracketEnd = IndexOfClosingBracket(lookingAt);

                    if (bracketEnd != NotFound && NextIsNotSpace(spaceEnd))
                    {
                        if (spaceEnd == NotFound) ending = bracketEnd;
                        else if (spaceEnd < bracketEnd) ending = spaceEnd;
                        else ending = bracketEnd;

                        //The 1 is to skip over the opening bracket,
                        //The -1 is to offset the 1
                        var tagName = lookingAt.Substring(1, ending - 1);

                        if (!SelfClosingTag(lookingAt, bracketEnd))
                            seenBefore.Insert(0, tagName); //synonymous to a push

                        i += MoveToEndOfBracket(bracketEnd);
                    }
                }

                ++i;
            }

            return result;
        }

        private static bool NextIsNotSpace(int spaceEnd)
        {
            return spaceEnd == -1 || spaceEnd > 1;
        }

        private static int IndexOfClosingBracket(string lookingAt)
        {
            return lookingAt.IndexOf('>');
        }

        private int MoveToEndOfBracket(int bracketEnd)
        {
            return bracketEnd;
        }

        private bool SelfClosingTag(string content, int bracketEnd)
        {
            //bracketEnd should be the index of the closing bracket
            //so just go back one spot
            return content[bracketEnd - 1] == '/';
        }

        public string PrepareTitle(string title, ContextInformation context)
        {
            return title;
        }

        public void Init(IHostV30 host, string config)
        {
            _host = host;
            _config = config ?? "";

            if (_config.ToLowerInvariant() == "nolog") _enableLogging = false;
        }

        public void Shutdown()
        {
        }

        public ComponentInformation Information
        {
            get
            {
                return new ComponentInformation("Wrapping Quote Block", "Chad Gorshing", "0.0.1", null, null);
            }
        }

        public string ConfigHelpHtml
        {
            get { return "Specify <i>nolog</i> for disabling warning log messages for non-existent files or attachments."; }
        }
    }
}
