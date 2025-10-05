from PIL import Image, ImageDraw
import os

def create_icon(size, filename):
    # 透明な背景で画像を作成
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # マージンを設定
    margin = max(2, size // 16)
    inner_size = size - 2 * margin
    
    # 外側の枠（メイン枠）
    outer_thickness = max(2, size // 16)
    draw.rectangle(
        [margin, margin, margin + inner_size, margin + inner_size],
        outline=(220, 50, 50, 255),  # 赤色
        width=outer_thickness
    )
    
    # 内側の小さな枠（アクセント）
    if size >= 32:
        inner_margin = margin + size // 4
        inner_inner_size = inner_size - 2 * (size // 4)
        inner_thickness = max(1, size // 24)
        
        draw.rectangle(
            [inner_margin, inner_margin, inner_margin + inner_inner_size, inner_margin + inner_inner_size],
            outline=(255, 100, 100, 255),  # 明るい赤色
            width=inner_thickness
        )
    
    # 角にドット（設定を表現）
    if size >= 24:
        dot_size = max(2, size // 16)
        # 右上角
        draw.ellipse(
            [size - margin - dot_size - 2, margin + 2, 
             size - margin - 2, margin + dot_size + 2],
            fill=(255, 255, 255, 255)
        )
    
    img.save(filename)
    print(f"Created {filename} ({size}x{size})")

def main():
    # 各サイズのアイコンを作成
    sizes = [16, 24, 32, 48, 64, 128, 256]
    
    for size in sizes:
        filename = f"icon_{size}.png"
        create_icon(size, filename)
    
    # メインアイコンとして256pxをコピー
    if os.path.exists("icon_256.png"):
        import shutil
        shutil.copy("icon_256.png", "app_icon.png")
        print("Created app_icon.png (main icon)")

if __name__ == "__main__":
    main()