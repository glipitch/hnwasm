let startRef;
let endRef;
export function attach(dotNetObject) {
    startRef = function handler(event) {
        const touches = event.touches[0];
        dotNetObject.invokeMethod('TouchStart', touches.clientX, touches.clientY);
    }
    document.addEventListener("touchstart", startRef);

    endRef = function handler(event) {
        const changedTouches = event.changedTouches[0];
        dotNetObject.invokeMethod('TouchEnd', changedTouches.clientX, changedTouches.clientY);
    }
    document.addEventListener("touchend", endRef);
}
export function detach() {
    document.removeEventListener("touchstart", startRef);
    document.removeEventListener("touchend", endRef);
}