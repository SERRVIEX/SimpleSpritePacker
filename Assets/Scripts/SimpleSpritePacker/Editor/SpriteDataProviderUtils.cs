namespace SimpleSpritePacker
{
    using System.Linq;

    using UnityEngine;

    using UnityEditor;
    using UnityEditor.U2D.Sprites;

    public static class SpriteDataProviderUtils
    {
        public static bool Contains(this ISpriteEditorDataProvider dataProvider, string name)
        {
            var spriteRects = dataProvider.GetSpriteRects().ToList();
            for (int i = 0; i < spriteRects.Count; i++)
                if (spriteRects[i].name == name)
                    return true;

            return false;
        }

        /// <summary>
        /// https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
        /// </summary>
        public static void Add(this ISpriteEditorDataProvider dataProvider, string name, Rect rect, Vector2 pivot)
        {
            // Define the new Sprite Rect.
            var newSprite = new SpriteRect()
            {
                name = name,
                spriteID = GUID.Generate(),
                pivot = pivot,
                rect = rect
            };

            // Add the Sprite Rect to the list of existing Sprite Rects.
            var spriteRects = dataProvider.GetSpriteRects().ToList();
            spriteRects.Add(newSprite);

            // Write the updated data back to the data provider.
            dataProvider.SetSpriteRects(spriteRects.ToArray());

            var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();
            nameFileIdPairs.Add(new SpriteNameFileIdPair(newSprite.name, newSprite.spriteID));
            spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);

            // Apply the changes.
            dataProvider.Apply();
        }

        /// <summary>
        /// https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
        /// </summary>
        public static void Remove(this ISpriteEditorDataProvider dataProvider, string name)
        {
            var spriteRects = dataProvider.GetSpriteRects().ToList();
            for (int i = 0; i < spriteRects.Count; i++)
            {
                if (spriteRects[i].name == name)
                {
                    spriteRects.RemoveAt(i);
                    break;
                }
            }

            // Write the updated data back to the data provider.
            dataProvider.SetSpriteRects(spriteRects.ToArray());

            var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();
            for (int i = 0; i < nameFileIdPairs.Count; i++)
            {
                if (nameFileIdPairs[i].name == name)
                {
                    nameFileIdPairs.RemoveAt(i);
                    break;
                }
            }

            spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);

            // Apply the changes.
            dataProvider.Apply();
        }

        /// <summary>
        /// https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
        /// </summary>
        public static void Update(this ISpriteEditorDataProvider dataProvider, string name, Rect rect)
        {
            // Get all the existing Sprites.
            var spriteRects = dataProvider.GetSpriteRects();

            // Loop over all Sprites and update the rects.
            foreach (var spriteRect in spriteRects)
                if (spriteRect.name == name)
                    spriteRect.rect = rect;

            // Write the updated data back to the data provider.
            dataProvider.SetSpriteRects(spriteRects);

            // Apply the changes.
            dataProvider.Apply();
        }

        public static SpriteRect GetSpriteRect(this ISpriteEditorDataProvider dataProvider, GUID id)
        {
            // Get all the existing Sprites.
            var spriteRects = dataProvider.GetSpriteRects();

            // Loop over all Sprites and get the sprite rect.
            foreach (var spriteRect in spriteRects)
                if (spriteRect.spriteID == id)
                    return spriteRect;

            return null;
        }

        public static void SetSpriteName(this ISpriteEditorDataProvider dataProvider, SpriteRect target, string name)
        {
            // Get all the existing Sprites.
            var spriteRects = dataProvider.GetSpriteRects();

            // Loop over all Sprites and update the name.
            foreach (var spriteRect in spriteRects)
            {
                if (spriteRect.name == target.name)
                {
                    spriteRect.name = name;
                    break;
                }
            }

            // Write the updated data back to the data provider.
            dataProvider.SetSpriteRects(spriteRects);

            // Apply the changes.
            dataProvider.Apply();
        }
    }
}