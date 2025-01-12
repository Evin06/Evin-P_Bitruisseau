using MQTTnet.Client;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Envelopes;
using MédiaPlayer.Models;
using MQTTnet;

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
            var fileRequest = JsonSerializer.Deserialize<FileRequest>(requestEnvelope.EnveloppeJson);
            if (fileRequest == null)
            {
                MessageBox.Show("Invalid file request.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filePath = Path.Combine(@"..\..\..\Music", fileRequest.FileName);
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {fileRequest.FileName}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fileContent = Convert.ToBase64String(File.ReadAllBytes(filePath));
            var sendMusic = new SendMusic { FileName = fileRequest.FileName, Content = fileContent };

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
        }


        public async Task SendData(string topic, string data)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(data)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(message);
        }

    }
}
