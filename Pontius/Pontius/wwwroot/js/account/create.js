async function createAccount() {

    const form = document.getElementById("createAccountForm");
    const formData = new FormData(form);

    const payload = Object.fromEntries(formData.entries());

    try {
        const response = await fetch('/account/create', {
            method: 'POST',
            credentials: 'include',
            headers: {
            'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            console.error('Failed to register. Status:', response.status);
            // Optionally show message to user
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