using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Book.Chapter4.Listing1
{
    public class MessageRendererTests
    {
        [Fact]
        public void Rendering_a_message()
        {
            var sut = new MessageRenderer();
            var message = new Message
            {
                Header = "h",
                Body = "b",
                Footer = "f"
            };

            string html = sut.Render(message);

            Assert.Equal("<h1>h</h1><b>b</b><i>f</i>", html);
        }

        [Fact]
        public void MessageRenderer_uses_correct_sub_renderers()
        {
            var sut = new MessageRenderer();

            IReadOnlyList<IRenderer> renderers = sut.SubRenderers;

            Assert.Equal(3, renderers.Count);
            Assert.IsAssignableFrom<HeaderRenderer>(renderers[0]);
            Assert.IsAssignableFrom<BodyRenderer>(renderers[1]);
            Assert.IsAssignableFrom<FooterRenderer>(renderers[2]);
        }

        [Fact(Skip = "Example of how not to write tests")]
        public void MessageRenderer_is_implemented_correctly()
        {
            string sourceCode = File.ReadAllText(@"<project path>\MessageRenderer.cs");

            Assert.Equal(
                @"
public class MessageRenderer : IRenderer
{
    public IReadOnlyList<IRenderer> SubRenderers { get; }

    public MessageRenderer()
    {
        SubRenderers = new List<IRenderer>
        {
            new HeaderRenderer(),
            new BodyRenderer(),
            new FooterRenderer()
        };
    }

    public string Render(Message message)
    {
        return SubRenderers
            .Select(x => x.Render(message))
            .Aggregate("", (str1, str2) => str1 + str2);
    }
}", sourceCode);
        }
    }

    public class Message
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }
    }

    public interface IRenderer
    {
        string Render(Message message);
    }

    public class MessageRenderer : IRenderer
    {
        public IReadOnlyList<IRenderer> SubRenderers { get; }

        public MessageRenderer()
        {
            SubRenderers = new List<IRenderer>
            {
                new HeaderRenderer(),
                new BodyRenderer(),
                new FooterRenderer()
            };
        }

        public string Render(Message message)
        {
            return SubRenderers
                .Select(x => x.Render(message))
                .Aggregate("", (str1, str2) => str1 + str2);
        }
    }

    public class FooterRenderer : IRenderer
    {
        public string Render(Message message)
        {
            return $"<i>{message.Footer}</i>";
        }
    }

    public class BodyRenderer : IRenderer
    {
        public string Render(Message message)
        {
            return $"<b>{message.Body}</b>";
        }
    }

    public class HeaderRenderer : IRenderer
    {
        public string Render(Message message)
        {
            return $"<h1>{message.Header}</h1>";
        }
    }
}
