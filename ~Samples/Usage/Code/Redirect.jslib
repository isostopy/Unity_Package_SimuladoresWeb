var Redirect = {
    GoToURL: function (url) {
        window.location.href = UTF8ToString(url);
    }
};

mergeInto(LibraryManager.library, Redirect);
