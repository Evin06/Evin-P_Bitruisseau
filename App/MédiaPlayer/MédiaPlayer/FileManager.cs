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

        public FileManager(IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
        }

        public async Task SendFile(string topic, GenericEnvelope requestEnvelope)
        {
            try
            {
                var fileRequest = JsonSerializer.Deserialize<FileRequest>(requestEnvelope.EnveloppeJson);
                if (fileRequest == null)
                {
                    MessageBox.Show("Invalid file request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var filePath = Path.Combine(@"..\..\..\Music", fileRequest.Title);
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"File not found: {fileRequest.Title}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Convert file to Base64
                var fileBytes = File.ReadAllBytes(filePath);
                var fileContentBase64 = Convert.ToBase64String(fileBytes);

                var sendMusic = new SendMusic { FileName = fileRequest.Title, Content = fileContentBase64 };

                var responseEnvelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_FICHIER,
                    EnveloppeJson = JsonSerializer.Serialize(sendMusic)
                };

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(JsonSerializer.Serialize(responseEnvelope))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
                Console.WriteLine($"File sent: {fileRequest.Title}");
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
