using UnityEditor;
using UnityEngine;

public class SpritePostProcessor : AssetPostprocessor
{
    // Этот метод вызывается автоматически при импорте любой текстуры
    void OnPreprocessTexture()
    {
        // Скрипт будет работать только для файлов, в пути которых есть слово "Sprites"
        // (Создайте в проекте папку Sprites и кидайте всё туда) 
        if (assetPath.Contains("Assets/Project/Art/Sprites"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;

            // Устанавливаем настройки для Pixel Art
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple; // Сразу ставит Multiple
            textureImporter.spritePixelsPerUnit = 32; // Укажите ваш стандартный PPU (16, 32, 64)

            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}