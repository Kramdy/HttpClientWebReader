/*
HTTP
1. Переписати код застосунку GetWebPage, використовуючи клас HttpClient.
2. Додати можливість прочитати Headers і StatusCode відповіді HttpResponseMessage.
3. Додати можливість задавати заголовки запиту HttpClient.DefaultRequestHeaders.
*/
using System.Net.Http;

namespace HttpClientWebReader;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new WebReaderForm());
    }
}

public sealed class WebReaderForm : Form
{
    private readonly TextBox urlBox = new() { Text = "https://learn.microsoft.com/", Dock = DockStyle.Fill };
    private readonly TextBox headerBox = new() { PlaceholderText = "Header: Value (one per line)", Multiline = true, Dock = DockStyle.Fill };
    private readonly Button loadButton = new() { Text = "Load page", Dock = DockStyle.Fill };
    private readonly Label statusLabel = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private readonly TextBox responseHeaders = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox responseBody = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, WordWrap = false };
    private readonly HttpClient client = new();

    public WebReaderForm()
    {
        Text = "HTTP Client"; ClientSize = new Size(900, 620);
        TableLayoutPanel top = new() { Dock = DockStyle.Top, Height = 105, ColumnCount = 2, RowCount = 3 };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85));
        top.Controls.Add(new Label { Text = "URL:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0); top.Controls.Add(urlBox, 1, 0);
        top.Controls.Add(new Label { Text = "Request headers:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 1); top.Controls.Add(headerBox, 1, 1);
        top.Controls.Add(loadButton, 1, 2);
        SplitContainer split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 180 };
        split.Panel1.Controls.Add(responseHeaders); split.Panel1.Controls.Add(statusLabel); statusLabel.Dock = DockStyle.Top;
        split.Panel2.Controls.Add(responseBody);
        Controls.Add(split); Controls.Add(top); loadButton.Click += async (_, _) => await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        if (!Uri.TryCreate(urlBox.Text.Trim(), UriKind.Absolute, out Uri? address)) { statusLabel.Text = "Enter a valid URL."; return; }
        loadButton.Enabled = false; responseHeaders.Clear(); responseBody.Clear(); client.DefaultRequestHeaders.Clear();
        try
        {
            foreach (string line in headerBox.Lines.Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                string[] pair = line.Split(':', 2);
                if (pair.Length == 2) client.DefaultRequestHeaders.TryAddWithoutValidation(pair[0].Trim(), pair[1].Trim());
            }
            using HttpResponseMessage response = await client.GetAsync(address);
            statusLabel.Text = $"StatusCode: {(int)response.StatusCode} ({response.StatusCode})";
            responseHeaders.Text = string.Join(Environment.NewLine, response.Headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")) + Environment.NewLine + string.Join(Environment.NewLine, response.Content.Headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}"));
            responseBody.Text = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException error) { statusLabel.Text = "Request error: " + error.Message; }
        finally { loadButton.Enabled = true; }
    }
    protected override void Dispose(bool disposing) { if (disposing) client.Dispose(); base.Dispose(disposing); }
}
