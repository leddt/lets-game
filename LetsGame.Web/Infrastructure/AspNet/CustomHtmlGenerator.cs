using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace LetsGame.Web.Infrastructure.AspNet
{
    public class CustomHtmlGenerator : DefaultHtmlGenerator
    {
        public CustomHtmlGenerator(IAntiforgery antiforgery, IOptions<MvcViewOptions> optionsAccessor, IModelMetadataProvider metadataProvider, IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, ValidationHtmlAttributeProvider validationAttributeProvider) : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
        {
        }

        protected override TagBuilder GenerateInput(
            ViewContext viewContext, 
            InputType inputType, 
            ModelExplorer modelExplorer, 
            string expression,
            object value, 
            bool useViewData, 
            bool isChecked, 
            bool setId, 
            bool isExplicitValue, 
            string format,
            IDictionary<string, object> htmlAttributes)
        {
            var tagBuilder = base.GenerateInput(viewContext, inputType, modelExplorer, expression, value, useViewData, isChecked, setId, isExplicitValue, format, htmlAttributes);
            
            AddInvalidClass(viewContext, expression, tagBuilder);

            return tagBuilder;
        }

        public override TagBuilder GenerateSelect(
            ViewContext viewContext, 
            ModelExplorer modelExplorer, 
            string optionLabel, 
            string expression,
            IEnumerable<SelectListItem> selectList, 
            ICollection<string> currentValues, 
            bool allowMultiple, 
            object htmlAttributes)
        {
            var tagBuilder = base.GenerateSelect(viewContext, modelExplorer, optionLabel, expression, selectList, currentValues, allowMultiple, htmlAttributes);

            AddInvalidClass(viewContext, expression, tagBuilder);

            return tagBuilder;
        }

        public override TagBuilder GenerateTextArea(
            ViewContext viewContext, 
            ModelExplorer modelExplorer, 
            string expression, 
            int rows,
            int columns, 
            object htmlAttributes)
        {
            var tagBuilder = base.GenerateTextArea(viewContext, modelExplorer, expression, rows, columns, htmlAttributes);
            
            AddInvalidClass(viewContext, expression, tagBuilder);

            return tagBuilder;
        }

        private static void AddInvalidClass(ViewContext viewContext, string expression, TagBuilder tagBuilder)
        {
            if (viewContext.ViewData.ModelState.TryGetValue(expression, out var entry) &&
                entry.ValidationState == ModelValidationState.Invalid)
            {
                tagBuilder.AddCssClass("is-invalid");
            }
        }
    }
}