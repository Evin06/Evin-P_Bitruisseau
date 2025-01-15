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
        private Dictionary<string, List<MediaData>> musicOther = new Dictionary<string, List<MediaData>>();
        public List<MediaData> myMusicList = new List<MediaData>();

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
                    },
                    new MqttTopicFilter
                    {
                        Topic = catalogManager.mqttClient.Options.ClientId,
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
                GenericEnvelope envelope = JsonSerializer.Deserialize<GenericEnvelope>(receivedMessage);
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
                        await fileManager.SendFile(envelope);
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
                Console.WriteLine($"Received catalog: {envelope.EnvelopeJson}");

                SendCatalog receivedCatalog = JsonSerializer.Deserialize<SendCatalog>(envelope.EnvelopeJson);
                if (musicOther.ContainsKey(envelope.SenderId))
                {
                    musicOther[envelope.SenderId] = receivedCatalog.Content;
                }
                else
                {
                    musicOther.Add(envelope.SenderId, new List<MediaData>());
                    musicOther[envelope.SenderId] = receivedCatalog.Content;
                }

                UpdateListBoxWithReceivedMusic();
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
                foreach (MediaData music in musicOther.Values.SelectMany(list => list).ToList())
                {
                    listBox1.Items.Add($"{music.Title} | {music.Artist} | {music} | {music.Size} bytes | Duration: {music.Duration}");
                }
            }
        }

        private void ProcessReceivedMusicFile(GenericEnvelope envelope)
        {
            try
            {
                SendMusic receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnvelopeJson);
                if (receivedMusic != null)
                {
                    var filePath = "..\\..\\..\\Music\\" + receivedMusic.FileInfo.Title + receivedMusic.FileInfo.Type;

                    byte[] fileBytes = Convert.FromBase64String(receivedMusic.Content);
                    File.WriteAllBytes(filePath, fileBytes);

                    MessageBox.Show($"File downloaded and saved: {receivedMusic.FileInfo}", "Download Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                EnvelopeJson = JsonSerializer.Serialize(new { Content = "Demande de catalogue" })
            };

            await catalogManager.SendData("tutu", JsonSerializer.Serialize(askCatalog));
            MessageBox.Show("Catalog request sent!");
        }

        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) return;

            AskMusic askMusic = new AskMusic();
            List<MediaData> list = new List<MediaData>();
            list = musicOther.Values.SelectMany(list => list).ToList();
            MediaData selectedMusic = list[listBox1.SelectedIndex];

            askMusic.FileName = selectedMusic.Title + selectedMusic.Type;

            GenericEnvelope requestEnvelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = MessageType.DEMANDE_FICHIER,
                EnvelopeJson = askMusic.ToJson()
            };

            try
            {
                await fileManager.SendData(musicOther.First(keyValue => keyValue.Value.Contains(selectedMusic)).Key, JsonSerializer.Serialize(requestEnvelope));
                MessageBox.Show($"Request sent for: {selectedMusic.Title}{selectedMusic.Type}");
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

            foreach (string path in musicFiles)
            {
                MediaData data = new();
                var tfile = TagLib.File.Create(path);

                FileInfo fileInfo = new(path);
                data.Size = fileInfo.Length;

                data.Title = fileInfo.Name.Replace(fileInfo.Extension, "");
                data.Type = Path.GetExtension(path);
                data.Artist = tfile.Tag.FirstPerformer;
                TimeSpan duration = tfile.Properties.Duration;
                data.Duration = $"{duration.Minutes:D2}:{duration.Seconds:D2}";
                myMusicList.Add(data);

                listBox1.Items.Add($"{data.Title}{data.Type} | Duration: {data.Duration}");
            }
            getMyMusic();
        }

        public void getMyMusic()
        {
            catalogManager.musics = myMusicList;
            fileManager.myMusicList = myMusicList;
        }
    }
}
