class Mqtt {
    constructor(username, password) {
        let options = {
            // Clean session
            clean: true,
            // Auth
            username,
            password,
        }

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
            DotNet.invokeMethodAsync('BlazorApp1', "GetJsData", uarr);
        });

    }

    publish(topic, payload) {
        this.client.publish(topic, payload);
    }
}

export function getMqtt() {
    return new Mqtt();
}
