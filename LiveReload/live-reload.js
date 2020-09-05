//LiveReload by Buildstarted 2020
(() => {
    let requestUrl = new URL(window.location.href);
    let hostname = requestUrl.hostname;
    let port = requestUrl.port;
    let protocol = requestUrl.protocol === 'https:' ? 'wss' : 'ws';
    let url = `${protocol}://${hostname}${port ? `:${port}` : ""}/live-reload`;

    let socket = new WebSocket(url);

    const reconnect = () => {
        socket = new WebSocket(url);
    };

    const serializeForm = (form) => {
        let obj = {};
        const formData = new FormData(form);
        for (let key of formData.keys()) {
            obj[key] = formData.get(key);
        }
        return obj;
    };

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


    socket.onmessage = (e) => {
        console.log(e.data);
        if (e.data.startsWith('reload')) {
            let path = e.data.split('|')[1];

            let filename = path.split('?')[0];

            let elements = document.querySelectorAll(`[src*='${filename}'], [href*='${filename}']`);
            for (let i = 0; i < elements.length; i++) {
                let element = elements[i];
                console.log(element);
                if (element.tagName === "LINK") {
                    //css
                    let link = document.createElement('link');
                    link.setAttribute('href', path);
                    link.setAttribute('rel', 'stylesheet');
                    link.setAttribute('type', 'text/css');

                    document.head.appendChild(link);
                } else if (element.src) {
                    element.src = path;
                }
            }
        } else {
            try {
                const forms = document.querySelectorAll("form");
                for (let i = 0; i < forms.length; i++) {
                    const form = forms[i];
                    window.localStorage.setItem(`__live-reload-form-data-${i}`, JSON.stringify(serializeForm(form)));
                }

            } catch (e) {
                console.error(e);
            }
            window.location.href = window.location.href;
        }
    };

    socket.onclose = (e) => { reconnect(); };

    //socket.onopen = (e) => {{ console.log('opened', e); }};
    //socket.onclose = (e) => {{ console.log('close', e); }};
    //socket.onerror = (e) => {{ console.log('error', e); }};
})();