export function revealComment(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        return;
    }

    const margin = 16;
    const rect = element.getBoundingClientRect();
    const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
    const isVisible = rect.top >= margin && rect.bottom <= viewportHeight - margin;
    if (isVisible) {
        return;
    }

    const behavior = matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth";
    element.scrollIntoView({ behavior, block: "nearest", inline: "nearest" });
}
