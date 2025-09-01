
let currentBuoyIndex = 0;

function clearLocalStorage() {
    localStorage.clear();
}

function answer(usersAnswer, usersCorrectAnswer) {
  alert(`Correct Answer: ${usersCorrectAnswer}  Your Answer: ${usersAnswer}`);

  // If you're in Razor, it's safest to generate the URL:
  // const endpoint = '@Url.Action("Answer", "[YourControllerName]")';
  const endpoint = 'Answer'; // relative to current controller; adjust if needed

  const body = new URLSearchParams({
    usersAnswer: String(usersAnswer),
    usersCorrectAnswer: String(usersCorrectAnswer),
  }).toString();

  fetch(endpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
    },
    credentials: 'same-origin' // include cookies if needed for auth
    ,body
  })
  .then(r => r.json())
  .then(data => {
    if (data?.success && data?.redirectUrl) {
        nextBuoy()
    } else {
        console.log('Server response:', data);
        nextBuoy()
      
    }
  })
  .catch(err => console.error('Error:', err));
}


function initBuoyProgress() {
    const allBuoys = document.querySelectorAll("[id^='markerBuoy_']");
    if (!allBuoys.length) return;

    const saved = parseInt(localStorage.getItem("buoyIndex") || "0", 10);
    currentBuoyIndex = Number.isFinite(saved) ? Math.min(Math.max(saved, 0), allBuoys.length - 1) : 0;

    allBuoys.forEach(el => el.classList.add("d-none"));
    const current = document.getElementById("markerBuoy_" + currentBuoyIndex);
    if (current) current.classList.remove("d-none");
}

function getCurrentBuoyType() {
    const currentBuoy = document.querySelector('.current-buoy');
    if (!currentBuoy) return null;
    return parseInt(currentBuoy.dataset.buoyType); // this gives you the enum int
}

function nextBuoy() {
    const currentBuoy = document.getElementById("markerBuoy_" + currentBuoyIndex);
    const nextBuoy = document.getElementById("markerBuoy_" + (currentBuoyIndex + 1));

    if (!nextBuoy) {
        console.warn("No more buoys!");
        return;
    }

    nextBuoy.classList.remove("d-none");
    nextBuoy.classList.add("current-buoy");
    if (currentBuoy) {
        currentBuoy.classList.add("d-none");
        currentBuoy.classList.remove("current-buoy");
    }

    currentBuoyIndex++;
    localStorage.setItem("buoyIndex", String(currentBuoyIndex));
}

document.addEventListener("DOMContentLoaded", initBuoyProgress);


