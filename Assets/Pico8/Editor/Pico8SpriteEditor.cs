using System.IO;
using UnityEditor;
using UnityEngine;

enum Pico9SpriteEditorState
{
    Brush,
    Stamp,
    Select,
    Hand,
    Paint,
    Circle
}  

public class Pico8SpriteEditor : EditorWindow
{
    private readonly int selectionRectThickness = 4;

    private readonly int spritesPerRow = 16;

    private Rect brushIconRect;
    private Texture2D brushTexture;
    private Rect circleIconRect;
    private Texture2D circleTexture;
    private int colorsPerRow = 4;
    private Rect handIconRect;
    private Texture2D handTexture;
    private Rect paintIconRect;
    private Texture2D paintTexture;
    private Color palletColor = Color.black;
    private Rect palletRect;

    private int palletSelection;
    private Texture2D palletTexture;
    private Rect selectIconRect;
    private Texture2D selectTexture;

    private bool shouldSave;
    private Rect smallTextureRect;
    private Rect spriteNumberRect;
    private Rect spriteRect;
    private int spriteSelection;
    private Rect spriteSheetRect;
    private Texture2D spriteSheetTexture;
    private Rect stampIconRect;
    private Texture2D stampTexture;

    private Rect textureRect;

    private int textureZoom;
    private int zoomFactor;
    private Texture2D zoomIconTexture;
    private Rect zoomSliderRect;
    private Rect zoomTextureRect;
    
    private Pico9SpriteEditorState toolState;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnGUI()
    {
        HandleUserInput();

        DrawSpriteEditView();

        DrawToolBar();

        DrawPallet();

        DrawZoomSlider();

        DrawSpriteLargeSheet();
    }

    [MenuItem("Tools/Pico-8 Palette Tool")]
    private static void ShowWindow()
    {
        var window = GetWindow<Pico8SpriteEditor>();
        window.titleContent = new GUIContent("Pico-8 Sprite Editor");

        var windowSize = new Vector2(512 + 60, 256 + 512 + 30 + 30 + 30); // Example size, adjust as needed
        window.minSize = windowSize;
        window.maxSize = windowSize;

        window.Show();
    }

    private void Initialize()
    {
        spriteSheetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/test.png");
        if (spriteSheetTexture == null)
        {
            spriteSheetTexture = new Texture2D(128, 128);
            var bytes = spriteSheetTexture.EncodeToPNG();
            var path = "Assets/test.png";
            if (path.Length != 0)
            {
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
            }

            spriteSheetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/test.png");
        }

        palletTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/pallet.png");
        zoomIconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/texturesize.png");
        brushTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/brush.png");
        stampTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/stamp.png");
        selectTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/select.png");
        handTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/hand.png");
        paintTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/paint.png");
        circleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/circle.png");

        colorsPerRow = palletTexture.width;

        textureRect = new Rect(30, 15, 256, 256);

        palletRect = new Rect(256 + 30 + 15, 15, 128, 128);
        zoomSliderRect = new Rect(256 + 30 + 15 + 40, 128 + 30 + 30, 140, 20);
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

    private void DrawRectangle(Rect rectangle, float thickness, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;
        var topBorderRect = new Rect(rectangle.x, rectangle.y, rectangle.width, thickness);
        var bottomBorderRect = new Rect(rectangle.x, rectangle.y + rectangle.height - thickness, rectangle.width,
            thickness);
        var leftBorderRect = new Rect(rectangle.x, rectangle.y, thickness, rectangle.height);
        var rightBorderRect = new Rect(rectangle.x + rectangle.width - thickness, rectangle.y, thickness,
            rectangle.height);

        DrawSolidRectangle(topBorderRect);
        DrawSolidRectangle(bottomBorderRect);
        DrawSolidRectangle(leftBorderRect);
        DrawSolidRectangle(rightBorderRect);

        Handles.EndGUI();
    }


    private void DrawSolidRectangle(Rect rect)
    {
        Vector3[] rectangleCorners =
        {
            new(rect.x, rect.y, 0),
            new(rect.x + rect.width, rect.y, 0),
            new(rect.x + rect.width, rect.y + rect.height, 0),
            new(rect.x, rect.y + rect.height, 0)
        };

        Handles.DrawSolidRectangleWithOutline(rectangleCorners, Handles.color, Color.clear);
    }

    private void DrawSpriteLargeSheet()
    {
        DrawTexture(spriteSheetTexture, spriteSheetRect);


        DrawSpriteSelection();
    }

    private void DrawZoomSlider()
    {
        textureZoom = EditorGUI.IntSlider(zoomSliderRect, "", textureZoom, 0, 3);

        zoomFactor = 8 * (int)Mathf.Pow(2, textureZoom);

        DrawTexture(zoomIconTexture, zoomTextureRect);
    }

    private void DrawPallet()
    {
        DrawTexture(palletTexture, palletRect);
        DrawPalletSelection();
    }

    private void DrawToolBar()
    {
        DrawTexture(brushTexture, brushIconRect);
        DrawTexture(stampTexture, stampIconRect, true);
        DrawTexture(selectTexture, selectIconRect, true);
        DrawTexture(handTexture, handIconRect, true);
        DrawTexture(paintTexture, paintIconRect, true);
        DrawTexture(circleTexture, circleIconRect, true);

        // draw tiny preview 
        DrawTexture(spriteSheetTexture, smallTextureRect, false, spriteRect);
        // Draw sprite number next to tiny preview
        var labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 24;
        labelStyle.normal.textColor = Color.gray;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleLeft;
        GUI.Label(spriteNumberRect, spriteSelection.ToString("000"), labelStyle);
    }

    private void DrawSpriteEditView()
    {
        // draw sprite in main window
        DrawTexture(spriteSheetTexture, textureRect, false, spriteRect);

        // draw outline for sprite
        DrawRectangle(textureRect, 1, Color.black);
    }

    private void HandleUserInput()
    {
        // update spriteRect for which sprites are selected
        spriteRect = GridConversion.ToRect(spriteSelection, spritesPerRow, zoomFactor, zoomFactor);
        spriteRect.x *= 8;
        spriteRect.y *= 8;
    
        var e = Event.current;

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            var mousePos = e.mousePosition;
            
            HandleToolStateChange(mousePos);

            if (textureRect.Contains(mousePos))
                HandleToolState(mousePos);
            else if (palletRect.Contains(mousePos))
                HandlePalletSelection(mousePos);
            else if (spriteSheetRect.Contains(mousePos))
                HandleDrawSpriteSheet(mousePos);
        }
    
        if (e.type == EventType.MouseLeaveWindow || e.type == EventType.MouseUp)
            SaveSpriteSheet();
    }

    private void HandleToolState(Vector2 mousePos)
    {
        switch (toolState)
        {
            case Pico9SpriteEditorState.Brush:
                HandleBrushSprite(mousePos);
                break;
            case Pico9SpriteEditorState.Stamp:
                break;
            case Pico9SpriteEditorState.Select:
                break;
            case Pico9SpriteEditorState.Hand:
                break;
            case Pico9SpriteEditorState.Paint:
                HandlePaintSprite(mousePos);
                break;
            case Pico9SpriteEditorState.Circle:
                break;
        }
    }


    private void HandleToolStateChange(Vector2 mousePos)
    {
        if(stampIconRect.Contains(mousePos))    
            toolState = Pico9SpriteEditorState.Stamp;
        else if(selectIconRect.Contains(mousePos))
            toolState = Pico9SpriteEditorState.Select;
        else if(handIconRect.Contains(mousePos))
            toolState = Pico9SpriteEditorState.Hand;
        else if(paintIconRect.Contains(mousePos))
            toolState = Pico9SpriteEditorState.Paint;
        else if(circleIconRect.Contains(mousePos))
            toolState = Pico9SpriteEditorState.Circle;
        else
            toolState = Pico9SpriteEditorState.Brush;
    }
    
    private void HandlePaintSprite(Vector2 mousePos)
    {
        throw new System.NotImplementedException();
    }
    
    private Vector2Int MousePosToSpritePos(Vector2 mousePos){
        var scaleFactor = textureRect.width / zoomFactor;
        var adjustedX = mousePos.x - textureRect.x;
        var adjustedY = mousePos.y - textureRect.y;
        var originalX = (int)(adjustedX / scaleFactor);
        var originalY = (int)(adjustedY / scaleFactor);
        var spriteRect = GridConversion.ToRect(spriteSelection, spritesPerRow, zoomFactor, zoomFactor);
        spriteRect.x *= 8;
        spriteRect.y *= 8;
        originalX += (int)spriteRect.x;
        originalY += (int)spriteRect.y;
        return new Vector2Int(originalX, originalY);
    }

    private void SetPixelOnSprite(Texture2D spriteSheetTexture, int originalX, int originalY, Color palletColor){
        shouldSave = true;
        SetPixel(spriteSheetTexture, originalX, spriteSheetTexture.height - originalY - 1, palletColor);
        Repaint();
    }

    private void HandleBrushSprite(Vector2 mousePos)
    {
        var spriteCoordinate = MousePosToSpritePos(mousePos);
        SetPixelOnSprite(spriteSheetTexture, spriteCoordinate.x, spriteCoordinate.y, palletColor);
    }
    
    private void HandlePalletSelection(Vector2 mousePos)
    {
        mousePos.x -= palletRect.x;
        mousePos.y -= palletRect.y;
        mousePos /= 32f;
        palletSelection = (int)mousePos.x + (int)mousePos.y * palletTexture.width;
        palletColor = palletTexture.GetPixel((int)mousePos.x, palletTexture.height - (int)mousePos.y - 1);

        if (palletColor == Color.black)
            palletColor = new Color(0, 0, 0, 0);

        Repaint();
    }
    
    private void HandleDrawSpriteSheet(Vector2 mousePos)
    {
        mousePos.x -= spriteSheetRect.x;
        mousePos.y -= spriteSheetRect.y;
        mousePos /= 32f;
        spriteSelection = (int)mousePos.x + (int)mousePos.y * 16;
        Repaint();
    }
    
    private void SaveSpriteSheet()
    {
        if(!shouldSave)
            return;

        var bytes = spriteSheetTexture.EncodeToPNG();
        var path = "Assets/test.png";

        if (path.Length != 0)
        {
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
    }
    
    private void SetPixel(Texture2D texture, int x, int y, Color c)
    {
        texture.SetPixel(x, y, c);
        texture.Apply();
    }

    private void DrawSpriteSelection()
    {
        var spriteSelectionWidth = spriteSheetRect.width / spritesPerRow;
        var spriteSelectionHeight = spriteSheetRect.height / spritesPerRow;

        var gridPosition = GridConversion.ToVector(spriteSelection, spritesPerRow);

        var sheetPosition = new Rect(
            spriteSheetRect.x + gridPosition.x * spriteSelectionWidth,
            spriteSheetRect.y + gridPosition.y * spriteSelectionHeight,
            spriteSelectionWidth * (int)Mathf.Pow(2, textureZoom),
            spriteSelectionHeight * (int)Mathf.Pow(2, textureZoom));

        DrawRectangle(sheetPosition, selectionRectThickness, Color.white);
    }

    private void DrawPalletSelection()
    {
        var palletSelectionWidth = palletRect.width / colorsPerRow;
        var palletSelectionHeight = palletRect.height / palletTexture.height;
        var gridPosition = GridConversion.ToVector(palletSelection, colorsPerRow);

        var palletPosition = new Rect(
            palletRect.x + gridPosition.x * palletSelectionWidth,
            palletRect.y + gridPosition.y * palletSelectionHeight,
            palletSelectionWidth, palletSelectionHeight);

        DrawRectangle(palletPosition, selectionRectThickness, Color.white);
    }

    private void DrawTexture(Texture2D texture, Rect position, bool dim = false, Rect sourceRect = new())
    {
        if (sourceRect.width == 0)
        {
            var textureStyle = new GUIStyle { normal = { background = texture } };
            GUI.Label(position, GUIContent.none, textureStyle);
        }
        else
        {
            var yInverted = texture.height - sourceRect.y - sourceRect.height;

            var xMin = sourceRect.x / texture.width;
            var xMax = (sourceRect.x + sourceRect.width) / texture.width;

            var yMin = yInverted / texture.height;
            var yMax = (yInverted + sourceRect.height) / texture.height;

            var texCoords = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            GUI.DrawTextureWithTexCoords(position, texture, texCoords, true);
        }

        if (dim)
        {
            var dimColor = new Color(0, 0, 0, 0.5f);
            var previousColor = GUI.color;
            GUI.color = dimColor;
            GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
            GUI.color = previousColor;
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
        var grid = Vector2.zero;
        grid.x = index % gridWidth;
        grid.y = index / gridWidth;
        return grid;
    }

    public static Rect ToRect(int index, int gridWidth, int width = 0, int height = 0)
    {
        var v = ToVector(index, gridWidth);
        var r = new Rect(v.x, v.y, width, height);
        return r;
    }
}
