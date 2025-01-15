using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Envelopes;
using MédiaPlayer.Models;
using MQTTnet.Packets;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private CatalogManager catalogManager;
        private FileManager fileManager;
        private List<Music> receivedMusicList = new List<Music>();
        public List<Music> myMusicList = new List<Music>();

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
                LoadMusicFiles();

                await InitializeSubscriptions();
            }
            else
            {
                MessageBox.Show("Failed to initialize MQTT connection.");
            }
        }

        private async Task InitializeSubscriptions()
        {
            if (mqttClient.IsConnected)
            {
                var topicFilters = new List<MqttTopicFilter>
        {
            new MqttTopicFilter
            {
                Topic = "tutu",
                QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce
            }
        };

                await mqttClient.SubscribeAsync(new MQTTnet.Client.MqttClientSubscribeOptions
                {
                    TopicFilters = topicFilters
                });

                Console.WriteLine("Subscribed to topic: tutu");
            }
            else
            {
                MessageBox.Show("MQTT client is not connected, unable to subscribe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string topic = e.ApplicationMessage.Topic;
            string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            if (string.IsNullOrWhiteSpace(receivedMessage))
            {
                Console.WriteLine("Empty message received on topic: " + topic);
                return;
            }

            try
            {
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
                        Console.WriteLine($"Unhandled message type: {envelope.MessageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex.Message}");
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
                Console.WriteLine($"Received catalog: {envelope.EnveloppeJson}");

                var receivedCatalog = JsonSerializer.Deserialize<SendCatalog>(envelope.EnveloppeJson);
                if (receivedCatalog?.Content != null && receivedCatalog.Content.Any())
                {
                    receivedMusicList.Clear();
                    receivedMusicList.AddRange(receivedCatalog.Content.Select(m => new Music
                    {
                        Name = m.Title,
                        Artist = m.Artist,
                        FileType = m.Type,
                        Duration = m.Duration,
                        Size = m.Size
                    }));

                    UpdateListBoxWithReceivedMusic();
                }
                else
                {
                    Console.WriteLine("Received catalog is empty or invalid.");
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
            try
            {
                var receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnveloppeJson);
                if (receivedMusic != null)
                {
                    var filePath = Path.Combine(@"..\..\..\Music", receivedMusic.FileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Assure que le dossier existe

                    var fileBytes = Convert.FromBase64String(receivedMusic.Content);
                    File.WriteAllBytes(filePath, fileBytes);

                    MessageBox.Show($"File downloaded and saved: {receivedMusic.FileName}", "Download Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Console.WriteLine("Invalid music file received.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "File Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Title = $"{selectedMusic.Name}{selectedMusic.Extension}"
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
                    Duration = "00:32"
                };

                myMusicList.Add(music);

                listBox1.Items.Add($"{music.Name}{music.Extension} | Duration: {music.Duration}");
            }
            getMyMusic();
        }

        public void getMyMusic()
        {
            catalogManager.musics = myMusicList;
        }
    }
}
