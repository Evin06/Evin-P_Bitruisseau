using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using MédiaPlayer.Models;
using MédiaPlayer.Envelopes;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private static List<Music> receivedMusicList = new List<Music>();

        public Form1()
        {
            InitializeComponent();
            InitializeMqttClient();
        }

        private async void InitializeMqttClient()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("blue.section-inf.ch", 1883)
                    .WithCredentials("ict", "321")
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithCleanSession()
                    .Build();

                await mqttClient.ConnectAsync(options);
                MessageBox.Show("Connecté au serveur MQTT.");

                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("tutu")
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion : {ex.Message}");
            }
        }

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var envelope = JsonSerializer.Deserialize<GenericEnvelope>(receivedMessage);

                if (envelope.SenderId == mqttClient.Options.ClientId) return;

                switch (envelope.MessageType)
                {
                    case MessageType.DEMANDE_CATALOGUE:
                        await RespondToCatalogRequest();
                        break;

                    case MessageType.ENVOIE_CATALOGUE:
                        ProcessReceivedCatalog(envelope);
                        break;

                    case MessageType.DEMANDE_FICHIER:
                        await SendRequestedFile(envelope);
                        break;

                    case MessageType.ENVOIE_FICHIER:
                        ProcessReceivedMusicFile(envelope);
                        break;

                    default:
                        Console.WriteLine("Message non reconnu.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du traitement du message : {ex.Message}");
            }
        }

        private async Task RespondToCatalogRequest()
        {
            try
            {
                var catalog = new SendCatalog
                {
                    Content = LoadLocalMediaData()
                };

                if (!catalog.Content.Any())
                {
                    MessageBox.Show("Aucun média local à envoyer.");
                    return;
                }

                var responseEnvelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_CATALOGUE,
                    EnveloppeJson = JsonSerializer.Serialize(catalog)
                };

                await SendData("tutu", JsonSerializer.Serialize(responseEnvelope));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du catalogue : {ex.Message}");
            }
        }

        private void ProcessReceivedCatalog(GenericEnvelope envelope)
        {
            try
            {
                // Désérialisation de la réponse JSON
                var receivedCatalog = JsonSerializer.Deserialize<SendCatalog>(envelope.EnveloppeJson);

                // Vérification que la liste "Content" contient des éléments
                if (receivedCatalog?.Content != null && receivedCatalog.Content.Any())
                {
                    // Vider la liste existante avant d'ajouter les nouveaux éléments
                    receivedMusicList.Clear();

                    // Parcourir chaque média dans le contenu reçu
                    foreach (var media in receivedCatalog.Content)
                    {
                        // Met à jour avec les bonnes propriétés du JSON
                        receivedMusicList.Add(new Music
                        {
                            Name = media.FileName,            // Le titre du fichier
                            Duration = media.FileDuration,  // Durée du fichier
                            Extension = media.FileType // Extension du fichier
                        });
                    }

                    // Mise à jour de l'affichage dans la ListBox
                    UpdateListBoxWithReceivedMusic();
                }
                else
                {
                    MessageBox.Show("Le catalogue reçu est vide ou invalide.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du traitement du catalogue : {ex.Message}");
            }
        }

        private void UpdateListBoxWithReceivedMusic()
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action(UpdateListBoxWithReceivedMusic));
            }
            else
            {
                listBox1.Items.Clear();

                // Ajouter chaque fichier dans la ListBox
                foreach (var music in receivedMusicList)
                {
                    listBox1.Items.Add($"{music.Name}{music.Extension} | Durée : {music.Duration}");
                }
            }
        }

        private async Task SendRequestedFile(GenericEnvelope envelope)
        {
            try
            {
                var fileRequest = JsonSerializer.Deserialize<FileRequest>(envelope.EnveloppeJson);

                if (fileRequest == null || string.IsNullOrWhiteSpace(fileRequest.FileName))
                {
                    MessageBox.Show("Demande de fichier invalide.");
                    return;
                }

                string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Music");
                string filePath = Path.Combine(musicFolderPath, fileRequest.FileName);

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Le fichier demandé n'existe pas : {fileRequest.FileName}");
                    return;
                }

                string fileBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));

                var sendMusic = new SendMusic
                {
                    FileName = fileRequest.FileName,
                    Content = fileBase64
                };

                var responseEnvelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_FICHIER,
                    EnveloppeJson = JsonSerializer.Serialize(sendMusic)
                };

                await SendData("tutu", JsonSerializer.Serialize(responseEnvelope));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du fichier : {ex.Message}");
            }
        }

        private void ProcessReceivedMusicFile(GenericEnvelope envelope)
        {
            try
            {
                var receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnveloppeJson);

                if (receivedMusic != null && !string.IsNullOrWhiteSpace(receivedMusic.Content))
                {
                    string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Music");
                    Directory.CreateDirectory(musicFolderPath);

                    string filePath = Path.Combine(musicFolderPath, receivedMusic.FileName);
                    File.WriteAllBytes(filePath, Convert.FromBase64String(receivedMusic.Content));

                    MessageBox.Show($"Fichier téléchargé : {receivedMusic.FileName}");

                    LoadMusicFiles();
                }
                else
                {
                    MessageBox.Show("Le fichier reçu est invalide.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la réception du fichier : {ex.Message}");
            }
        }

        private async Task SendData(string topic, string data)
        {
            try
            {
                var message = new MQTTnet.MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(data)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi des données : {ex.Message}");
            }
        }

        private List<MediaData> LoadLocalMediaData()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Music");
            var mediaList = new List<MediaData>();

            if (Directory.Exists(musicFolderPath))
            {
                // Utilisation du filtre *.* pour récupérer tous les types de fichiers
                var files = Directory.GetFiles(musicFolderPath, "*.*");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    mediaList.Add(new MediaData
                    {
                        FileName = Path.GetFileName(file),
                        FileType = fileInfo.Extension,  // Utilisation de l'extension pour connaître le type
                        FileDuration = "Durée inconnue"  // Durée laissée comme inconnue
                    });
                }
            }

            return mediaList;
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

            await SendData("tutu", JsonSerializer.Serialize(askCatalog));
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
                await SendData("tutu", JsonSerializer.Serialize(requestEnvelope));
                MessageBox.Show($"Demande envoyée pour : {selectedMusic.Name}{selectedMusic.Extension}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi de la demande : {ex.Message}");
            }
        }
    }
}
