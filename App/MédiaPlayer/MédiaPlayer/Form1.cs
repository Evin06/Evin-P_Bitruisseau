using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Envelopes;
using MédiaPlayer.Models;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private CatalogManager catalogManager;
        private FileManager fileManager;
        private List<Music> receivedMusicList = new List<Music>();

        public Form1()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            mqttClient = await MQTTConnectionManager.GetClientAsync();
            if (mqttClient != null)
            {
                catalogManager = new CatalogManager(mqttClient);
                fileManager = new FileManager(mqttClient);
                mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
            }
            else
            {
                MessageBox.Show("Failed to initialize MQTT connection.");
            }
        }

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var envelope = JsonSerializer.Deserialize<GenericEnvelope>(receivedMessage);
            if (envelope == null || envelope.SenderId == mqttClient.Options.ClientId) return;

            switch (envelope.MessageType)
            {
                case MessageType.DEMANDE_CATALOGUE:
                    await SendMyCatalog();
                    break;
                case MessageType.ENVOIE_CATALOGUE:
                    ProcessReceivedCatalog(envelope);
                    break;
                case MessageType.DEMANDE_FICHIER:
                    await fileManager.SendFile("tutu", envelope);
                    break;
                case MessageType.ENVOIE_FICHIER:
                    ProcessReceivedMusicFile(envelope);
                    break;
                default:
                    Console.WriteLine("Unhandled message type.");
                    break;
            }
        }


        private async Task SendMyCatalog()
        {
            await catalogManager.SendCatalog("tutu");
        }

        private void ProcessReceivedCatalog(GenericEnvelope envelope)
        {
            try
            {
                var receivedCatalog = JsonSerializer.Deserialize<SendCatalog>(envelope.EnveloppeJson);
                if (receivedCatalog?.Content != null)
                {
                    receivedMusicList.Clear();
                    receivedMusicList.AddRange(receivedCatalog.Content.Select(m => new Music
                    {
                        Name = m.FileName,
                        Artist = m.FileArtist,
                        FileType = m.FileType,
                        Duration = m.FileDuration,
                        Size = m.FileSize
                    }));

                    UpdateListBoxWithReceivedMusic();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing received catalog: {ex.Message}");
            }
        }

        private void UpdateListBoxWithReceivedMusic()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UpdateListBoxWithReceivedMusic));
            }
            else
            {
                listBox1.Items.Clear();
                foreach (var music in receivedMusicList)
                {
                    listBox1.Items.Add($"{music.Name} | {music.Artist} | {music.FileType} | {music.Size} bytes | Duration: {music.Duration}");
                }
            }
        }

        private void ProcessReceivedMusicFile(GenericEnvelope envelope)
        {
            var receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnveloppeJson);
            if (receivedMusic != null)
            {
                var filePath = Path.Combine(@"..\..\..\Music", receivedMusic.FileName);
                try
                {
                    File.WriteAllBytes(filePath, Convert.FromBase64String(receivedMusic.Content));
                    MessageBox.Show($"File downloaded and saved: {receivedMusic.FileName}", "Download Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "File Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void buttonMesMedias_Click(object sender, EventArgs e)
        {
            LoadMusicFiles();
        }

        private async void buttonMediaAutres_Click(object sender, EventArgs e)
        {
            var askCatalog = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = MessageType.DEMANDE_CATALOGUE,
                EnveloppeJson = JsonSerializer.Serialize(new { Content = "Demande de catalogue" })
            };

            await catalogManager.SendData("tutu", JsonSerializer.Serialize(askCatalog));
            MessageBox.Show("Catalog request sent!");
        }

        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0 || listBox1.SelectedIndex >= receivedMusicList.Count) return;

            var selectedMusic = receivedMusicList[listBox1.SelectedIndex];
            var requestEnvelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = MessageType.DEMANDE_FICHIER,
                EnveloppeJson = JsonSerializer.Serialize(new FileRequest
                {
                    FileName = $"{selectedMusic.Name}{selectedMusic.Extension}"
                })
            };

            try
            {
                await fileManager.SendData("tutu", JsonSerializer.Serialize(requestEnvelope));
                MessageBox.Show($"Request sent for: {selectedMusic.Name}{selectedMusic.Extension}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file request: {ex.Message}");
            }
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Music");

            listBox1.Items.Clear();

            if (!Directory.Exists(musicFolderPath))
            {
                listBox1.Items.Add("Music folder does not exist.");
                return;
            }

            string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.*");  // Load all file types

            foreach (string musicFile in musicFiles)
            {
                var music = new Music
                {
                    Name = Path.GetFileNameWithoutExtension(musicFile),
                    Extension = Path.GetExtension(musicFile),
                    Duration = "Unknown duration"  // You could also add a method to fetch file durations
                };

                listBox1.Items.Add($"{music.Name}{music.Extension} | Duration: {music.Duration}");
            }
        }
    }
}
