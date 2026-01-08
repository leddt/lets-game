using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement("form-group", Attributes = "for", TagStructure = TagStructure.WithoutEndTag)]
    public class FormGroupTagHelper(IHtmlGenerator generator) : TagHelper
    {
        private const string LabelAttributePrefix = "label-";
        private const string ControlAttributePrefix = "control-";
        private const string ValidationAttributePrefix = "val-";

        public required ModelExpression For { get; set; }
        
        public ControlElement As { get; set; }
        
        [ViewContext]
        [HtmlAttributeNotBound]
        public required ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("form-group", HtmlEncoder.Default);
            
            await CreateLabel(context, output);
            await CreateControl(context, output);
            await CreateValidation(context, output);
        }

        private async Task CreateLabel(TagHelperContext context, TagHelperOutput output)
        {
            var tag = CreateTag("label");
            
            ApplyAttributes(context, output, tag, LabelAttributePrefix);

            await new LabelTagHelper(generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);

            output.Content.AppendHtml(tag);
        }

        private async Task CreateControl(TagHelperContext context, TagHelperOutput output)
        {
            var tag = As switch
            {
                ControlElement.TextArea => CreateTag("textarea"), 
                _ => CreateTag("input")
            };
            
            ApplyAttributes(context, output, tag, ControlAttributePrefix);
            tag.AddClass("form-control", HtmlEncoder.Default);

            if (As == ControlElement.TextArea)
            {
                await new TextAreaTagHelper(generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);
            }
            else
            {
                await new InputTagHelper(generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);
            }

            output.Content.AppendHtml(tag);
        }

        private async Task CreateValidation(TagHelperContext context, TagHelperOutput output)
        {
            var tag = CreateTag("span");
            
            ApplyAttributes(context, output, tag, ValidationAttributePrefix);
            tag.AddClass("invalid-feedback", HtmlEncoder.Default);

            await new ValidationMessageTagHelper(generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);
            
            output.Content.AppendHtml(tag);
        }

        private TagHelperOutput CreateTag(string name) => new(
            name,
            [],
            (b, encoder) => Task.FromResult((TagHelperContent) new DefaultTagHelperContent()));

        private static void ApplyAttributes(TagHelperContext context, TagHelperOutput output, TagHelperOutput tag, string prefix)
        {
            foreach (var attr in context.AllAttributes.Where(x => x.Name.StartsWith(prefix)))
            {
                output.Attributes.Remove(attr);
                tag.Attributes.SetAttribute(attr.Name.Substring(prefix.Length), attr.Value);
            }
        }

        public enum ControlElement
        {
            Input, TextArea
        }
    }
}