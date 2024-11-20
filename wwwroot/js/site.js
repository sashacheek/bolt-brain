function openMenu() {
    document.getElementById("side-menu").style.transform = "translateX(250px)";
}

function closeMenu() {
    document.getElementById("side-menu").style.transform = "translateX(-250px)";
}

const menu = document.getElementById('side-menu');
const menuButton = document.getElementById('open-menu');

document.addEventListener('click', (event) => {
    if (!menu.contains(event.target) && !menuButton.contains(event.target)){
        closeMenu();
    }
});