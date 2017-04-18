using NUnit.Framework;

namespace NoteSync.Sony.Tests
{
    [TestFixture]
    public class SonyRepositoryTests
    {
        private SonyRepository _repository;

        [SetUp]
        public void SetupFixture()
        {
            _repository = new SonyRepository(null, null);    
        }

        [Test]
        public void ExtractTags_NoTags_ReturnsEmptyArray()
        {
            // Arrange
            const string line = "This is a sample line";

            // Act
            var tags = _repository.ExtractTags(line);

            // Assert
            Assert.IsEmpty(tags);
        }

        [Test]
        [TestCase("[Tag] A line")]
        [TestCase("[Tag][Tag2] A line")]
        public void ExtractTags_Tags_ReturnsTagArray(string line)
        {
            // Act
            var tags = _repository.ExtractTags(line);

            // Assert
            Assert.IsNotEmpty(tags);
        }

        [Test]
        public void ExtractTags_Tag_ReturnsTagNameWithoutBrackets()
        {
            // Arrange
            const string line = "This has a [tag]";

            // Act
            var tags = _repository.ExtractTags(line);

            // Assert
            Assert.AreEqual("tag", tags[0]);
        }

        [Test]
        public void RemoveTags_Tags_RemovedTags()
        {
            // Arrange
            const string line = "[tag] This has a tag";

            // Act
            var modifiedLine = _repository.RemoveTags(line);

            // Assert
            Assert.AreEqual("This has a tag", modifiedLine);
        }
    }
}
