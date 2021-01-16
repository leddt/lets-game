using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement("game-card")]
    public class GameCardTagHelper : TagHelper
    {
        private const string PlaceholderImage =
            "data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' width='555' height='312'><rect width='100%25' height='100%25' fill='%23868e96'></rect></svg>";
        
        public string Name { get; set; }
        public string ImageId { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            
            output.AddClass("card", HtmlEncoder.Default);

            var slot = await output.GetChildContentAsync();
            
            var img = CreateImage();
            var overlay = CreateOverlay(slot);

            output.Content.Clear();
            output.Content.AppendHtml(img);
            output.Content.AppendHtml(overlay);
        }

        private TagHelperOutput CreateImage()
        {
            var src = string.IsNullOrWhiteSpace(ImageId)
                ? PlaceholderImage
                : Image.GetScreenshotMedUrl(ImageId);

            var alt = string.IsNullOrWhiteSpace(ImageId)
                ? ""
                : $"Screenshot from {Name}";

            var img = CreateTag("img");
            img.AddClass("card-img", HtmlEncoder.Default);
            img.Attributes.SetAttribute("src", src);
            img.Attributes.SetAttribute("alt", alt);

            return img;
        }

        private TagHelperOutput CreateOverlay(TagHelperContent slot)
        {
            var container = CreateTag("div");
            container.AddClass("card-img-overlay", HtmlEncoder.Default); 
            container.AddClass("bg-overlay-dark", HtmlEncoder.Default);
            container.AddClass("text-white", HtmlEncoder.Default);

            var title = CreateTag("h5");
            title.AddClass("card-title", HtmlEncoder.Default); 
            title.AddClass("text-shadow", HtmlEncoder.Default);
            title.Content.Append(Name);

            container.Content.AppendHtml(title);
            container.Content.AppendHtml(slot);

            return container;
        }

        private TagHelperOutput CreateTag(string name) => new(
            name,
            new TagHelperAttributeList(),
            (b, encoder) => Task.FromResult((TagHelperContent) new DefaultTagHelperContent()));
    }
}