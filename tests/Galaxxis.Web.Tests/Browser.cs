using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Galaxxis.Web.Tests;

/// <summary>
/// A browser session against the test server: keeps cookies, does not follow redirects (tests check them), and sends
/// forms the way a browser does, including the antiforgery token taken from the page that contains the form.
/// </summary>
public sealed partial class Browser(HttpClient client) : IDisposable
{
    /// <summary>The standard password of test accounts; satisfies the Identity password rules.</summary>
    public const string Password = "Galaxxis-Test-1!";

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    /// <summary>Sends a GET request.</summary>
    /// <param name="url">The address.</param>
    /// <returns>The response.</returns>
    public Task<HttpResponseMessage> GetAsync(string url) => client.GetAsync(url, Cancellation);

    /// <summary>Loads a page that must answer with 200 OK.</summary>
    /// <param name="url">The address.</param>
    /// <returns>The HTML.</returns>
    public async Task<string> GetPageAsync(string url)
    {
        HttpResponseMessage response = await GetAsync(url);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"GET {url} answered {(int)response.StatusCode}.");
        return await response.Content.ReadAsStringAsync(Cancellation);
    }

    /// <summary>
    /// Posts a form like a browser: loads <paramref name="formPage"/> first and sends its antiforgery token along.
    /// </summary>
    /// <param name="url">The form target.</param>
    /// <param name="formPage">The page that contains the form (its token is used).</param>
    /// <param name="fields">The form fields.</param>
    /// <param name="asScript">Whether to send the request like the command script does (expects JSON).</param>
    /// <returns>The response.</returns>
    public async Task<HttpResponseMessage> PostFormAsync(string url, string formPage, IDictionary<string, string>? fields = null, bool asScript = false)
    {
        string token = AntiforgeryToken(await GetPageAsync(formPage));
        Dictionary<string, string> form = new(fields ?? new Dictionary<string, string>()) { ["__RequestVerificationToken"] = token };
        return await PostAsync(url, form, asScript);
    }

    /// <summary>Posts form fields exactly as given, without adding an antiforgery token.</summary>
    /// <param name="url">The form target.</param>
    /// <param name="fields">The form fields.</param>
    /// <param name="asScript">Whether to send the request like the command script does (expects JSON).</param>
    /// <returns>The response.</returns>
    public Task<HttpResponseMessage> PostAsync(string url, IDictionary<string, string> fields, bool asScript = false)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(fields) };
        if (asScript)
        {
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        }

        return client.SendAsync(request, Cancellation);
    }

    /// <summary>Reads a JSON answer.</summary>
    /// <param name="response">The response; must be 200 OK.</param>
    /// <returns>The JSON document.</returns>
    public static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Cancellation);
    }

    /// <summary>Registers a new account (which also signs it in) and returns its name.</summary>
    /// <returns>The account name.</returns>
    public async Task<string> RegisterAsync()
    {
        string userName = "player" + Guid.NewGuid().ToString("N")[..10];
        HttpResponseMessage response = await PostFormAsync("/account/register", "/account/register", new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = $"{userName}@example.com",
            ["Password"] = Password,
            ["ConfirmPassword"] = Password,
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return userName;
    }

    /// <summary>Signs in.</summary>
    /// <param name="userName">Account name or e-mail address.</param>
    /// <param name="password">The password.</param>
    /// <returns>The response: a redirect on success, the login page otherwise.</returns>
    public Task<HttpResponseMessage> LoginAsync(string userName, string password) =>
        PostFormAsync("/account/login", "/account/login", new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Password"] = password,
        });

    /// <summary>Enters a universe with a race and returns the id of the assigned home planet.</summary>
    /// <param name="universeId">The universe.</param>
    /// <returns>The home planet id.</returns>
    public async Task<int> JoinAsync(int universeId)
    {
        HttpResponseMessage response = await PostFormAsync($"/universe/join/{universeId}", $"/universe/join/{universeId}",
            new Dictionary<string, string> { ["race"] = "Dwarf" });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string location = response.Headers.Location!.OriginalString;
        Match planet = PlanetUrl().Match(location);
        Assert.True(planet.Success, $"Unexpected redirect after joining: {location}");
        return int.Parse(planet.Groups[1].Value);
    }

    /// <inheritdoc />
    public void Dispose() => client.Dispose();

    private static string AntiforgeryToken(string html)
    {
        Match match = AntiforgeryTokenField().Match(html);
        Assert.True(match.Success, "The page contains no form with an antiforgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryTokenField();

    [GeneratedRegex(@"^/Planet(?:/Index)?/(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex PlanetUrl();
}
