function blazorGetTimezoneOffset() {
    return new Date().getTimezoneOffset();
}

function scrollTop() {
    $('.message-block').animate({
        scrollTop: $('.message-block-inner').height()
    });
}
