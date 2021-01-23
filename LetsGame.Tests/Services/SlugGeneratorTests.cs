using System.Text;
using System.Threading.Tasks;
using LetsGame.Web.Services;
using Xunit;

namespace LetsGame.Tests.Services
{
    public class SlugGeneratorTests
    {
        public SlugGeneratorTests()
        {
            // Required setup for the removal of accents
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Theory]
        [InlineData("Basic Title", "basic-title")]
        [InlineData("Numbers 123 456 work789", "numbers-123-456-work789")]
        [InlineData("Àccèntéd çhärâctêrs", "accented-characters")]
        [InlineData("$pecial_characters!", "pecial-characters")]
        [InlineData("   white   space   ", "white-space")]
        [InlineData("!_/collapsing -|\\ separators ...,", "collapsing-separators")]
        public async Task GeneratesValidSlugs(string name, string expectedSlug)
        {
            var sut = new SlugGenerator();

            var actualSlug = await sut.GenerateWithCheck(name, _ => Task.FromResult(false));
            
            Assert.Equal(expectedSlug, actualSlug);
        }

        [Fact]
        public async Task HandlesCollisions()
        {
            var sut = new SlugGenerator();

            var actualSlug = await sut.GenerateWithCheck("Duplicate slug", x => Task.FromResult(x == "duplicate-slug"));
            
            Assert.Matches("^duplicate-slug-.{3}$", actualSlug);
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(5, 3)]
        [InlineData(6, 4)]
        [InlineData(10, 4)]
        [InlineData(11, 5)]
        public async Task GrowsSuffixAfterSubsequentCollisions(int failureCount, int suffixLength)
        {
            var sut = new SlugGenerator();

            var actualSlug = await sut.GenerateWithCheck("Duplicate slug", _ => Task.FromResult(failureCount-- > 0));
            
            Assert.Matches($"^duplicate-slug-.{{{suffixLength}}}$", actualSlug);
        }
    }
}