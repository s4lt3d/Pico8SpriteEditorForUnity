using UnityEngine;
using UnityEditor;
using System.Drawing.Printing;
using System.Data;

public class Pico8SpriteEditor : EditorWindow
{
    private Texture2D textureToPreview;
    private Texture2D palletTexture;
    private Texture2D textureSizeTexture;
    private Texture2D brushTexture;
    private Texture2D stampTexture;
    private Texture2D selectTexture;
    private Texture2D handTexture;
    private Texture2D paintTexture;
    private Texture2D circleTexture;


    private Rect textureRect;
    private Rect palletRect;
    private Rect spriteRect;
    private Rect smallTextureRect; 
    private Rect spriteNumberRect; 
    private Rect zoomTextureRect;
    private Rect zoomSliderRect;
    private Rect spriteSheetRect;

    private Rect brushIconRect;
    private Rect stampIconRect;
    private Rect selectIconRect;
    private Rect handIconRect;
    private Rect paintIconRect;
    private Rect circleIconRect;

    private int palletSelection = 0;
    private int spriteSelection = 0;
    private Color palletColor = Color.black;
    private int zoomFactor = 0;

    private int textureZoom = 0;

    int spritesPerRow = 16;
    int colorsPerRow = 4;
    int selectionRectThickness = 4;

    private bool shouldSave = false;

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
        brushTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/brush.png"); ;
        stampTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/stamp.png"); ;
        selectTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/select.png"); ;
        handTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/hand.png"); ;
        paintTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/paint.png"); ;
        circleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/circle.png"); ;

        colorsPerRow = palletTexture.width;
    }

    private void OnEnable()
    {
        LoadTexture();
        textureRect = new Rect(30, 15, 256, 256);
        
        
        palletRect = new Rect(256 + 30 + 15, 15, 128, 128);
        zoomSliderRect = new Rect(256 + 30 + 15 + 40 , 128 + 30 + 30, 140, 20);
        zoomTextureRect = new Rect(256 + 30 + 15, 128 + 30 + 24, 32, 32);
        spriteSheetRect = new Rect(30, 256 + 30 + 30, 512, 512);


        smallTextureRect = new Rect(256 + 20, 256 + 21, 32, 32);
        spriteNumberRect = new Rect(256 + 20 + 32, 256 + 22, 64, 36);
        brushIconRect = new Rect(30, 256 + 21, 32, 32);
        stampIconRect = new Rect(30 + 40 * 1, 256 + 21, 32, 32);
        selectIconRect = new Rect(30 + 40 * 2, 256 + 21, 32, 32);
        handIconRect = new Rect(30 + 40 * 3, 256 + 21, 32, 32);
        paintIconRect = new Rect(30 + 40 * 4, 256 + 21, 32, 32);
        circleIconRect = new Rect(30 + 40 * 5, 256 + 21, 32, 32);

}

    private void DrawThickRectangle(Rect rectangle, float thickness, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;
        Rect topBorderRect = new Rect(rectangle.x, rectangle.y, rectangle.width, thickness);
        Rect bottomBorderRect = new Rect(rectangle.x, rectangle.y + rectangle.height - thickness, rectangle.width, thickness);
        Rect leftBorderRect = new Rect(rectangle.x, rectangle.y, thickness, rectangle.height);
        Rect rightBorderRect = new Rect(rectangle.x + rectangle.width - thickness, rectangle.y, thickness, rectangle.height);

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
        textureZoom = EditorGUI.IntSlider(zoomSliderRect, "", textureZoom, 0, 3);

        zoomFactor = 8 * (int)Mathf.Pow(2, textureZoom);

        HandleMouseClicks();

        DrawThickRectangle(textureRect, 1, Color.black);

        if (textureToPreview != null)
        {
            Rect spriteRect = GridConversion.ToRect(spriteSelection, spritesPerRow, zoomFactor, zoomFactor);
            spriteRect.x *= 8;
            spriteRect.y *= 8;
            DrawTexture(textureToPreview, textureRect, spriteRect);
            DrawTexture(textureToPreview, smallTextureRect, spriteRect);
            DrawTexture(textureToPreview, spriteSheetRect);
            DrawTexture(brushTexture, brushIconRect);
            DrawTexture(stampTexture, stampIconRect);
            DrawTexture(selectTexture, selectIconRect);
            DrawTexture(handTexture, handIconRect);
            DrawTexture(paintTexture, paintIconRect);
            DrawTexture(circleTexture, circleIconRect);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 24; // Example: Change font size
            labelStyle.normal.textColor = Color.gray; // Example: Change text color
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleLeft; // Example: Center the text

            // Draw the label with the custom style
            GUI.Label(spriteNumberRect, spriteSelection.ToString("000"), labelStyle);
            
        }

        if (palletTexture != null)
        {
            DrawTexture(palletTexture, palletRect);
        }

        if (textureSizeTexture != null)
        {
            DrawTexture(textureSizeTexture, zoomTextureRect);
        }

        DrawPalletSelection();
        DrawSpriteSelection();
    }

    private void DrawSpriteSelection()
    {
        float spriteSelectionWidth =  spriteSheetRect.width / spritesPerRow;
        float spriteSelectionHeight =  spriteSheetRect.height / spritesPerRow; 

        Vector2 gridPosition = GridConversion.ToVector(spriteSelection, spritesPerRow);

        Rect sheetPosition = new Rect(
            spriteSheetRect.x + gridPosition.x * spriteSelectionWidth,
            spriteSheetRect.y + gridPosition.y * spriteSelectionHeight,
            spriteSelectionWidth * (int)Mathf.Pow(2, textureZoom), spriteSelectionHeight * (int)Mathf.Pow(2, textureZoom));

        DrawThickRectangle(sheetPosition, selectionRectThickness, Color.white);
    }

    private void DrawPalletSelection()
    {
        float palletSelectionWidth = palletRect.width / colorsPerRow; 
        float palletSelectionHeight = palletRect.height / palletTexture.height;
        Vector2 gridPosition = GridConversion.ToVector(palletSelection, colorsPerRow);

        Rect palletPosition = new Rect(
            palletRect.x + gridPosition.x * palletSelectionWidth,
            palletRect.y + gridPosition.y * palletSelectionHeight,
            palletSelectionWidth, palletSelectionHeight);

        DrawThickRectangle(palletPosition, selectionRectThickness, Color.white);
    }


    private void SetPixel(int x, int y, Color c)
    {
        textureToPreview.SetPixel(x, y, c);
        textureToPreview.Apply(); 
    }

    

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

                Rect spriteRect = GridConversion.ToRect(spriteSelection, spritesPerRow, zoomFactor, zoomFactor);
                spriteRect.x *= 8;
                spriteRect.y *= 8;

                originalX += (int)spriteRect.x;
                originalY += (int)spriteRect.y;

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
                if (palletColor == Color.black)
                    palletColor = new Color(0, 0, 0, 0);
                Repaint();
            }
            else if(spriteSheetRect.Contains(mousePos)) {
                mousePos.x -= spriteSheetRect.x;
                mousePos.y -= spriteSheetRect.y;
                mousePos /= 32f;
                spriteSelection = (int)mousePos.x + (int)mousePos.y * 16;
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

public class GridConversion
{
    public static int ToIndex(int x, int y, int gridWidth)
    {
        return y * gridWidth + x;
    }

    public static Vector2 ToVector(int index, int gridWidth)
    {
        Vector2 grid = Vector2.zero;
        grid.x = index % gridWidth;
        grid.y = index / gridWidth; 
        return grid;
    }

    public static Rect ToRect(int index, int gridWidth, int width=0, int height=0)
    {
        Vector2 v = ToVector(index, gridWidth);
        Rect r = new Rect(v.x, v.y, width, height);
        return r;
    }
}