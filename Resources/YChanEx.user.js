// ==UserScript==
// @name        YChanEx Download Buttons
// @namespace   murrty
// @description Adds a download button to threads on supported boards. May not work for all sites.
// @include     http*://boards.4chan.org/*/thread/*
// @include     http*://boards.4channel.org/*/thread/*
// @include     http*://boards.420chan.org/*/res/*
// @include     https://*.7chan.org/*/res/*
// @include     http*://8ch.net/*/res/*
// @include     http*://8chan.moe/*/res/*
// @include     http*://u18chan.com/board/u18chan/*/topic/*
// @version     1.0
// @updateURL   https://raw.githubusercontent.com/murrty/YChanEx/master/Resources/YChanEx.user.js
// @grant       none
// ==/UserScript==

var downloaddiv = document.createElement('div');
var downloadlink = document.createElement('a');
var divelement;
var chan = -1;
var url = document.URL;

downloaddiv.id = "downloadDiv";

downloadlink.id = "download-thread";
downloadlink.title = "Download this thread";
downloadlink.appendChild(document.createTextNode('download thread'));

if (document.URL.indexOf('4chan.org/') > -1 || document.URL.indexOf('4channel.org/') > -1) {
    chan = 0;
}
else if (document.URL.indexOf('420chan.org/') > -1) {
    chan = 1;
}
else if (document.URL.indexOf('7chan.org/') > -1) {
    chan = 2;
}
else if (document.URL.indexOf('8ch.net/') > -1 || document.URL.indexOf('8chan.moe') > -1) {
    chan = 3;
}
else if (document.URL.indexOf('fchan.us/') > -1) {
    chan = 4;
}
else if (document.URL.indexOf('u18chan.com/') > -1) {
    chan = 5;
}

switch (chan) {
    case 0: {
        // 4CHAN
        divelement = document.getElementById('op');
    
        downloadlink.className = "qr-link";
    } break;

    case 1: {
        // 420CHAN
        divelement = document.getElementById('delform');

        downloadlink.className = "fg-button ui-state-default";
        downloadlink.style = "padding: 2px 4px 2px;"
    } break;

    case 2: {
        //7chan
        divelement = document.getElementById('delform');
    } break;

    case 3: {
        //8chan
        divelement = document.getElementById('thread_' + document.URL.split('/')[5].replace(".html", ""));
    } break;
    
    case 4: {
        //fchan
        // not viable at this time.
    } break;

    case 5: {
        //u18chan
        divelement = document.getElementById('FirstPost');
  
        downloadlink.className = "TagBox";
        url = document.URL.replace("board/u18chan/", "");
    } break;
}

downloadlink.href = "ychanex:" + url;
downloaddiv.appendChild(downloadlink);
divelement.parentNode.insertBefore(downloaddiv, divelement);