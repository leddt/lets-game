using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LetsGame.Web.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement("avatar")]
    public class AvatarTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private IDictionary<string, string> _routeValues;

        public AvatarTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public string Name { get; set; }
        public bool IsButton { get; set; }
        public bool IsLink => !string.IsNullOrWhiteSpace(Page);
        public bool IsAdd { get; set; }
        public bool IsRemove { get; set; }
        public bool IsLarge { get; set; }
        
        [HtmlAttributeName("asp-page")] public string Page { get; set; }
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get => _routeValues ??= new Dictionary<string, string>();
            set => _routeValues = value;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("avatar", HtmlEncoder.Default);
            output.Attributes.SetAttribute("title", Name);

            if (IsLarge)
            {
                output.AddClass("avatar-large", HtmlEncoder.Default);
            }

            if (IsLink)
            {
                output.TagName = "a";
                
                var actionContext = _actionContextAccessor.ActionContext;
                var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                var url = urlHelper.Page(Page, RouteValues);

                output.Attributes.SetAttribute("href", url);
            }
            else if (IsButton)
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