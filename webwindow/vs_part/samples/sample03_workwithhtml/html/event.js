"use strict";
window.onload = function () {
    var btn = document.getElementById('btn');
    btn.onclick = function () {
        var txt = document.getElementById('textbox');
        var __api = parent["__api"];
        __api.sendback(['click', txt.value]);
    };
};
