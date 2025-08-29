
let currentBuoyIndex = 0;

function initBuoyProgress() {
    const allBuoys = document.querySelectorAll("[id^='markerBuoy_']");
    if (!allBuoys.length) return;

    // Read saved index and clamp to valid range
    const saved = parseInt(localStorage.getItem("buoyIndex") || "0", 10);
    currentBuoyIndex = Number.isFinite(saved) ? Math.min(Math.max(saved, 0), allBuoys.length - 1) : 0;

    // Hide all, then show the current one
    allBuoys.forEach(el => el.classList.add("d-none"));
    const current = document.getElementById("markerBuoy_" + currentBuoyIndex);
    if (current) current.classList.remove("d-none");
}

function nextBuoy() {
    const currentBuoy = document.getElementById("markerBuoy_" + currentBuoyIndex);
    const nextBuoy = document.getElementById("markerBuoy_" + (currentBuoyIndex + 1));

    if (!nextBuoy) {
        console.warn("No more buoys!");
        // Optionally mark completion; comment out to keep last seen:
        // localStorage.setItem("buoyCompleted", "true");
        return;
    }

    nextBuoy.classList.remove("d-none");
    if (currentBuoy) currentBuoy.classList.add("d-none");

    currentBuoyIndex++;
    localStorage.setItem("buoyIndex", String(currentBuoyIndex)); // persist progress
}

// Optional reset button handler
function resetTestProgress() {
    localStorage.removeItem("buoyIndex");
    // localStorage.removeItem("buoyCompleted"); // if you use it
    currentBuoyIndex = 0;
    initBuoyProgress();
}

document.addEventListener("DOMContentLoaded", initBuoyProgress);

