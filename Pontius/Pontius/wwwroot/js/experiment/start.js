async function startExperiment() {


    const form = document.getElementById("startExperimentForm");
    const formData = new FormData(form);

    const payload = Object.fromEntries(formData.entries());

    try {
        const response = await fetch('Start', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            console.error('Failed to register. Status:', response.status);
            return;
        }

        const result = await response.json();

        console.log(result)

        if (result.redirectUrl) {
            console.log('Redirecting to:', result.redirectUrl);
            window.location.href = result.redirectUrl;
        } else {
            console.warn('No redirect URL provided by server.');
        }

    } catch (error) {
        console.error('Error during registration:', error);
    }
}