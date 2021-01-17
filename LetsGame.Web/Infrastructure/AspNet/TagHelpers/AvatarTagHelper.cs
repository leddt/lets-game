using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement("avatar")]
    public class AvatarTagHelper : TagHelper
    {
        public string Name { get; set; }
        public bool IsButton { get; set; }
        public bool IsAdd { get; set; }
        public bool IsRemove { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("avatar", HtmlEncoder.Default);
            output.Attributes.SetAttribute("title", Name);
            
            if (IsButton)
            {
                output.TagName = "button";
            }
            else
            {
                output.TagName = "div";
            }

            if (IsAdd)
            {
                output.AddClass("avatar-action", HtmlEncoder.Default);
                output.Content.AppendHtml(new HtmlString("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"18\" height=\"18\" viewBox=\"0 0 24 24\"><path d=\"M24 10h-10v-10h-4v10h-10v4h10v10h4v-10h10z\"/></svg>"));
            } 
            else if (IsRemove)
            {
                output.AddClass("avatar-action", HtmlEncoder.Default);
                output.Content.AppendHtml(new HtmlString("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"18\" height=\"18\" viewBox=\"0 0 24 24\"><path d=\"M0 10h24v4h-24z\"/></svg>"));
            }
            else
            {
                output.Content.Append(Name.ToInitials(3));   
            }

            return base.ProcessAsync(context, output);
        }
    }
}