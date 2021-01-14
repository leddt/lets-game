using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement("form-group", Attributes = "for", TagStructure = TagStructure.WithoutEndTag)]
    public class FormGroupTagHelper : TagHelper
    {
        private const string LabelAttributePrefix = "label-";
        private const string ControlAttributePrefix = "control-";
        private const string ValidationAttributePrefix = "val-";
        
        private readonly IHtmlGenerator _generator;

        public ModelExpression For { get; set; }
        
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public FormGroupTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("form-group", HtmlEncoder.Default);
            
            CreateLabel(context, output);
            CreateControl(context, output);
            CreateValidation(context, output);
        }

        private void CreateLabel(TagHelperContext context, TagHelperOutput output)
        {
            var tag = CreateTag("label");
            
            ApplyAttributes(context, output, tag, LabelAttributePrefix);

            new LabelTagHelper(_generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);

            output.Content.AppendHtml(tag);
        }

        private void CreateControl(TagHelperContext context, TagHelperOutput output)
        {
            var tag = CreateTag("input");
            
            ApplyAttributes(context, output, tag, ControlAttributePrefix);
            tag.AddClass("form-control", HtmlEncoder.Default);

            new InputTagHelper(_generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);

            output.Content.AppendHtml(tag);
        }

        private void CreateValidation(TagHelperContext context, TagHelperOutput output)
        {
            var tag = CreateTag("span");
            
            ApplyAttributes(context, output, tag, ValidationAttributePrefix);
            tag.AddClass("invalid-feedback", HtmlEncoder.Default);

            new ValidationMessageTagHelper(_generator) {For = For, ViewContext = ViewContext}.ProcessAsync(context, tag);
            
            output.Content.AppendHtml(tag);
        }

        private TagHelperOutput CreateTag(string name) => new(
            name,
            new TagHelperAttributeList(),
            (b, encoder) => Task.FromResult((TagHelperContent) new DefaultTagHelperContent()));

        private static void ApplyAttributes(TagHelperContext context, TagHelperOutput output, TagHelperOutput tag, string prefix)
        {
            foreach (var attr in context.AllAttributes.Where(x => x.Name.StartsWith(prefix)))
            {
                output.Attributes.Remove(attr);
                tag.Attributes.SetAttribute(attr.Name.Substring(prefix.Length), attr.Value);
            }
        }
    }
}