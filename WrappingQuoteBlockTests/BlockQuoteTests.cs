using BlockQuoteWithFormatting;
using NUnit.Framework;
using ScrewTurn.Wiki.PluginFramework;

namespace WrappingQuoteBlockTests
{
    [TestFixture]
    public class BlockQuoteTests
    {
        private BlockQuote _blockQuote;

        [SetUp]
        public void SetUp()
        {
            _blockQuote = new BlockQuote();
        }

        [TearDown]
        public void TearDown()
        {
        }

        private ContextInformation BuildContext()
        {
            return new ContextInformation(false, false, FormattingContext.PageContent, null, null,
                                                            null, null, null);
        }

        private string TagsWithContent(string data)
        {
            return "<blockquote style=\"display: block; background: #F4F5F7; border: 1px dashed #CCC; margin: 5px 10px 10px 20px; padding: 8px 12px 8px 10px\"> \n" + data + "</blockquote>";
        }

        [Test]
        public void FormatDoesNothingWhenPhaseIsNotPageContent()
        {
            const string raw = "@@some text@@ and then shortly thereafter, ''some'' other tag(s)";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithOutTags()
        {
            const string raw = "some text";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithOtherTags()
        {
            const string raw = "@@some text@@";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithMultipleTags()
        {
            const string raw = "@@some text@@ and then shortly thereafter, ''some'' other tag(s)";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatReplacesTagsWithNoContent()
        {
            const string raw = "{quote}{/quote}";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FormatReplacesTagsWithSingleWordContent()
        {
            const string word = "Bocephus";
            const string raw = "{quote}" + word + "{/quote}";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(TagsWithContent(word)));
        }

        [Test]
        public void FormatReplacesContentButNotOtherTags()
        {
            const string sentence = "''Bocephus is the man!''";
            const string raw = "{quote}" + sentence + "{/quote}";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(TagsWithContent(sentence)));
        }

        [Test]
        public void FormatContinuesPastParagraphs()
        {
            const string paragraph =
                "This will be on the first line\nThis will continue on to the second\nAnd finally the third or last line.";
            const string raw = "{quote}" + paragraph + "{/quote}";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            Assert.That(result, Is.EqualTo(TagsWithContent(paragraph)));
        }

        [Test]
        public void FormatHandlesDoubleTags()
        {
            const string raw = "Some lady said {quote}hello{/quote} then she said {quote}good bye{/quote}.";

            string result = _blockQuote.Format(raw, BuildContext(), FormattingPhase.Phase2);

            var shouldBe = "Some lady said " + TagsWithContent("hello") + " then she said " +
                           TagsWithContent("good bye") + ".";
            Assert.That(result, Is.EqualTo(shouldBe));
        }
    }
}
