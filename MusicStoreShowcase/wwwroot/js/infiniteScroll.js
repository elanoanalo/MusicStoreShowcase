// wwwroot/js/infiniteScroll.js
// Uses IntersectionObserver to detect when the bottom "scroll-trigger" div
// becomes visible, then calls back into the Blazor component to load more data.

window._infiniteScrollObservers = window._infiniteScrollObservers || {};

window.initScrollListener = function (dotNetHelper) {
    // Always clean up any previous observer first (important because
    // OnAfterRenderAsync can fire again, e.g. after view-mode toggles).
    window.disposeScrollListener();

    const target = document.getElementById('scroll-trigger');
    if (!target) {
        console.warn('initScrollListener: #scroll-trigger element not found yet.');
        return;
    }

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                dotNetHelper.invokeMethodAsync('LoadMoreTracks');
            }
        });
    }, {
        root: null,        // viewport
        rootMargin: '200px', // start loading a bit before it's fully in view
        threshold: 0
    });

    observer.observe(target);

    window._infiniteScrollObservers.current = observer;
    window._infiniteScrollObservers.dotNetHelper = dotNetHelper;
};

window.disposeScrollListener = function () {
    if (window._infiniteScrollObservers.current) {
        window._infiniteScrollObservers.current.disconnect();
        window._infiniteScrollObservers.current = null;
    }
};

// Called from C# whenever generation parameters change (seed/language),
// so the Gallery View visually returns to its starting scroll position,
// matching the "Reset the Gallery View to the initial scroll position"
// requirement.
window.scrollGalleryToTop = function () {
    window.scrollTo({ top: 0, behavior: 'instant' });
};