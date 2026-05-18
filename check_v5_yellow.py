
import math

def get_linear_rgb(hex_color):
    hex_color = hex_color.lstrip('#')
    r = int(hex_color[0:2], 16) / 255.0
    g = int(hex_color[2:4], 16) / 255.0
    b = int(hex_color[4:6], 16) / 255.0
    
    def linearize(c):
        if c <= 0.04045:
            return c / 12.92
        else:
            return ((c + 0.055) / 1.055) ** 2.4
            
    return linearize(r), linearize(g), linearize(b)

def get_luminance(hex_color):
    rl, gl, bl = get_linear_rgb(hex_color)
    return 0.2126 * rl + 0.7152 * gl + 0.0722 * bl

cinder_bg = "#28271F"
yellow = "#ABAB24"

l_bg = get_luminance(cinder_bg)
l_y = get_luminance(yellow)

print(f"Cinder Bg L: {l_bg:.5f}")
print(f"Yellow L: {l_y:.5f}")
print(f"Contrast: {(l_y+0.05)/(l_bg+0.05):.5f}")

# Compare with v4 Yellow #9F9F22
yellow_v4 = "#9F9F22"
l_y4 = get_luminance(yellow_v4)
print(f"v4 Yellow L: {l_y4:.5f}")
print(f"v4 Contrast: {(l_y4+0.05)/(l_bg+0.05):.5f}")
