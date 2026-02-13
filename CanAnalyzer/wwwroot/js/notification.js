// Глобальный объект для управления подписчиками
window.notificationManager = {
    dotNetHelpers: {
        toast: null,      // для NotificationToast
        analyzer: null    // для CanAnalyzer
    },
    connection: null,

    setupToastListener: function (helper) {
        console.log("Setting up toast listener...");
        this.dotNetHelpers.toast = helper;
        this.ensureConnection();
    },

    setupAnalyzerListener: function (helper) {
        console.log("Setting up analyzer listener...");
        this.dotNetHelpers.analyzer = helper;
        this.ensureConnection();
    },

    ensureConnection: function () {
        if (this.connection) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/canhub", {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        this.connection.on("ReceiveNotification", (data) => {
            console.log("Received notification:", data);

            if (this.dotNetHelpers.toast) {
                this.dotNetHelpers.toast.invokeMethodAsync('AddNotification',
                    data.sdoId,
                    data.statusCode,
                    data.statusName,
                    data.time
                ).catch(err => {
                    console.error("Error in AddNotification:", err);
                });
            }
        });

        this.connection.on("RefreshData", () => {
            console.log("RefreshData received");

            if (this.dotNetHelpers.analyzer) {
                this.dotNetHelpers.analyzer.invokeMethodAsync('RefreshData')
                    .catch(err => {
                        console.error("Error in RefreshData:", err);
                    });
            }
        });

        this.connection.start()
            .then(() => console.log("SignalR connected successfully"))
            .catch(err => {
                console.error("SignalR connection error:", err);
                setTimeout(() => this.connection.start(), 5000);
            });
    }
};

// Сохраняем обратную совместимость
window.setupNotificationListener = function (helper) {
    // Этот метод больше не используется, но оставляем для безопасности
    console.warn("setupNotificationListener is deprecated. Use setupToastListener or setupAnalyzerListener instead.");
};