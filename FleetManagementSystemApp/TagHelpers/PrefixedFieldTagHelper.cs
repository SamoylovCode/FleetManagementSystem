using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.TagHelpers;

[HtmlTargetElement("prefixed-field")]
public class PrefixedFieldTagHelper : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression For { get; set; }

    [HtmlAttributeName("prefix")]
    public string Prefix { get; set; }

    [HtmlAttributeName("class")]
    public string Class { get; set; }

    [HtmlAttributeName("id")]
    public string Id { get; set; }

    [HtmlAttributeName("tag")]
    public string TagName { get; set; } = "div";

    [HtmlAttributeName("type")]
    public string Type { get; set; }

    [HtmlAttributeName("value")]
    public string Value { get; set; }

    [HtmlAttributeName("asp-validation-for")]
    public ModelExpression ValidationFor { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        Prefix ??= ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        output.TagName = TagName;

        foreach (var attr in context.AllAttributes)
        {
            if (attr.Name != "asp-for" &&
                attr.Name != "prefix" &&
                attr.Name != "tag" &&
                attr.Name != "type" &&
                attr.Name != "value" &&
                attr.Name != "asp-validation-for")
            {
                output.Attributes.SetAttribute(attr.Name, attr.Value);
            }
        }

        if (TagName == "span" || ValidationFor != null)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("data-valmsg-for", $"{Prefix}.{ValidationFor?.Name ?? For?.Name}");
            output.Attributes.SetAttribute("data-valmsg-replace", "true");
            return;
        }

        if (TagName == "input")
        {
            output.TagMode = TagMode.SelfClosing;
            output.Attributes.SetAttribute("name", $"{Prefix}.{For.Name}");

            var modelType = For.ModelExplorer.ModelType;
            var explicitType = Type?.ToLowerInvariant();

            string inputType = explicitType ?? GetDefaultInputType(modelType);
            string inputValue = GetFormattedValue(modelType, For.Model, inputType);

            output.Attributes.SetAttribute("type", inputType);
            if (!string.IsNullOrEmpty(inputValue))
                output.Attributes.SetAttribute("value", inputValue);

            if (For.Metadata.IsRequired && inputType != "hidden")
            {
                output.Attributes.SetAttribute("data-val", "true");
                var requiredMessage = For.Metadata.ValidatorMetadata
                    .OfType<RequiredAttribute>()
                    .FirstOrDefault()?.ErrorMessage;

                if (!string.IsNullOrEmpty(requiredMessage))
                    output.Attributes.SetAttribute("data-val-required", requiredMessage);
            }
            return;
        }

        if (TagName == "label")
        {
            output.Attributes.SetAttribute("for", $"{Prefix}.{For.Name}");
            output.Content.SetContent(For.Metadata.DisplayName ?? For.Name);
            return;
        }

        output.Content.AppendHtml($"<label for=\"{Prefix}.{For.Name}\">{For.Metadata.DisplayName}</label>");
        output.Content.AppendHtml($"<input name=\"{Prefix}.{For.Name}\" value=\"{For.Model}\" />");
        output.Content.AppendHtml($"<span class=\"error\" data-valmsg-for=\"{Prefix}.{For.Name}\"></span>");
    }

    private static string GetDefaultInputType(Type modelType)
    {
        return modelType switch
        {
            Type t when t == typeof(DateOnly) => "date",
            Type t when t == typeof(TimeOnly) => "time",
            Type t when t == typeof(DateTime) => "datetime-local",
            Type t when t == typeof(bool) => "checkbox",
            _ => "text"
        };
    }

    private static string GetFormattedValue(Type modelType, object model, string inputType)
    {
        if (model == null)
        {
            return null;
        }

        return inputType switch
        {
            "date" when model is DateOnly d => d.ToString("yyyy-MM-dd"),
            "time" when model is TimeOnly t => t.ToString(@"HH\:mm"),
            "datetime-local" when model is DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm"),
            _ => model.ToString()!
        };
    }
}