namespace Test.Unit.Core.Domain.Models
{
    using System;
    using CompanyName.Notebook.NoteTaking.Core.Domain.Models;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class CategoryTester
    {
        [Test]
        public void CanCreateCategoryWithNoInput()
        {
            // ARRANGE

            // ACT
            var subjectUnderTest = new Category();

            // ASSERT
            Assert.That(subjectUnderTest, Is.TypeOf(typeof(Category)));
            Assert.That(subjectUnderTest, Is.InstanceOf(typeof(ICategory)));
        }

        [Test]
        public void CanCreateCategory()
        {
            // ARRANGE
            string expectedName = "Verbose Notes";

            // ACT
            var subjectUnderTest = new Category(expectedName);

            // ASSERT
            Assert.That(subjectUnderTest, Is.TypeOf(typeof(Category)));
            Assert.That(subjectUnderTest, Is.InstanceOf(typeof(ICategory)));
            Assert.That(subjectUnderTest.Name, Is.EqualTo(expectedName));
            Assert.That(subjectUnderTest.Created, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }
    }
}
