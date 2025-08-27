//Log clicks
let clickCount = 0;

document.addEventListener("click", function () {
    clickCount++;

    // Send the count to the server
    fetch('/statistics/logClick', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ count: 1 }) // just log one click at a time
    });
});