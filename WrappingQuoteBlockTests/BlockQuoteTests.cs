using BlockQuoteWithFormatting;
using NUnit.Framework;
using ScrewTurn.Wiki.PluginFramework;
using Rhino.Mocks;

namespace WrappingQuoteBlockTests
{
    [TestFixture]
    public class BlockQuoteTests
    {
        private BlockQuote _blockQuote;
        private MockRepository _mocks;
        private IHostV30 _mockedHost;

        [SetUp]
        public void SetUp()
        {
            _mocks = new MockRepository();

            _mockedHost = _mocks.DynamicMock<IHostV30>();

            _blockQuote = new BlockQuote();

            _blockQuote.Init(_mockedHost, string.Empty);
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

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithOutTags()
        {
            const string raw = "some text";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithOtherTags()
        {
            const string raw = "@@some text@@";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatDoesNotAlterCopyWithMultipleTags()
        {
            const string raw = "@@some text@@ and then shortly thereafter, ''some'' other tag(s)";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(raw));
        }

        [Test]
        public void FormatReplacesTagsWithNoContent()
        {
            const string raw = "{quote}{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FormatReplacesTagsWithSingleWordContent()
        {
            const string word = "Bocephus";
            const string raw = "{quote}" + word + "{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent(word)));
        }

        [Test]
        public void FormatReplacesContentButNotOtherTags()
        {
            const string sentence = "''Bocephus is the man!''";
            const string raw = "{quote}" + sentence + "{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent(sentence)));
        }

        [Test]
        public void FormatContinuesPastParagraphs()
        {
            const string paragraph =
                "This will be on the first line\nThis will continue on to the second\nAnd finally the third or last line.";
            const string raw = "{quote}" + paragraph + "{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent(paragraph)));
        }

        [Test]
        public void FormatHandlesDoubleTags()
        {
            const string raw = "Some lady said {quote}hello{/quote} then she said {quote}good bye{/quote}.";

            string result = CallFormat(raw);

            var shouldBe = "Some lady said " + TagsWithContent("hello") + " then she said " +
                           TagsWithContent("good bye") + ".";
            Assert.That(result, Is.EqualTo(shouldBe));
        }

        [Test]
        public void FormatHandlesBrokenApartTags()
        {
            //string raw = "* One\n{quote}foo\n\nfoo2\n{/quote}";
            //string raw = "<ul><li>One<br />" + TagsWithContent("foo<br /></li></ul><br />foo2");
            string raw = "<ul><li>One<br />{quote}foo<br /></li></ul><br />foo2 {/quote}";

            string result = CallFormat(raw);

            var shouldBe = "<ul><li>One<br />" + TagsWithContent("foo<br /><br />foo2 ") + "</li></ul>";
            Assert.That(result, Is.EqualTo(shouldBe));
        }

        [Test]
        public void FormatMovesToEnd()
        {
            string raw = "{quote}Some</b> lady{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("Some lady") + "</b>"));
        }

        [Test]
        public void FormatDoesNothingWhenTagsMatchUp()
        {
            string raw = "{quote}Some <b>lady</b>{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("Some <b>lady</b>")));
        }

        [Test]
        public void FormatDoesNothingWhenNestedCorrectly()
        {
            string raw = "{quote}Some <b>la<i>d</i>y</b>{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("Some <b>la<i>d</i>y</b>")));
        }

        [Test]
        public void FormatHandlesAttributes()
        {
            string raw = "{quote}Some <a href=\"foo.html\" id=\"x9\">lady</a>{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("Some <a href=\"foo.html\" id=\"x9\">lady</a>")));
        }

        [Test]
        public void FormatDoesNothingWithLessThan()
        {
            string raw = "{quote}Did you know that 1 < 2 == true?{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("Did you know that 1 < 2 == true?")));
        }

        [Test]
        public void FormatDoesNothingWithHalfClosingTag()
        {
            string raw = "{quote}foo </{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo </")));
        }

        [Test]
        public void FormatDoesNothingWithHalfOpenedTag()
        {
            string raw = "{quote}foo <b{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo <b")));
        }

        [Test]
        public void FormatDoesNothingWithSpacedOutOpeningTag()
        {
            string raw = "{quote}foo <div class=\"nav\" {/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo <div class=\"nav\" ")));
        }

        [Test]
        public void Foo()
        {
            string raw = "{quote}foo <div> {/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo <div> ")));
        }

        [Test]
        public void FormatDoesNothingWithHalfOpenedTag2()
        {
            string raw = "{quote}foo <b {/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo <b ")));
        }

        [Test]
        public void FormatHandlesSelfClosingTags()
        {
            string raw = "{quote}foo<br /></li></ul><br />foo2 {/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo<br /><br />foo2 ") + "</li></ul>"));
        }

        [Test]
        public void FormatDoesNothingWithHtmlComments()
        {
            string raw = "{quote}foo <!-- shhhh -->{/quote}";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo(TagsWithContent("foo <!-- shhhh -->")));
        }

        [Ignore]
        [Test]
        public void Format______WhatToDoAboutBrokenHtmlComments()
        {
            string raw = "{quote}foo <!-- shhhh{/quote} another -->";

            string result = CallFormat(raw);

            Assert.That(result, Is.EqualTo("Fix this, I'm not sure what to do"));
        }

        private string CallFormat(string data)
        {
            return _blockQuote.Format(data, BuildContext(), FormattingPhase.Phase2);
        }
    }
}
