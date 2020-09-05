//LiveReload by Buildstarted 2020
(() => {
    //live_reload_options
    //  url
    //  saveFormData
    //  inlineUpdatesWhenPossible
    
    let requestUrl = new URL(window.location.href);
    let hostname = requestUrl.hostname;
    let port = requestUrl.port;
    let protocol = requestUrl.protocol === 'https:' ? 'wss' : 'ws';
    let url = `${protocol}://${hostname}${port ? `:${port}` : ""}/${live_reload_options.url}`;

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
            window.localStorage.setItem(`__live-reload-form-data-${i}`, JSON.stringify(serializeForm(form)));
        }
    };

    const updateElements = (data) => {
        let path = data.split('|')[1];

        let filename = path.split('?')[0];

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
            const data = window.localStorage.getItem(`__live-reload-form-data-${i}`);
            window.localStorage.removeItem(`__live-reload-form-data-${i}`);
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

    const messageReceived = (e) => {
        if (e.data.startsWith('reload') && live_reload_options.inlineUpdatesWhenPossible) {
            updateElements(e.data);
        } else {
            try {
                if (live_reload_options.saveFormData) {
                    saveFormData();
                }
            } catch (e) {
                console.error(e);
            }
            window.location.href = window.location.href;
        }
    };

    const socketClosed = (e) => { connect(); };
    
    const connect = () => {
        let socket = new WebSocket(url);
        socket.addEventListener("close", socketClosed);
        socket.addEventListener("message", messageReceived);
    }

    connect();

    //socket.onopen = (e) => {{ console.log('opened', e); }};
    //socket.onclose = (e) => {{ console.log('close', e); }};
    //socket.onerror = (e) => {{ console.log('error', e); }};
})();