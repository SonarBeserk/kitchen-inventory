// MIT License
//
// Copyright (c) 2024 SonarBeserk
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.TagHelpers;

/// <summary>
///  Adds an "active" class to the given element when the route parameters match. An Active Page Tag Helper for use with Razor Pages.
/// See https://gist.github.com/DanElliott/32787b4ae1941780d70cb085d55f8b24
/// </summary>
[HtmlTargetElement(Attributes = "is-active-page")]
public class ActivePageTagHelper : TagHelper
{
    /// <summary>The name of the action method.</summary>
    /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
    [HtmlAttributeName("asp-page")]
    public string Page { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.Rendering.ViewContext" /> for the current request.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        if (ShouldBeActive())
        {
            MakeActive(output);
        }

        output.Attributes.RemoveAll("is-active-page");
    }

    private bool ShouldBeActive()
    {
        string currentPage = ViewContext.RouteData.Values["Page"].ToString();

        if (!string.IsNullOrWhiteSpace(Page) && Page.ToLower() != currentPage.ToLower())
        {
            return false;
        }

        return true;
    }

    private void MakeActive(TagHelperOutput output)
    {
        var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
        if (classAttr == null)
        {
            classAttr = new TagHelperAttribute("class", "active");
            output.Attributes.Add(classAttr);
        }
        else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("active") < 0)
        {
            output.Attributes.SetAttribute("class", classAttr.Value == null
                ? "active"
                : classAttr.Value.ToString() + " active");
        }
    }
}
