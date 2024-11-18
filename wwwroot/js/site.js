// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function openMenu() {
    document.getElementById("side-menu").style.transform = "translateX(250px)";
}

function closeMenu() {
    document.getElementById("side-menu").style.transform = "translateX(-250px)";
}