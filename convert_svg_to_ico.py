#!/usr/bin/env python3
"""Create a water drop ICO file using PIL"""

from PIL import Image, ImageDraw
import os

def create_water_drop_ico():
    """Create a water drop icon using PIL drawing"""
    
    ico_path = r"AquaParkManager\Resources\water_drop.ico"
    
    # Create images for different sizes
    sizes = [256, 128, 64, 32, 16]
    images = []
    
    for size in sizes:
        # Create a new RGBA image with transparent background
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # Calculate coordinates for the water drop
        # Drop goes from top (8) to bottom (52) in original SVG with viewBox 64
        # Scale to current size
        scale = size / 64.0
        
        top = int(8 * scale)
        bottom = int(52 * scale)
        left = int(20 * scale)
        right = int(44 * scale)
        mid = int(32 * scale)
        
        # Draw water drop - teardrop shape using polygon
        # Top point
        points = [
            (mid, top),  # top point
        ]
        
        # Right curve (quadratic)
        for i in range(1, 9):
            t = i / 8.0
            x = mid + (right - mid) * (1 - (1 - t) ** 2)
            y = top + (bottom - top) * (t ** 2)
            points.append((x, y))
        
        # Bottom point
        points.append((mid, bottom))
        
        # Left curve (quadratic)
        for i in range(8, 0, -1):
            t = i / 8.0
            x = mid - (mid - left) * (1 - (1 - t) ** 2)
            y = top + (bottom - top) * (t ** 2)
            points.append((x, y))
        
        # Draw filled drop with gradient effect
        # Main drop - blue gradient from light to dark
        draw.polygon(points, fill=(0, 153, 255, 255), outline=(0, 119, 204, 255))
        
        # Add highlight
        highlight_size = max(4, int(6 * scale))
        highlight_x = int(28 * scale)
        highlight_y = int(24 * scale)
        draw.ellipse(
            [highlight_x - highlight_size, highlight_y - highlight_size * 1.5,
             highlight_x + highlight_size, highlight_y + highlight_size // 2],
            fill=(255, 255, 255, 100)
        )
        
        images.append(img)
    
    # Save as multi-resolution ICO
    images[0].save(
        ico_path,
        format='ICO',
        sizes=[(s, s) for s in sizes]
    )
    
    print(f"✓ Successfully created water drop icon: {ico_path}")
    return ico_path

if __name__ == '__main__':
    try:
        create_water_drop_ico()
    except Exception as e:
        print(f"✗ Error creating icon: {e}")
        import traceback
        traceback.print_exc()
