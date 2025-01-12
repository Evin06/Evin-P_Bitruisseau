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

        public async Task SendFile(string topic, string fileName)
        {
            var filePath = Path.Combine(@"..\..\..\Music", fileName);
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {fileName}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fileContent = Convert.ToBase64String(File.ReadAllBytes(filePath));
            var sendMusic = new SendMusic { FileName = fileName, Content = fileContent };

            var envelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = MessageType.ENVOIE_FICHIER,
                EnveloppeJson = JsonSerializer.Serialize(sendMusic)
            };

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonSerializer.Serialize(envelope))
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
