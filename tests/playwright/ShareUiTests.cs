using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ShareUiTests : PageTest
{
    [Test]
    public async Task Post_Share_Button_Uses_X_Branding_And_Twitter_Intent()
    {
        const string postUrl = "http://127.0.0.1:4000/2022/10/autofaker.html";
        const string expectedPathData = "M14.234 10.162 22.977 0h-2.072l-7.591 8.824L7.251 0H.258l9.168 13.343L.258 24H2.33l8.016-9.318L16.749 24h6.993zm-2.837 3.299-.929-1.329L3.076 1.56h3.182l5.965 8.532.929 1.329 7.754 11.09h-3.182z";

        await Page.GotoAsync(postUrl);

        var shareLink = Page.GetByRole(AriaRole.Link, new() { NameString = "Share on X", Exact = true });

        Assert.That(await shareLink.CountAsync(), Is.EqualTo(1));
        Assert.That(await shareLink.IsVisibleAsync(), Is.True);

        var title = await shareLink.GetAttributeAsync("title");
        var ariaLabel = await shareLink.GetAttributeAsync("aria-label");
        var href = await shareLink.GetAttributeAsync("href");
        var iconPath = Page.Locator("li.twitter svg path");
        var iconPathCount = await iconPath.CountAsync();
        var iconPathData = await iconPath.GetAttributeAsync("d");

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("Share on X"));
            Assert.That(ariaLabel, Is.EqualTo("Share on X"));
            Assert.That(href, Does.StartWith("https://twitter.com/intent/tweet?"));
            Assert.That(href, Does.Contain("url=https://christianhelle.com/2022/10/autofaker.html"));
            Assert.That(href, Does.Contain("via=christianhelle"));
            Assert.That(href, Does.Contain("related=christianhelle"));
            Assert.That(iconPathCount, Is.EqualTo(1));
            Assert.That(iconPathData, Is.EqualTo(expectedPathData));
        });
    }

    [Test]
    public async Task Post_Share_Icons_Render_At_The_Same_Size()
    {
        const string postUrl = "http://127.0.0.1:4000/2022/10/autofaker.html";

        await Page.GotoAsync(postUrl);

        var twitterIcon = Page.Locator("li.twitter svg");
        var linkedinIcon = Page.Locator("li.linkedin svg");
        var facebookIcon = Page.Locator("li.facebook svg");

        var twitterVisible = await twitterIcon.IsVisibleAsync();
        var linkedinVisible = await linkedinIcon.IsVisibleAsync();
        var facebookVisible = await facebookIcon.IsVisibleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(twitterVisible, Is.True);
            Assert.That(linkedinVisible, Is.True);
            Assert.That(facebookVisible, Is.True);
        });

        var twitterBox = await twitterIcon.BoundingBoxAsync();
        var linkedinBox = await linkedinIcon.BoundingBoxAsync();
        var facebookBox = await facebookIcon.BoundingBoxAsync();

        Assert.That(twitterBox, Is.Not.Null);
        Assert.That(linkedinBox, Is.Not.Null);
        Assert.That(facebookBox, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(twitterBox!.Width, Is.EqualTo(40).Within(0.5));
            Assert.That(twitterBox.Height, Is.EqualTo(40).Within(0.5));
            Assert.That(linkedinBox!.Width, Is.EqualTo(twitterBox.Width).Within(0.5));
            Assert.That(linkedinBox.Height, Is.EqualTo(twitterBox.Height).Within(0.5));
            Assert.That(facebookBox!.Width, Is.EqualTo(twitterBox.Width).Within(0.5));
            Assert.That(facebookBox.Height, Is.EqualTo(twitterBox.Height).Within(0.5));
        });
    }
}
