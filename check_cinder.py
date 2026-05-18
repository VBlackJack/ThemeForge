
def get_luminance(hex_color):
    hex_color = hex_color.lstrip('#')
    r = int(hex_color[0:2], 16) / 255.0
    g = int(hex_color[2:4], 16) / 255.0
    b = int(hex_color[4:6], 16) / 255.0
    
    def linearize(c):
        if c <= 0.03928: # Correct threshold is 0.03928 for WCAG 2.x
            return c / 12.92
        else:
            return ((c + 0.055) / 1.055) ** 2.4
            
    rl = linearize(r)
    gl = linearize(g)
    bl = linearize(b)
    
    return 0.2126 * rl + 0.7152 * gl + 0.0722 * bl

cinder_cl = "#3D3A2F"
cinder_com = "#A9A283"

l1 = get_luminance(cinder_com)
l2 = get_luminance(cinder_cl)

print(f"Comment L: {l1:.5f}")
print(f"CL L: {l2:.5f}")
print(f"Contrast: {(l1+0.05)/(l2+0.05):.5f}")
