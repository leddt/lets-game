using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LetsGame.Web.Infrastructure.AspNet.TagHelpers
{
    [HtmlTargetElement(Attributes = "is-active-class")]
    [HtmlTargetElement(Attributes = "is-inactive-class")]
    public class ActiveClassTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public ActiveClassTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        private IDictionary<string, string> _routeValues;

        [HtmlAttributeName("asp-page")] public string Page { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get => _routeValues ??= new Dictionary<string, string>();
            set => _routeValues = value;
        }


        [HtmlAttributeName("is-active-class")] public string ActiveClass { get; set; }

        [HtmlAttributeName("is-inactive-class")]
        public string InactiveClass { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive())
                MakeActive(output);
            else
                MakeInactive(output);

            output.Attributes.RemoveAll("is-active-class");
            output.Attributes.RemoveAll("is-inactive-class");
        }

        private bool ShouldBeActive()
        {
            var actionContext = _actionContextAccessor.ActionContext;
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var expectedUrl = urlHelper.Page(Page, RouteValues);

            return actionContext.HttpContext.Request.Path.Equals(expectedUrl, StringComparison.OrdinalIgnoreCase);
        }

        private void MakeActive(TagHelperOutput output) => AddClass(output, ActiveClass);
        private void MakeInactive(TagHelperOutput output) => AddClass(output, InactiveClass);

        private void AddClass(TagHelperOutput output, string classToAdd)
        {
            if (string.IsNullOrWhiteSpace(classToAdd)) return;

            var classes =
                (output.Attributes
                    .FirstOrDefault(a => a.Name == "class")?
                    .Value?
                    .ToString() ?? "")
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Append(classToAdd)
                .Distinct();

            output.Attributes.SetAttribute("class", string.Join(' ', classes));
        }
    }
}