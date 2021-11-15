// ==UserScript==
// @name        YChanEx Download Buttons
// @namespace   murrty
// @description Adds a download button to threads on supported boards. May not work for all sites.
// @include     https://boards.4chan.org/*/thread/*
// @include     http://boards.4chan.org/*/thread/*
// @include     https://boards.4channel.org/*/thread/*
// @include     http://boards.4channel.org/*/thread/*
// @include     https://boards.420chan.org/*/res/*
// @include     http://boards.420chan.org/*/res/*
// @include     https://7chan.org/*/res/*
// @include     http://7chan.org/*/res/*
// @include     https://www.7chan.org/*/res/*
// @include     http://www.7chan.org/*/res/*
// @include     https://8ch.net/*/res/*
// @include     http://8ch.net/*/res/*
// @include     https://u18chan.com/board/u18chan/*/topic/*
// @include     http://u18chan.com/board/u18chan/*/topic/*
// @version     1.0
// @updateURL   https://github.com/murrty/YChanEx/raw/master/Resources/YChanEx.user.js
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
else if (document.URL.indexOf('8ch.net/') > -1) {
    chan = 3;
}
else if (document.URL.indexOf('fchan.us/') > -1) {
    chan = 4;
}
else if (document.URL.indexOf('u18chan.com/') > -1) {
    chan = 5;
}

if (chan == 0) {
    // 4CHAN
    divelement = document.getElementById('op');
    
    downloadlink.className = "qr-link";
}
else if (chan == 1) {
    // 420CHAN
    divelement = document.getElementById('delform');

    downloadlink.className = "fg-button ui-state-default";
    downloadlink.style = "padding: 2px 4px 2px;"
}
else if (chan == 2) {
    //7chan
    divelement = document.getElementById('delform');
}
else if (chan == 3) { 
    //8chan
    divelement = document.getElementById('thread_' + document.URL.split('/')[5].replace(".html", ""));
}
else if (chan == 4) {
    //fchan
    // not viable at this time.
}
else if (chan == 5) {
    //u18chan
    divelement = document.getElementById('FirstPost');
  
    downloadlink.className = "TagBox";
    url = document.URL.replace("board/u18chan/", "");
}

downloadlink.href = "ychanex:" + url;
downloaddiv.appendChild(downloadlink);
divelement.parentNode.insertBefore(downloaddiv, divelement);