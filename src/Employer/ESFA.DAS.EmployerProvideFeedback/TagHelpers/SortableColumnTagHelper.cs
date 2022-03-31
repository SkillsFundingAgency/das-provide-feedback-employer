using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.TagHelpers
{
    [HtmlTargetElement(TagName)]
    public class SfaSortableColumnHeaderTagHelper : TagHelper
    {
        private const string TagName = "sfa-sortable-column-header";

        private const string AspActionName = "asp-action";
        private const string AspControllerName = "asp-controller";
        private const string AspRouteValuesDictionaryName = "asp-all-route-data";
        private const string AspRouteValuesPrefix = "asp-route-";
        private const string AspHostName = "asp-host";
        private const string AspProtocolName = "asp-protocol";
        private const string AspFragmentName = "asp-fragment";

        private const string SfaSortColumnName = "sfa-sort-column";
        private const string SfaSortDirectionName = "sfa-sort-direction";

        private const string SfaTableSortColumnName = "sfa-table-sort-column";
        private const string DataSortDirectionName = "data-SortDirection";

        private IDictionary<string, string> _routeValues;
        private readonly IHtmlGenerator _generator;

        public SfaSortableColumnHeaderTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(AspActionName)]
        public string AspAction { get; set; }

        [HtmlAttributeName(AspControllerName)]
        public string AspController { get; set; }

        [HtmlAttributeName(AspRouteValuesDictionaryName, DictionaryAttributePrefix = AspRouteValuesPrefix)]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (_routeValues == null)
                {
                    _routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return _routeValues;
            }
            set
            {
                _routeValues = value;
            }
        }

        [HtmlAttributeName(AspHostName)]
        public string AspHost { get; set; }

        [HtmlAttributeName(AspProtocolName)]
        public string AspProtocol { get; set; }

        [HtmlAttributeName(AspFragmentName)]
        public string AspFragment { get; set; }

        [HtmlAttributeName(SfaSortColumnName)]
        public string SfaSortColumn { get; set; }

        [HtmlAttributeName(SfaSortDirectionName)]
        public string SfaSortDirection { get; set; }

        [HtmlAttributeName(SfaTableSortColumnName)]
        public string SfaTableSortColumn { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            using (var writer = new StringWriter())
            {
                if (SfaSortColumn.Equals(SfaTableSortColumn, StringComparison.InvariantCultureIgnoreCase))
                {
                    var sortDirectionClass = SfaSortDirection == SortOrder.Asc
                        ? "sorted-ascending"
                        : "sorted-descending";

                    var classes = output.Attributes.FirstOrDefault(a => a.Name == "class")?.Value.ToString();

                    output.Attributes.SetAttribute("class",
                        string.IsNullOrEmpty(classes)
                            ? $"{sortDirectionClass}"
                            : $"{sortDirectionClass} {classes}");
                }

                output.Attributes.Add(DataSortDirectionName, new HtmlString(ToogleSortDirection()));

                var content = (await output.GetChildContentAsync()).GetContent();

                var routeValues = _routeValues?.Count > 0 ? new RouteValueDictionary(_routeValues) : new RouteValueDictionary();
                routeValues["SortColumn"] = SfaSortColumn;
                routeValues["SortDirection"] = ToogleSortDirection();

                var link = _generator.GenerateActionLink(ViewContext, content,
                    AspAction, AspController, AspProtocol, AspHost, AspFragment,
                    routeValues,
                    GetPassThroughAttributes(output));

                link.WriteTo(writer, NullHtmlEncoder.Default);

                output.TagName = null;
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.Clear();
                output.PostContent.SetHtmlContent(writer.ToString());
            }
        }

        private string ToogleSortDirection()
        {
            return (SfaSortColumn == SfaTableSortColumn) ? SortOrder.Toggle(SfaSortDirection) : SfaSortDirection;
        }

        private Dictionary<string, object> GetPassThroughAttributes(TagHelperOutput output)
        {
            var passThroughAttributes = new Dictionary<string, object>();
            foreach (var attribute in output.Attributes)
            {
                passThroughAttributes[attribute.Name] = attribute.Value;
            }
            return passThroughAttributes;
        }
    }

    public static class SortOrder
    {
        public const string Asc = "Asc";
        public const string Desc = "Desc";

        public static string Toggle(string sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortOrder))
                return Asc;
            if(sortOrder.Equals(Asc, StringComparison.InvariantCultureIgnoreCase))
            {
                return Desc;
            }
            else
            {
                return Asc;
            }
        }
    }
}
