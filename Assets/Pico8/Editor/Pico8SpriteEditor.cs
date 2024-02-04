using UnityEngine;
using UnityEditor;
using System.Drawing.Printing;
using System.Data;

public class Pico8SpriteEditor : EditorWindow
{
    private Texture2D textureToPreview;
    private Texture2D palletTexture;
    private Texture2D textureSizeTexture;
    private Rect textureRect;
    private Rect palletRect;
    private Rect spriteRect;
    private Rect zoomTextureRect;
    private Rect zoomSliderRect;
    private Rect spriteSheetRect;

    private int palletSelection = 0;
    private int spriteSelection = 0;
    private Color palletColor = Color.black;
    private int zoomFactor = 0;

    private int textureZoom = 0;

    int spritesPerRow = 16;

    [MenuItem("Tools/Pico-8 Palette Tool")]
    private static void ShowWindow()
    {
        var window = GetWindow<Pico8SpriteEditor>();
        window.titleContent = new GUIContent("Pico-8 Sprite Editor");

        Vector2 windowSize = new Vector2(512 + 60, 256+512+30+30+30); // Example size, adjust as needed
        window.minSize = windowSize;
        window.maxSize = windowSize;

        window.Show();
    }

    void LoadTexture()
    {
        textureToPreview = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/test.png");
        palletTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/pallet.png");
        textureSizeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/texturesize.png");
    }

    private void OnEnable()
    {
        LoadTexture();
        textureRect = new Rect(30, 15, 256, 256);
        palletRect = new Rect(256 + 30 + 15, 15, 128, 128);
        zoomSliderRect = new Rect(256 + 30 + 15 + 40 , 128 + 30 + 30, 140, 20);
        zoomTextureRect = new Rect(256 + 30 + 15, 128 + 30 + 24, 32, 32);
        spriteSheetRect = new Rect(30, 256 + 30 + 30, 512, 512);
    }

    private void DrawThickRectangle(Vector2 position, float width, float height, float thickness, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;
        Rect topBorderRect = new Rect(position.x, position.y, width, thickness);
        Rect bottomBorderRect = new Rect(position.x, position.y + height - thickness, width, thickness);
        Rect leftBorderRect = new Rect(position.x, position.y, thickness, height);
        Rect rightBorderRect = new Rect(position.x + width - thickness, position.y, thickness, height);
        DrawSolidRectangle(topBorderRect);
        DrawSolidRectangle(bottomBorderRect);
        DrawSolidRectangle(leftBorderRect);
        DrawSolidRectangle(rightBorderRect);
        Handles.EndGUI();
    }

    private void DrawSolidRectangle(Rect rect)
    {
        Vector3[] rectangleCorners = {
            new Vector3(rect.x, rect.y, 0),
            new Vector3(rect.x + rect.width, rect.y, 0),
            new Vector3(rect.x + rect.width, rect.y + rect.height, 0),
            new Vector3(rect.x, rect.y + rect.height, 0)
        };

        Handles.DrawSolidRectangleWithOutline(rectangleCorners, Handles.color, Color.clear);
    }

    private void OnGUI()
    {
        zoomFactor = 8 * (int)Mathf.Pow(2, textureZoom);

        HandleMouseClicks();

        if (textureToPreview != null)
        {
            DrawTexture(textureToPreview, textureRect, new Rect(0, 0, zoomFactor, zoomFactor));
            DrawTexture(textureToPreview, spriteSheetRect);
        }

        if (palletTexture != null)
        {
            DrawTexture(palletTexture, palletRect);
        }

        if (textureSizeTexture != null)
        {
            DrawTexture(textureSizeTexture, zoomTextureRect);
        }

        Vector2 vector2 = new Vector2();
        vector2.x = palletSelection % palletTexture.width;
        vector2.y = palletSelection / palletTexture.height;

        Vector2 palletPosition = new Vector2(palletRect.x - 2 + vector2.x * (palletTexture.width / palletTexture.width), palletRect.y- 2 + vector2.y * (palletTexture.height / palletTexture.height)); // X, Y position
        float thickness = 4;
        float width = palletRect.width / palletTexture.width + thickness;
        float height = palletRect.height / palletTexture.height + thickness;
        

        DrawThickRectangle(palletPosition, width, height, thickness, Color.white);

        Vector2 sprintRectVect = new Vector2();
        sprintRectVect.x = spriteSelection % spritesPerRow;
        sprintRectVect.y = spriteSelection / spritesPerRow;

        Vector2 sheetposition = new Vector2(spriteSheetRect.x - 2 + sprintRectVect.x * spriteSheetRect.width / spritesPerRow, spriteSheetRect.y - 2 + sprintRectVect.y * spritesPerRow); // X, Y position
        DrawThickRectangle(sheetposition, width, height, thickness, Color.white);

        textureZoom = EditorGUI.IntSlider(zoomSliderRect, "", textureZoom, 0, 3);
    }


    private void SetPixel(int x, int y, Color c)
    {
        textureToPreview.SetPixel(x, y, c);
        textureToPreview.Apply(); 
    }

    bool shouldSave = false;

    private void HandleMouseClicks()
    {
        Event e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0) 
        {
            Vector2 mousePos = e.mousePosition;
            if (textureRect.Contains(mousePos))
            {
                float scaleFactor = textureRect.width / zoomFactor;

                float adjustedX = mousePos.x - textureRect.x;
                float adjustedY = (mousePos.y - textureRect.y);
                int originalX = (int)(adjustedX / scaleFactor);
                int originalY = (int)(adjustedY / scaleFactor);

                Debug.Log($"Clicked on textureToPreview at {originalX} {originalY}");
                shouldSave = true;
                SetPixel(originalX, textureToPreview.height - originalY - 1, palletColor);
                Repaint();

            }
            else if (palletRect.Contains(mousePos))
            {
                mousePos.x -= palletRect.x;
                mousePos.y -= palletRect.y;
                mousePos /= 32f;
                palletSelection = (int)mousePos.x + (int)mousePos.y * (int)palletTexture.width;
                palletColor = palletTexture.GetPixel((int)mousePos.x, (int)palletTexture.height - (int)mousePos.y - 1);                
                Repaint();
            }
            else if(spriteSheetRect.Contains(mousePos)) {
                mousePos.x -= spriteSheetRect.x;
                mousePos.y -= spriteSheetRect.y;
                mousePos /= 32f;
                spriteSelection = (int)mousePos.x + (int)mousePos.y * 32;
                Repaint();

            }
        }

        if((e.type == EventType.MouseLeaveWindow || e.type == EventType.MouseUp))
        {
            if(shouldSave) {
                byte[] bytes = textureToPreview.EncodeToPNG();
                var path = "Assets/test.png";
                if (path.Length != 0)
                {
                    System.IO.File.WriteAllBytes(path, bytes);
                    Debug.Log($"Texture saved to: {path}");
                    AssetDatabase.Refresh();
                }
            }
        }
    }

    private void DrawTexture(Texture2D texture, Rect position, Rect sourceRect = new Rect())
    {
        if (sourceRect.width == 0)
        {
            GUIStyle textureStyle = new GUIStyle { normal = { background = texture } };
            GUI.Label(position, GUIContent.none, textureStyle);
        }
        else
        {
            // Invert the Y coordinate for the sourceRect to align with GUI's top-left origin
            float yInverted = texture.height - sourceRect.y - sourceRect.height;

            // Convert sourceRect from pixel coordinates to normalized coordinates, with Y-axis inverted
            float xMin = sourceRect.x / texture.width;
            float xMax = (sourceRect.x + sourceRect.width) / texture.width;
            // Use yInverted for the Y coordinate calculations
            float yMin = yInverted / texture.height;
            float yMax = (yInverted + sourceRect.height) / texture.height;

            Rect texCoords = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            GUI.DrawTextureWithTexCoords(position, texture, texCoords, true);
        }
    }
}

