
function hexToRgb(hex) {
    const r = parseInt(hex.slice(1, 3), 16) / 255;
    const g = parseInt(hex.slice(3, 5), 16) / 255;
    const b = parseInt(hex.slice(5, 7), 16) / 255;
    return [r, g, b];
}

function rgbToHsl(r, g, b) {
    const max = Math.max(r, g, b), min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;

    if (max === min) {
        h = s = 0; // achromatic
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = (g - b) / d + (g < b ? 6 : 0); break;
            case g: h = (b - r) / d + 2; break;
            case b: h = (r - g) / d + 4; break;
        }
        h /= 6;
    }

    return [h * 360, s * 100, l * 100];
}

const darks = [
    { name: "Striga", bg: "#28211F", comment: "#B0948D" },
    { name: "Ember",  bg: "#28271F", comment: "#B0AA8D" },
    { name: "Yew",    bg: "#21281F", comment: "#92B08D" },
    { name: "Tarn",   bg: "#1F2825", comment: "#8DB0A4" },
    { name: "Mortis", bg: "#1F2528", comment: "#8DA4B0" },
    { name: "Slate",  bg: "#1F2228", comment: "#8D98B0" },
    { name: "Voivode",bg: "#211F28", comment: "#928DB0" },
    { name: "Carmilla",bg: "#261F28", comment: "#A78DB0" },
    { name: "Whitby", bg: "#281F26", comment: "#B08DA7" },
    { name: "Vesper", bg: "#281F22", comment: "#B08D98" }
];

console.log("Comments HSL:");
darks.forEach(d => {
    const [h, s, l] = rgbToHsl(...hexToRgb(d.comment));
    console.log(`${d.name.padEnd(10)} | H:${h.toFixed(1)} S:${s.toFixed(1)} L:${l.toFixed(1)}`);
});

console.log("\nBg HSL:");
darks.forEach(d => {
    const [h, s, l] = rgbToHsl(...hexToRgb(d.bg));
    console.log(`${d.name.padEnd(10)} | H:${h.toFixed(1)} S:${s.toFixed(1)} L:${l.toFixed(1)}`);
});
