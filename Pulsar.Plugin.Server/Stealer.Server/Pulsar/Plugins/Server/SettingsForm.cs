// Decompiled with JetBrains decompiler
// Type: Pulsar.Plugins.Server.SettingsForm
// Assembly: Stealer.Server, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D55D4852-5F7A-42EA-B9D3-2DEEE645AD6A
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Server.dll

using System;
using System.Drawing;
using System.Windows.Forms;

#nullable enable
namespace Pulsar.Plugins.Server;

public class SettingsForm : Form
{
  private TextBox txtWebhook;
  private TextBox txtToken;
  private TextBox txtChatId;

  public string WebhookUrl { get; private set; } = "";

  public string TgToken { get; private set; } = "";

  public string TgChatId { get; private set; } = "";

  public SettingsForm(string webhook, string token, string chatId)
  {
    this.Text = "Stealer Configuration";
    this.Size = new Size(500, 350);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    int num1 = 20;
    Label label1 = new Label();
    label1.Text = "Discord Webhook URL:";
    label1.Left = 20;
    label1.Top = num1;
    label1.AutoSize = true;
    Label label2 = label1;
    TextBox textBox1 = new TextBox();
    textBox1.Text = webhook;
    textBox1.Left = 20;
    textBox1.Top = num1 + 20;
    textBox1.Width = 440;
    this.txtWebhook = textBox1;
    int num2 = num1 + 60;
    Label label3 = new Label();
    label3.Text = "Telegram Bot Token:";
    label3.Left = 20;
    label3.Top = num2;
    label3.AutoSize = true;
    Label label4 = label3;
    TextBox textBox2 = new TextBox();
    textBox2.Text = token;
    textBox2.Left = 20;
    textBox2.Top = num2 + 20;
    textBox2.Width = 440;
    this.txtToken = textBox2;
    int num3 = num2 + 60;
    Label label5 = new Label();
    label5.Text = "Telegram Chat ID:";
    label5.Left = 20;
    label5.Top = num3;
    label5.AutoSize = true;
    Label label6 = label5;
    TextBox textBox3 = new TextBox();
    textBox3.Text = chatId;
    textBox3.Left = 20;
    textBox3.Top = num3 + 20;
    textBox3.Width = 440;
    this.txtChatId = textBox3;
    int num4 = num3 + 70;
    Button button1 = new Button();
    button1.Text = "Save Config";
    button1.Left = 280;
    button1.Top = num4;
    button1.Width = 100;
    button1.Height = 30;
    button1.DialogResult = DialogResult.OK;
    Button button2 = button1;
    Button button3 = new Button();
    button3.Text = "Cancel";
    button3.Left = 390;
    button3.Top = num4;
    button3.Width = 70;
    button3.Height = 30;
    button3.DialogResult = DialogResult.Cancel;
    Button button4 = button3;
    button2.Click += (EventHandler) ((s, e) =>
    {
      this.WebhookUrl = this.txtWebhook.Text.Trim();
      this.TgToken = this.txtToken.Text.Trim();
      this.TgChatId = this.txtChatId.Text.Trim();
    });
    this.Controls.Add((Control) label2);
    this.Controls.Add((Control) this.txtWebhook);
    this.Controls.Add((Control) label4);
    this.Controls.Add((Control) this.txtToken);
    this.Controls.Add((Control) label6);
    this.Controls.Add((Control) this.txtChatId);
    this.Controls.Add((Control) button2);
    this.Controls.Add((Control) button4);
  }
}
