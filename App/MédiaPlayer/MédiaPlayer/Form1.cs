using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
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
                case MessageType.DEMANDE_CATALOGUE: // Assurez-vous que cela correspond à 1 selon votre énumération
                    await SendMyCatalog();
                    break;
                case MessageType.ENVOIE_CATALOGUE:
                    ProcessReceivedCatalog(envelope);
                    break;
                case MessageType.DEMANDE_FICHIER:
                    await fileManager.SendFile("tutu", envelope.EnveloppeJson);
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
                    foreach (var media in receivedCatalog.Content)
                    {
                        receivedMusicList.Add(new Music
                        {
                            Name = media.FileName,
                            Artist = media.FileArtist,
                            FileType = media.FileType,
                            Duration = media.FileDuration,
                            Size = media.FileSize
                        });
                    }
                    UpdateListBoxWithReceivedMusic();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du traitement du catalogue : {ex.Message}");
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
                    listBox1.Items.Add($"{music.Name} | {music.Artist} | {music.FileType} | {music.Size} bytes | Durée : {music.Duration}");
                }
            }
        }




        private void ProcessReceivedMusicFile(GenericEnvelope envelope)
        {
            var receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnveloppeJson);
            if (receivedMusic != null)
            {
                MessageBox.Show($"Fichier téléchargé : {receivedMusic.FileName}");
                // Additional logic to handle the downloaded file
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
            MessageBox.Show("Demande de catalogue envoyée !");
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
                MessageBox.Show($"Demande envoyée pour : {selectedMusic.Name}{selectedMusic.Extension}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi de la demande : {ex.Message}");
            }
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Music");

            listBox1.Items.Clear();

            if (!Directory.Exists(musicFolderPath))
            {
                listBox1.Items.Add("Le dossier 'Music' n'existe pas.");
                return;
            }

            string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.*"); // Permet de charger tous les types de fichiers

            foreach (string musicFile in musicFiles)
            {
                var music = new Music
                {
                    Name = Path.GetFileNameWithoutExtension(musicFile),
                    Extension = Path.GetExtension(musicFile),
                    Duration = "Durée inconnue" // Vous pouvez également ajouter une méthode pour obtenir la durée des fichiers
                };

                listBox1.Items.Add($"{music.Name}{music.Extension} | Durée : {music.Duration}");
            }
        }
    }
}
