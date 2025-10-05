from PIL import Image
import os

def create_ico_from_pngs():
    # 利用可能なPNGファイルを収集
    png_files = []
    sizes = [16, 24, 32, 48, 64, 128, 256]
    
    for size in sizes:
        filename = f"icon_{size}.png"
        if os.path.exists(filename):
            png_files.append(filename)
    
    if not png_files:
        print("No PNG files found!")
        return
    
    # 最初の画像を読み込み
    images = []
    for png_file in png_files:
        img = Image.open(png_file)
        images.append(img)
    
    # ICOファイルとして保存
    if images:
        images[0].save('app.ico', format='ICO', sizes=[(img.width, img.height) for img in images])
        print("Created app.ico with multiple sizes")
        
        # ファイルサイズを確認
        size = os.path.getsize('app.ico')
        print(f"app.ico size: {size} bytes")

if __name__ == "__main__":
    create_ico_from_pngs()