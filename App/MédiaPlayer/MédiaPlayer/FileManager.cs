using MQTTnet;
using MQTTnet.Client;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Envelopes;
using MédiaPlayer.Models;

namespace MédiaPlayer
{
    public class FileManager
    {
        private readonly IMqttClient mqttClient;
        public List<MediaData> myMusicList;

        public FileManager(IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
        }

        public async Task SendFile(GenericEnvelope requestEnvelope)
        {
            try
            {
                AskMusic fileRequest = JsonSerializer.Deserialize<AskMusic>(requestEnvelope.EnvelopeJson);
                if (fileRequest == null)
                {
                    MessageBox.Show("Invalid file request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string filePath = "..\\..\\..\\Music\\" + fileRequest.FileName;
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"File not found: {fileRequest.FileName}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Convert file to Base64

                SendMusic sendMusic = new SendMusic { FileInfo = myMusicList.First(music => music.Title == Path.GetFileNameWithoutExtension(fileRequest.FileName)), Content = Convert.ToBase64String(File.ReadAllBytes(filePath)) };

                GenericEnvelope responseEnvelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_FICHIER,
                    EnvelopeJson = sendMusic.ToJson()
                };

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(requestEnvelope.SenderId)
                    .WithPayload(JsonSerializer.Serialize(responseEnvelope))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
                Console.WriteLine($"File sent: {fileRequest.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public async Task SendData(string topic, string data)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(data)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}", "Data Send Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
