//LiveReload by Buildstarted 2020
(() => {
    //live_reload_options
    //  url
    //  saveFormData
    //  inlineUpdatesWhenPossible
    //  showStatusOnPage
    //  reloadOnReconnect
    let requestUrl = new URL(window.location.href);
    let hostname = requestUrl.hostname;
    let port = requestUrl.port;
    let protocol = requestUrl.protocol === 'https:' ? 'wss' : 'ws';
    let url = `${protocol}://${hostname}${port ? `:${port}` : ""}${live_reload_options.url}`;
    let socket;
    let reconnect = true;
    let $statusicon = null;
    let isreconnecting = false;
    const formdatakey = "__live-reload-form-data-";

    if (live_reload_options.showStatusOnPage) {
        let status = document.createElement("div");
        status.style.position = "fixed";
        status.style.right = ".5em";
        status.style.top = ".5em";
        status.style.background = "#f54242";
        status.style.width = "16px";
        status.style.height = "16px";
        status.style.borderRadius = "100%";
        status.setAttribute("id", "__live-reload-status-icon");
        status.style.zIndex = "1000";
        document.body.appendChild(status);
        $statusicon = status;
    }

    const serializeForm = (form) => {
        let obj = {};
        const formData = new FormData(form);
        for (let key of formData.keys()) {
            obj[key] = formData.get(key);
        }
        return obj;
    };

    const saveFormData = () => {
        const forms = document.querySelectorAll("form");
        for (let i = 0; i < forms.length; i++) {
            const form = forms[i];
            window.localStorage.setItem(`${formdatakey}${i}`, JSON.stringify(serializeForm(form)));
        }
    };

    const updateElements = (data) => {
        let path = data.split('|')[1];

        let filename = path.split('?')[0];
        if (filename[0] === '/') {
            filename = filename.substring(1);
        }

        let elements = document.querySelectorAll(`[src*='${filename}'], [href*='${filename}']`);
        for (let i = 0; i < elements.length; i++) {
            let element = elements[i];
            if (element.href) {
                element.href = path;
            } else if (element.src) {
                element.src = path;
            }
        }
    };

    if (live_reload_options.saveFormData) {
        const forms = document.querySelectorAll("form");
        for (let i = 0; i < forms.length; i++) {
            const form = forms[i];
            const data = window.localStorage.getItem(`${formdatakey}${i}`);
            window.localStorage.removeItem(`${formdatakey}${i}`);
            const result = JSON.parse(data);
            for (let key in result) {
                const element = form.querySelector(`[name='${key}']`);
                if (element) {
                    if (element.tagName === "SELECT") {
                        const options = element.options;
                        for (let option, j = 0; option = options[j]; j++) {
                            if (option.value === result[key]) {
                                element.selectedIndex = j;
                                break;
                            }
                        }
                    } else if (element.tagName === "INPUT") {
                        element.value = result[key];
                    }
                }
            }
        }
    }

    const refresh = () => {
        try {
            if (live_reload_options.saveFormData) {
                saveFormData();
            }
        } catch (e) {
            console.error(e);
        }
        window.location.href = window.location.href;
    }

    const messageReceived = (e) => {
        if (e.data.startsWith('reload') && live_reload_options.inlineUpdatesWhenPossible) {
            updateElements(e.data);
        } else if (e.data.startsWith('ping')) {
        } else {
            refresh();
        }
    };

    const unload = (e) => {
        if (socket) {
            reconnect = false;
            socket.close(1001);
        }
    };

    window.addEventListener("beforeunload", unload);

    const socketClosed = (e) => {
        if ($statusicon) {
            $statusicon.style.background = "#800";
        }
        if (reconnect) {
            isreconnecting = true;
            connect();
        }
    };

    const socketOpened = (e) => {
        if (isreconnecting) {
            isreconnecting = false;
            if (live_reload_options.reloadOnReconnect) {
                refresh();
            }
        }
        if ($statusicon) {
            $statusicon.style.background = "#080";
        }
    }

    const connect = () => {
        socket = new WebSocket(url);
        socket.addEventListener("open", socketOpened);
        socket.addEventListener("close", socketClosed);
        socket.addEventListener("message", messageReceived);
    }

    connect();
})();