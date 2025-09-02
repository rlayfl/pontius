
let currentBuoyIndex = 0;

function clearLocalStorage() {
    localStorage.clear();
}

function answer(usersAnswer, usersCorrectAnswer, experimentType) {
//alert(`Experiment Type: ${experimentType}\nCorrect Answer: ${usersCorrectAnswer}\nYour Answer: ${usersAnswer}`);


  const endpoint = 'Answer';

  const body = new URLSearchParams({
    usersAnswer: String(usersAnswer),
    usersCorrectAnswer: String(usersCorrectAnswer),
    experimentType: String(experimentType)
  }).toString();

  fetch(endpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
    },
    credentials: 'same-origin'
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

    if (localStorage.getItem("experimentFinished") === "true") {
        alert("You have already completed this experiment.");
        return;
    }

    const current = document.getElementById("markerBuoy_" + currentBuoyIndex);
    if (current) {
        allBuoys.forEach(el => el.classList.remove("current-buoy"));
        current.classList.remove("d-none");
        current.classList.add("current-buoy");
        
    }
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
        alert("You have completed this experiment.")
        currentBuoy.classList.add("d-none");
        localStorage.setItem("experimentFinished", String(true));
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


