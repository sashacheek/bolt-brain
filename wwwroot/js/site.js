function openMenu() {
    document.getElementById("side-menu").style.transform = "translateX(250px)";
}

function closeMenu() {
    document.getElementById("side-menu").style.transform = "translateX(-250px)";
}

// rewrite following function

const menu = document.getElementById('side-menu');
const menuButton = document.getElementById('open-menu');

document.addEventListener('click', (event) => {
    if (!menu.contains(event.target) && !menuButton.contains(event.target)){
        closeMenu();
    }
});

function fixLength(input, max) {
    if (input.value.length >= max) input.value = input.value.slice(0, input.max + 1);
}