window._infiniteScrollObservers = window._infiniteScrollObservers || {};

window.initScrollListener = function (dotNetHelper) {

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
        root: null,        
        rootMargin: '200px', 
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

window.scrollGalleryToTop = function () {
    window.scrollTo({ top: 0, behavior: 'instant' });
};