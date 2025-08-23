async function startTest() {

    const form = document.getElementById("startTestForm");
    const formData = new FormData(form);

    const payload = Object.fromEntries(formData.entries());

    await fetch('/experiment/SetTestHasStarted', {
      method: 'GET',
      credentials: 'include',
      headers: {
        'Accept': 'application/json'
      }
    });

    try {
        const response = await fetch('/experiment/test', {
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