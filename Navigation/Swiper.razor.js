let startRef;
let endRef;

export function attach(dotNetObject, sensitivity) {
    let startX, startY, startTime;

    startRef = function handler(event) {
        const touch = event.touches[0];
        startX = touch.clientX;
        startY = touch.clientY;
        startTime = performance.now();
    };

    endRef = function handler(event) {
        if (startX == null || startY == null || performance.now() - startTime > 300) {
            startX = startY = null;
            return;
        }

        const touch = event.changedTouches[0];
        const deltaX = startX - touch.clientX;
        const deltaY = startY - touch.clientY;
        startX = startY = null;

        if (Math.abs(deltaX) < sensitivity && Math.abs(deltaY) < sensitivity) {
            return;
        }
        if (Math.abs(deltaX) > Math.abs(deltaY)) {
            dotNetObject.invokeMethodAsync("Swipe", deltaX > 0 ? "rightToLeft" : "leftToRight");
        }
    };

    document.addEventListener("touchstart", startRef);
    document.addEventListener("touchend", endRef);
}

export function detach() {
    document.removeEventListener("touchstart", startRef);
    document.removeEventListener("touchend", endRef);
}
