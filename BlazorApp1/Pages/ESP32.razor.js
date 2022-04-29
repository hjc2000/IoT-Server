class Mqtt {
    /**
     * 如果没有传递参数，则会使用匿名连接
     * @param {string} username
     * @param {string} password
     */
    constructor(dotnetHelper,username, password) {
        let options = {
            // Clean session
            clean: true,
            // Auth
            username,
            password,
        }

        this.dotnetHelper = dotnetHelper;

        this.client = mqtt.connect("ws://127.0.0.1:8083/mqtt", options);

        this.client.on("connect", () => {
            console.log("已连接");
            this.client.subscribe("esp32/temperature", (error, granted) => {
                if (error) {
                    console.log(error);
                } else {
                    console.log("订阅了" + granted[0].topic);
                }
            });
        });

        //在收到数据后将数据传给.NET方法
        this.client.on("message", (topic, payload) => {
            let uarr = Uint8Array.from(payload.toJSON().data);
            this.dotnetHelper.invokeMethodAsync("GetJsData", uarr);
        });

    }

    /**
     * 发布主题
     * @param {string} topic
     * @param {Uint8Array} payload
     */
    publish(topic, payload) {
        this.client.publish(topic, payload);
    }
}

export function getMqtt(dotnetHelper) {
    return new Mqtt(dotnetHelper,"hjcWebApp","123456");
}
