namespace TemplateTool;

public static class ConnectionBuilder
{
    public static string Build(
        string authType, string url, string appId,
        string redirectUri, string loginPrompt) =>
        $"AuthType={authType};Url={url};AppId={appId};RedirectUri={redirectUri};LoginPrompt={loginPrompt};";
}
