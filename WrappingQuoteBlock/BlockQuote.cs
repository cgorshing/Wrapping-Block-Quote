using System;
using System.Text;
using System.Text.RegularExpressions;
using ScrewTurn.Wiki.PluginFramework;

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
                    // Add an opening "pre" tag with a language ("brush") definition...
                    targetText.AppendFormat(
                        "<blockquote style=\"display: block; background: #F4F5F7; border: 1px dashed #CCC; margin: 5px 10px 10px 20px; padding: 8px 12px 8px 10px\"> \n");
                    // ... the content...
                    targetText.Append(content);
                    // ... and a closing tag.
                    targetText.Append("</blockquote>");
                }

			    // Get the next code block.
                match = pattern.Match(raw);
			}

            // Append rest of source text to target.
            targetText.Append(raw);

			return targetText.ToString();
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
