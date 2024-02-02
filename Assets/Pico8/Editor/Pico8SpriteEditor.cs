using UnityEngine;
using UnityEditor;

public class Pico8SpriteEditor : EditorWindow
{
    private Texture2D textureToPreview;
    private Texture2D palletTexture;
    private Rect textureRect;
    private Rect palletRect;

    private int palletSelection = 0;
    private Color palletColor = Color.black; 

    [MenuItem("Tools/Pico-8 Palette Tool")]
    private static void ShowWindow()
    {
        var window = GetWindow<Pico8SpriteEditor>();
        window.titleContent = new GUIContent("Pico-8 Sprite Editor");
        window.Show();
    }

    void LoadTexture()
    {
        textureToPreview = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/test.png");
        palletTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pico8/pallet.png");
    }

    private void OnEnable()
    {
        LoadTexture();
        // Initialize your texture rectangles here (or you might want to adjust them dynamically in OnGUI)
        textureRect = new Rect(50, 15, 256, 256);
        palletRect = new Rect(256 + 50 + 15, 15, 128, 128);
    }

    private void DrawThickRectangle(Vector2 position, float width, float height, float thickness, Color color)
    {
        Handles.BeginGUI();

        // Set the color for the rectangle borders
        Handles.color = color;

        // Calculate positions and sizes for the border rectangles
        // Top border
        Rect topBorderRect = new Rect(position.x, position.y, width, thickness);
        // Bottom border
        Rect bottomBorderRect = new Rect(position.x, position.y + height - thickness, width, thickness);
        // Left border
        Rect leftBorderRect = new Rect(position.x, position.y, thickness, height);
        // Right border
        Rect rightBorderRect = new Rect(position.x + width - thickness, position.y, thickness, height);

        // Draw the borders as filled rectangles
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
        HandleMouseClicks();

        if (textureToPreview != null)
        {
            DrawTexture(textureToPreview, textureRect);
        }

        if (palletTexture != null)
        {
            DrawTexture(palletTexture, palletRect);
        }


        Vector2 vector2 = new Vector2();
        vector2.x = palletSelection % 4;
        vector2.y = palletSelection / 4;


        Vector2 position = new Vector2(256 + 50 + 15 -2 + vector2.x * 32, 15-2 + vector2.y * 32); // X, Y position
        float width = 36;
        float height = 36;
        float thickness = 4; // Thickness of the rectangle's border


        DrawThickRectangle(position, width, height, thickness, Color.white);
    }


    private void SetPixelAndSave(int x, int y, Color c)
    {
        // Example: Set the pixel at (x: 2, y: 2) to red
        textureToPreview.SetPixel(x, y, c);
        textureToPreview.Apply(); // Apply changes to the texture

        // Save texture back to file
        byte[] bytes = textureToPreview.EncodeToPNG();
        var path = "Assets/test.png";//EditorUtility.SaveFilePanel("Save texture as PNG", "", "modifiedTexture.png", "png");
        if (path.Length != 0)
        {
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log($"Texture saved to: {path}");

            // Refresh the AssetDatabase if the texture is within the Assets folder
            AssetDatabase.Refresh();
        }
    }

    private void HandleMouseClicks()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0) // Check for left mouse button click
        {
            Vector2 mousePos = e.mousePosition;
            if (textureRect.Contains(mousePos))
            {


                float scaleFactor = 256.0f / 16.0f; // Or directly 16 if it's always going to be this scale

                // Adjusted mouse position to account for padding/offset
                float adjustedX = mousePos.x - 50;
                float adjustedY = 256 - (mousePos.y - 15);

                // Convert from scaled image position back to original texture coordinates
                int originalX = (int)(adjustedX / scaleFactor);
                int originalY = (int)(adjustedY / scaleFactor);

                Debug.Log($"Clicked on textureToPreview at {originalX} {originalY}");



                SetPixelAndSave(originalX, originalY, palletColor);
            }
            else if (palletRect.Contains(mousePos))
            {
                

                mousePos.x -= 256 + 50 + 15;
                mousePos.y -= 15;
                mousePos /= 32f;

                palletSelection = (int)mousePos.x + (int)mousePos.y * 4;

                palletColor = palletTexture.GetPixel((int)mousePos.x, 3 - (int)mousePos.y);

                Debug.Log("Clicked on palletTexture at " + palletSelection);

                // Handle click on palletTexture
                Repaint();
            }
        }
    }

    private void DrawTexture(Texture2D texture, Rect position)
    {
        GUIStyle textureStyle = new GUIStyle { normal = { background = texture } };
        GUI.Label(position, GUIContent.none, textureStyle);
    }
}
