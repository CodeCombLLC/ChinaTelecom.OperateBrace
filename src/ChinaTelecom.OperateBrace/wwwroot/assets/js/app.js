
//BACKGROUND CHANGER

$(function () {
    $("#button-bg1").click(function () {
        $.post('/Home/Skin', { style: "cover1" }, function () {
            location.reload();
        });
    });
    $("#button-bg2").click(function () {
        $.post('/Home/Skin', { style: "cover2" }, function () {
            location.reload();
        });
    });


    $("#button-bg3").click(function () {
        $.post('/Home/Skin', { style: "cover3" }, function () {
            location.reload();
        });
    });

    $("#button-bg4").click(function () {
        $.post('/Home/Skin', { style: "cover4" }, function () {
            location.reload();
        });
    });

    $("#button-bg5").click(function () {
        $.post('/Home/Skin', { style: "cover5" }, function () {
            location.reload();
        });
    });

    $("#button-bg6").click(function () {
        $.post('/Home/Skin', { style: "cover6" }, function () {
            location.reload();
        });
    });
    $("#button-bg7").click(function () {
        $.post('/Home/Skin', { style: "cover7" }, function () {
            location.reload();
        });
    });
    $("#button-bg8").click(function () {
        $.post('/Home/Skin', { style: "cover8" }, function () {
            location.reload();
        });
    });

    $("#button-bg9").click(function () {
        $.post('/Home/Skin', { style: "cover9" }, function () {
            location.reload();
        });
    });
    $("#button-bg10").click(function () {
        $.post('/Home/Skin', { style: "cover10" }, function () {
            location.reload();
        });
    });
    $("#button-bg11").click(function () {
        $.post('/Home/Skin', { style: "cover11" }, function () {
            location.reload();
        });
    });

    $("#button-bg12").click(function () {
        $.post('/Home/Skin', { style: "cover12" }, function () {
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
