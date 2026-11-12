window.petLocation = {
    getCurrentCoordinates: () => new Promise(resolve => {
        if (!navigator.geolocation) {
            resolve(null);
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => resolve([position.coords.longitude, position.coords.latitude]),
            () => resolve(null),
            { enableHighAccuracy: false, maximumAge: 60000, timeout: 8000 });
    })
};

window.petAvatarPreview = (() => {
    let currentUrl = null;

    return {
        create(inputId) {
            if (currentUrl) URL.revokeObjectURL(currentUrl);
            const file = document.getElementById(inputId)?.files?.[0];
            currentUrl = file ? URL.createObjectURL(file) : null;
            return currentUrl;
        },
        clear() {
            if (currentUrl) URL.revokeObjectURL(currentUrl);
            currentUrl = null;
        }
    };
})();

window.petLocations = {
    async getCities(stateId) {
        const response = await fetch(
            `https://servicodados.ibge.gov.br/api/v1/localidades/estados/${encodeURIComponent(stateId)}/municipios?orderBy=nome`);
        if (!response.ok) throw new Error(`IBGE respondeu ${response.status}`);
        const municipalities = await response.json();
        return municipalities.map(municipality => municipality.nome);
    }
};