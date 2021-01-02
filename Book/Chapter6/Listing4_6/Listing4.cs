using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Book.Chapter6.Listing4_6
{
    public class CustomerControllerTests
    {
        [Fact]
        public void Adding_a_comment_to_an_article()
        {
            var sut = new Article();
            var text = "Comment text";
            var author = "John Doe";
            var now = new DateTime(2019, 4, 1);

            sut.AddComment(text, author, now);

            Assert.Equal(1, sut.Comments.Count);
            Assert.Equal(text, sut.Comments[0].Text);
            Assert.Equal(author, sut.Comments[0].Author);
            Assert.Equal(now, sut.Comments[0].DateCreated);
        }

        [Fact]
        public void Adding_a_comment_to_an_article2()
        {
            var sut = new Article();
            var text = "Comment text";
            var author = "John Doe";
            var now = new DateTime(2019, 4, 1);

            sut.AddComment(text, author, now);

            sut.ShouldContainNumberOfComments(1)
                .WithComment(text, author, now);
        }

        [Fact]
        public void Adding_a_comment_to_an_article3()
        {
            var sut = new Article();
            var comment = new Comment(
                "Comment text",
                "John Doe",
                new DateTime(2019, 4, 1));

            sut.AddComment(comment.Text, comment.Author, comment.DateCreated);

            sut.Comments.Should()
                .BeEquivalentTo(comment);
        }
    }

    public class Article
    {
        private readonly List<Comment> _comments = new List<Comment>();

        public IReadOnlyList<Comment> Comments =>
            _comments.ToList();

        public void AddComment(string text, string author, DateTime now)
        {
            _comments.Add(new Comment(text, author, now));
        }

        public Article ShouldContainNumberOfComments(int i)
        {
            return this;
        }
    }

    public class Comment
    {
        public readonly string Text;
        public readonly string Author;
        public readonly DateTime DateCreated;

        public Comment(string text, string author, DateTime dateCreated)
        {
            Text = text;
            Author = author;
            DateCreated = dateCreated;
        }

        protected bool Equals(Comment other)
        {
            return string.Equals(Text, other.Text)
                && string.Equals(Author, other.Author)
                && DateCreated.Equals(other.DateCreated);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Comment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Author != null ? Author.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DateCreated.GetHashCode();
                return hashCode;
            }
        }
    }

    public static class ArticleExtensions
    {
        public static Article ShouldContainNumberOfComments(this Article article, int commentCount)
        {
            Assert.Equal(1, article.Comments.Count);
            return article;
        }

        public static Article WithComment(this Article article, string text, string author, DateTime dateCreated)
        {
            Comment comment = article.Comments.SingleOrDefault(x => x.Text == text && x.Author == author && x.DateCreated == dateCreated);
            Assert.NotNull(comment);
            return article;
        }
    }
}
