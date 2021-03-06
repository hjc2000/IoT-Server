using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;

namespace WebServerLib
{
    /// <summary>
    /// 我的 MQTT 客户端类
    /// </summary>
    public class ServerMqttClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ServerMqttClient()
        {
            StartConnect();
        }
        IMqttClient? _client;
        /// <summary>
        /// 开始连接到MQTT服务器
        /// </summary>
        async void StartConnect()
        {
            var optionBuilder = GetDefaultOptionBuilder();
            _client = new MqttFactory().CreateMqttClient();
            UseHook();
            await _client.ConnectAsync(optionBuilder.Build());
        }
        /// <summary>
        /// 获取默认的连接配置构建器
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        static MqttClientOptionsBuilder GetDefaultOptionBuilder()
        {
            return new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 1883)//连接到本机的MQTT服务器
                .WithClientId("server")//设置客户端ID
                .WithCredentials("server", _password);
        }
        //随机生成密码
        public static string _password = Guid.NewGuid().ToString();
        /// <summary>
        /// 使用事件钩子
        /// </summary>
        void UseHook()
        {
            //连接完成后被回调
            _client.UseConnectedHandler((c) =>
            {
                SubscribeAsync("server/#");
            });
            //在接收到MQTT消息的时候被回调
            _client.UseApplicationMessageReceivedHandler(void (e) =>
            {
                var msg = e.ApplicationMessage;
            });
        }
        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        public async void SubscribeAsync(string topic)
        {
            MqttClientSubscribeOptions subscribeOptions = new MqttClientSubscribeOptions();
            subscribeOptions.TopicFilters = new List<MqttTopicFilter>
                {
                    //添加多个MqttTopicFilter可以同时订阅多个主题
                    new MqttTopicFilter()
                    {
                        Topic = topic,
                    },
                };
            await _client.SubscribeAsync(subscribeOptions);
        }
        /// <summary>
        /// 发布主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        public async void PublishAsync(string topic,byte[] payload)
        {
            await _client.PublishAsync(topic,payload);
        }
    }
}
