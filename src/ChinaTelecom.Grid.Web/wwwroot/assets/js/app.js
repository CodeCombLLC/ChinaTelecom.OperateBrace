
//BACKGROUND CHANGER

$(function () {
    $("#button-bg").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg5.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });
    $("#button-bg2").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg2.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });


    $("#button-bg3").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });

    $("#button-bg5").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/giftly.png')repeat" }, function () {
            location.reload();
        });
    });

    $("#button-bg6").click(function () {
        $.post('/Home/Skin', { style: "background:#2c3e50" }, function () {
            location.reload();
        });
    });

    $("#button-bg7").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg3.png')repeat" }, function () {
            location.reload();
        });
    });
    $("#button-bg8").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg8.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });
    $("#button-bg9").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg9.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });

    $("#button-bg10").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg10.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });
    $("#button-bg11").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg11.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });
    $("#button-bg12").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg12.jpg')no-repeat center center fixed" }, function () {
            location.reload();
        });
    });

    $("#button-bg13").click(function () {
        $.post('/Home/Skin', { style: "background:url('/assets/img/bg13.jpg')repeat" }, function () {
            location.reload();
        });
    });
    /**
     * Background Changer end
     */
});

//TOGGLE CLOSE
$('.nav-toggle').click(function () {
    //get collapse content selector
    var collapse_content_selector = $(this).attr('href');

    //make the collapse content to be shown or hide
    var toggle_switch = $(this);
    $(collapse_content_selector).slideToggle(function () {
        if ($(this).css('display') == 'block') {
            //change the button label to be 'Show'
            toggle_switch.html('<span class="entypo-minus-squared"></span>');
        } else {
            //change the button label to be 'Hide'
            toggle_switch.html('<span class="entypo-plus-squared"></span>');
        }
    });
});


$('.nav-toggle-alt').click(function () {
    //get collapse content selector
    var collapse_content_selector = $(this).attr('href');

    //make the collapse content to be shown or hide
    var toggle_switch = $(this);
    $(collapse_content_selector).slideToggle(function () {
        if ($(this).css('display') == 'block') {
            //change the button label to be 'Show'
            toggle_switch.html('<span class="entypo-up-open"></span>');
        } else {
            //change the button label to be 'Hide'
            toggle_switch.html('<span class="entypo-down-open"></span>');
        }
    });
    return false;
});
//CLOSE ELEMENT
$(".gone").click(function () {
    var collapse_content_close = $(this).attr('href');
    $(collapse_content_close).hide();
});

//tooltip
$('.tooltitle').tooltip();

function Resize() {
    $('.paper-wrap').css('min-height', $(window).height() - 75 + "px");
}

$(document).ready(function () {
    Resize();
    $(window).resize(function () { Resize(); });
});
