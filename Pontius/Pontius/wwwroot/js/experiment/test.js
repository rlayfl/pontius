
let currentBuoyIndex = 0;

function answer(answer, correctAnswer) {
    
    alert("Correct Answer: " + correctAnswer + "Your Answer: " + answer)

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
    if (currentBuoy) currentBuoy.classList.add("d-none");

    currentBuoyIndex++;
    localStorage.setItem("buoyIndex", String(currentBuoyIndex));
}

document.addEventListener("DOMContentLoaded", initBuoyProgress);


