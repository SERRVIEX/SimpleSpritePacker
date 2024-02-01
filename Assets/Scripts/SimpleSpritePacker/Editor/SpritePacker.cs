namespace SimpleSpritePacker
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;

    using UnityEngine;

    using UnityEditor;
    using UnityEditor.U2D.Sprites;

    using Algorithms;
    using Algorithms.BinaryTree;
    using Algorithms.FirstFitDecreasingHeight;

    public class SpritePacker : EditorWindow
    {
        /// <summary>
        /// Serialized object of this class.
        /// </summary>
        private SerializedObject _serializedObject;

        [SerializeField] private float _filledArea;

        /// <summary>
        /// Atlas width.
        /// </summary>
        [SerializeField] private int _width = 2048;

        /// <summary>
        /// Atlas height.
        /// </summary>
        [SerializeField] private int _height = 2048;

        [SerializeField] private bool _customSize;

        /// <summary>
        /// Spacing between sprites (recommended 2).
        /// </summary>
        [SerializeField] private int _spacing = 2;

        /// <summary>
        /// Min sprite size required to pack it.
        /// </summary>
        [SerializeField] private Vector2Int _minSpriteSize;

        /// <summary>
        /// Max sprite size required to pack it.
        /// </summary>
        [SerializeField] private Vector2Int _maxSpriteSize = new Vector2Int(1024, 1024);

        /// <summary>
        /// If reference is not null, then packing will
        /// override the referenced atlas instead of creating a new pack.
        /// This is optional value.
        /// </summary>
        [SerializeField] private Texture2D _sourceAtlas;

        /// <summary>
        /// Textures which need to pack.
        /// </summary>
        [SerializeField] private List<Texture2D> _textures = new List<Texture2D>();
        [SerializeField] private List<ReplacedTexture> _replacedTextures = new List<ReplacedTexture>();

        /// <summary>
        /// Which algorithm will be use for packing.
        /// </summary>
        [SerializeField] private AlgorithmType _algorithmType = AlgorithmType.Binary;

        /// <summary>
        /// List with textures thare were packed.
        /// </summary>
        private List<TextureNode> _packedTextures = new List<TextureNode>();

        /// <summary>
        /// List with textures that were not packed due to size or lack of space
        /// </summary>
        private List<TextureNode> _unpackedTextures = new List<TextureNode>();
        private string _unpackedLog;

        [SerializeField] private string _outputName = "sprite_pack";
        [SerializeField] private string _outputSpritesPrefixName = "";
        private Texture2D _outputTexture;

        private class AtlasTextureCache
        {
            public string Name;
            public Sprite Sprite;
            public Texture2D Texture;
            public SpriteRect SpriteRect;
        }

        private List<AtlasTextureCache> _atlasCache = new List<AtlasTextureCache>();

        private bool _areSpritesFoldout;

        // Cached values.
        private Shader _textureShader;
        private Material _textureMaterial;

        private Vector2 _scrollPos;

        // Methods

        [MenuItem("Tools/Sprite Packer")]
        private static void Initialize()
        {
            SpritePacker window = (SpritePacker)GetWindow(typeof(SpritePacker));
            window.Show();
            window._outputTexture = new Texture2D(window._width, window._height);
            window.minSize = new Vector2(256, window.minSize.y);
            window.maxSize = new Vector2(512, window.maxSize.y);
        }

        private void OnEnable()
        {
            _textureShader = Shader.Find("Sprites/Default");
            _textureMaterial = new Material(_textureShader);
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            DrawAtlasPreview();
            DrawMainProperties();
            DrawSource();
            DrawTextures();

            if (_sourceAtlas == null)
            {
                AdvancedGUI.Headline("Export");
                _outputName = EditorGUILayout.TextField("Name", _outputName);
                _outputSpritesPrefixName = EditorGUILayout.TextField("Sprites Prefix", _outputSpritesPrefixName);
            }

            AdvancedGUI.Headline("Actions");

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_algorithmType"), true);

            GUI.color = Color.green;

            if (GUILayout.Button("Pack"))
                Pack();

            GUI.color = Color.white;

            if (_outputTexture != null)
                if (GUILayout.Button("Export"))
                    Export();

            // Print in the inspector which textures have not been packed.
            if (_unpackedTextures.Count > 0)
            {
                AdvancedGUI.Headline($"Not packed ({_unpackedTextures.Count} from {_textures.Count})");

                if (string.IsNullOrEmpty(_unpackedLog))
                {
                    StringBuilder builder = new StringBuilder();
                    int count = _unpackedTextures.Count;
                    int lastIndex = count - 1;
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append($"{_unpackedTextures[i].Texture.name} - ({_unpackedTextures[i].Texture.width}x{_unpackedTextures[i].Texture.height})");
                        if (i != lastIndex)
                            builder.Append("\n");
                    }

                    _unpackedLog = builder.ToString();
                }

                EditorGUILayout.LabelField(_unpackedLog, new GUIStyle("HelpBox"));
            }
            else
                _unpackedLog = string.Empty;

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            _serializedObject.ApplyModifiedProperties();
            GUI.changed = false;
        }

        private void DrawAtlasPreview()
        {
            if (_outputTexture != null)
            {
                (float width, float height) textureSize = GetTextureSize(_outputTexture.width, _outputTexture.height, EditorGUIUtility.currentViewWidth - 50, 512);

                if (_outputTexture != null)
                {
                    EditorGUI.DrawRect(new Rect(position.width / 2 - textureSize.width / 2f, 5, textureSize.width, textureSize.height), new Color(0, 0, 0, 0.25f));
                    EditorGUI.DrawPreviewTexture(new Rect(position.width / 2 - textureSize.width / 2f, 5, textureSize.width, textureSize.height), _outputTexture, _textureMaterial);
                }

                GUILayout.Space(textureSize.height + 20 * .75f);
            }

            GUI.color = Color.yellow;
            EditorGUILayout.LabelField($"Area Filled {_filledArea}%", new GUIStyle("helpBox"));
            GUI.color = Color.white;
        }

        private void DrawMainProperties()
        {
            AdvancedGUI.Headline("Properties");

            // Start a code block to check for GUI changes.
            EditorGUI.BeginChangeCheck();

            if (_sourceAtlas == null)
            {
                _width = EditorGUILayout.IntField("Width", Mathf.Clamp(_width, 128, 4096));
                _height = EditorGUILayout.IntField("Height", Mathf.Clamp(_height, 128, 4096));
            }
            else
            {
                _customSize = EditorGUILayout.Toggle("Custom Size", _customSize);
                if (_customSize)
                {
                    _width = EditorGUILayout.IntField("Width", Mathf.Clamp(_width, 128, 4096));
                    _height = EditorGUILayout.IntField("Height", Mathf.Clamp(_height, 128, 4096));
                }
                else
                {
                    _width = _sourceAtlas.width;
                    _height = _sourceAtlas.height;
                }
            }

            GUI.color = Color.white;

            _spacing = EditorGUILayout.IntField("Spacing", _spacing);

            AdvancedGUI.Headline("Conditions");

            _minSpriteSize = EditorGUILayout.Vector2IntField("Min Sprite Size", _minSpriteSize);
            _maxSpriteSize = EditorGUILayout.Vector2IntField("Max Sprite Size", _maxSpriteSize);
        }

        private void DrawSource()
        {
            AdvancedGUI.Headline("Source");

            SerializedProperty sourceAtlasProperty = _serializedObject.FindProperty("_sourceAtlas");

            // Start a code block to check for GUI changes.
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(sourceAtlasProperty, true);

            if (_sourceAtlas != null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_sourceAtlas)) as TextureImporter;
                if (textureImporter.spriteImportMode != SpriteImportMode.Multiple)
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField($"Sprite import mode must be multiple.", new GUIStyle("helpBox"));
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField($"Width: {_sourceAtlas.width} Height: {_sourceAtlas.height}", new GUIStyle("helpBox"));
                    GUI.color = Color.white;

                    ISpriteEditorDataProvider spriteDataProvider = GetSpriteEditorDataProvider();
                    SpriteRect[] spriteRects = spriteDataProvider.GetSpriteRects();

                    // If data provider does not contains sprite that is
                    // defined in the replace array then remove it.
                    for (int i = _replacedTextures.Count - 1; i >= 0; i--)
                    {
                        bool found = false;
                        for (int j = 0; j < spriteRects.Length; j++)
                        {
                            if (_replacedTextures[i].Id == spriteRects[j].spriteID)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            _replacedTextures.RemoveAt(i);
                    }

                    // If replace array does not contains sprite that is 
                    // defined in the data provider then add it.
                    for (int i = 0; i < spriteRects.Length; i++)
                    {
                        bool found = false;
                        for (int j = 0; j < _replacedTextures.Count; j++)
                        {
                            if (spriteRects[i].spriteID == _replacedTextures[j].Id)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            _replacedTextures.Add(new ReplacedTexture(spriteRects[i].spriteID, spriteRects[i].name));
                    }

                    // Optimize drawin sprites in the inspector.
                    if (_atlasCache.Count != _replacedTextures.Count)
                    {
                        _atlasCache.Clear();

                        string spriteSheet = AssetDatabase.GetAssetPath(_sourceAtlas);
                        Sprite[] spritesForPreviews = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();

                        Func<string, Sprite> getSprite = value =>
                        {
                            for (int i = 0; i < spritesForPreviews.Length; i++)
                                if (spritesForPreviews[i].name == value)
                                    return spritesForPreviews[i];
                            return null;
                        };

                        for (int i = 0; i < _replacedTextures.Count; i++)
                        {
                            ReplacedTexture spriteMetadata = _replacedTextures[i];
                            SpriteRect spriteRect = spriteDataProvider.GetSpriteRect(spriteMetadata.Id);

                            Sprite sprite = getSprite(spriteRect.name);

                            AtlasTextureCache pool = new AtlasTextureCache();
                            pool.Sprite = sprite;
                            pool.Texture = AssetPreview.GetAssetPreview(sprite);
                            pool.SpriteRect = spriteRect;

                            _atlasCache.Add(pool);
                        }
                    }

                    _areSpritesFoldout = EditorGUILayout.Foldout(_areSpritesFoldout, "Included Sprites", EditorStyles.foldoutHeader);
                    if (_areSpritesFoldout)
                    {
                        int previewWidth = 45;
                        int previewHeight = 45;
                        for (int i = 0; i < _replacedTextures.Count; i++)
                        {
                            ReplacedTexture spriteMetadata = _replacedTextures[i];
                            SpriteRect spriteRect = _atlasCache[i].SpriteRect;

                            GUILayout.BeginHorizontal();

                            var controlRect = EditorGUILayout.GetControlRect(true, GUILayout.MaxWidth(0));
                            EditorGUI.DrawRect(new Rect(controlRect.x, controlRect.y, previewWidth, previewWidth), new Color(0, 0, 0, 0.25f));

                            if (_atlasCache[i].Texture == null)
                                _atlasCache[i].Texture = AssetPreview.GetAssetPreview(_atlasCache[i].Sprite);

                            if (_atlasCache[i].Texture != null)
                            {
                                (float w, float h) textureSize = GetTextureSize(_atlasCache[i].Texture.width, _atlasCache[i].Texture.height, previewWidth, previewHeight);
                                EditorGUI.DrawPreviewTexture(new Rect(controlRect.x, controlRect.y, textureSize.w, textureSize.h), _atlasCache[i].Texture, _textureMaterial);
                            }

                            // Space after preview.
                            GUILayout.Space(previewWidth);

                            // Draw button rename if the name is changed.
                            if (spriteMetadata.Name != spriteRect.name)
                            {
                                GUILayout.BeginVertical(GUILayout.Width(100));

                                // Draw input for the name.
                                spriteMetadata.Name = GUILayout.TextField($"{spriteMetadata.Name}", GUILayout.Width(100));
                                // Draw button to perform rename action.
                                if (GUILayout.Button("RENAME", GUILayout.Width(100)))
                                {
                                    if (spriteDataProvider.Contains(spriteMetadata.Name))
                                    {
                                        if (EditorUtility.DisplayDialog("Rename Sprite", "The same name already exists!", "OK"))
                                            spriteMetadata.Name = spriteRect.name;
                                    }
                                    else
                                    {
                                        spriteDataProvider.SetSpriteName(spriteRect, spriteMetadata.Name);
                                        spriteRect.name = spriteMetadata.Name;
                                        var assetImporter = spriteDataProvider.targetObject as AssetImporter;
                                        assetImporter.SaveAndReimport();
                                    }
                                }
                                GUILayout.EndVertical();
                            }
                            else
                                spriteMetadata.Name = GUILayout.TextField($"{spriteMetadata.Name}", GUILayout.Width(100));

                            // Replace texture field.
                            spriteMetadata.ReplaceTexture2D = EditorGUILayout.ObjectField(spriteMetadata.ReplaceTexture2D, typeof(Texture2D), false) as Texture2D;

                            // Export and delete.
                            GUILayout.BeginVertical();

                            if (GUILayout.Button("EXPORT", GUILayout.Width(100)))
                            {
                                Texture2D texture = new Texture2D((int)spriteRect.rect.width, (int)spriteRect.rect.height, TextureFormat.ARGB32, false, true);
                                texture.SetPixels(_sourceAtlas.GetPixels((int)spriteRect.rect.x, (int)spriteRect.rect.y, (int)spriteRect.rect.width, (int)spriteRect.rect.height));
                                texture.Apply();
                                texture.name = spriteMetadata.Name;

                                WriteTexture(texture);
                            }

                            GUI.color = Color.red;

                            if (GUILayout.Button("DELETE", GUILayout.Width(100)))
                            {
                                if (EditorUtility.DisplayDialog("Remove Sprite", "Are you sure? That action can't be undone!", "Delete"))
                                {
                                    SpriteDataProviderUtils.Remove(spriteDataProvider, spriteRect.name);
                                    Pack();
                                    Export();

                                    GUI.color = Color.white;
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                    i--;
                                    continue;
                                }
                            }

                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();

                            // Space between rows.
                            GUILayout.Space(5);

                            GUI.color = Color.white;

                            _replacedTextures[i] = spriteMetadata;
                        }
                    }
                }
            }
        }

        private void DrawTextures()
        {
            AdvancedGUI.Headline("Textures");

            GUI.color = Color.green;
            EditorGUILayout.LabelField($"New textures that should be included.", new GUIStyle("helpBox"));
            GUI.color = Color.white;

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_textures"), true);
        }

        private ISpriteEditorDataProvider GetSpriteEditorDataProvider(Texture2D texture = null)
        {
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider;

            if (_sourceAtlas != null)
            {
                dataProvider = factory.GetSpriteEditorDataProviderFromObject(_sourceAtlas);
                dataProvider.InitSpriteEditorDataProvider();
            }
            else
            {
                dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
                dataProvider.InitSpriteEditorDataProvider();
            }

            return dataProvider;
        }

        private (float width, float height) GetTextureSize(float width, float height, float minValue, float maxValue)
        {
            float w = Mathf.Min(minValue, width, maxValue);
            float h = w * (height / width);

            if (h > maxValue)
            {
                float ratio = Mathf.Max(w, h) / Mathf.Min(w, h);
                h = maxValue;
                w = maxValue / ratio;
            }

            return (w, h);
        }

        /// <summary>
        /// Pack into the atlas.
        /// </summary>
        private void Pack()
        {
            // Clear previous data.
            _packedTextures.Clear();
            _unpackedTextures.Clear();

            List<Texture2D> textures = new List<Texture2D>();

            // Get sprites metadata from the atlas.
            if (_sourceAtlas != null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_sourceAtlas)) as TextureImporter;

                // Grab the textures from the spritesheet.
                if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                {
                    Texture2D targetAtlas = _sourceAtlas;
                    // If the target atlas isn't readable, then create a copy.
                    if (!_sourceAtlas.isReadable)
                        targetAtlas = CreateReadableTexture2D(_sourceAtlas);

                    // Create a new texture from each sprite from the atlas.
                    var atlasMetadata = textureImporter.spritesheet;
                    for (int i = 0; i < atlasMetadata.Length; i++)
                    {
                        SpriteMetaData metadata = atlasMetadata[i];

                        Texture2D texture;
                        if (TryGetReplaceableTexture2D(metadata.name, out Texture2D output))
                            texture = output;
                        else
                        {
                            texture = new Texture2D((int)metadata.rect.width, (int)metadata.rect.height, TextureFormat.ARGB32, false, true);
                            texture.SetPixels(targetAtlas.GetPixels((int)metadata.rect.x, (int)metadata.rect.y, (int)metadata.rect.width, (int)metadata.rect.height));
                            texture.Apply();
                        }

                        texture.name = atlasMetadata[i].name;
                        textures.Add(texture);
                    }
                }
            }

            // Skip textures that don't match properties.
            for (int i = _textures.Count - 1; i >= 0; i--)
            {
                if (_textures[i] == null)
                    continue;

                if (_textures[i].width > _maxSpriteSize.x || _textures[i].height > _maxSpriteSize.y ||
                    _textures[i].width < _minSpriteSize.x || _textures[i].height < _minSpriteSize.y)
                    continue;

                _textures[i] = CreateReadableTexture2D(_textures[i]);
                textures.Add(_textures[i]);
            }

            _outputTexture = _algorithmType switch
            {
                AlgorithmType.FFDH => FFDHPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures),
                AlgorithmType.Binary => BinaryPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures),
                _ => FFDHPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures),
            };

            float maxArea = _width * _height;
            float area = 0;
            for (int i = 0; i < _packedTextures.Count; i++)
                area += _packedTextures[i].Width * _packedTextures[i].Height;

            _filledArea = area / maxArea * 100f;

            _atlasCache.Clear();
        }

        /// <summary>
        /// If input texture is not marked as readable, then create a new through render texture.
        /// </summary>
        private Texture2D CreateReadableTexture2D(Texture2D source)
        {
            if(!source.isReadable)
                SetTextureImporterFormat(source, true);
            return source;

            // Obsolete.
            RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableTex = new Texture2D(source.width, source.height);
            readableTex.name = source.name;
            readableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTex.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableTex;
        }

        private void SetTextureImporterFormat(Texture2D texture, bool isReadable)
        {
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = isReadable;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        private bool TryGetReplaceableTexture2D(string name, out Texture2D texture2D)
        {
            texture2D = null;

            for (int i = 0; i < _replacedTextures.Count; i++)
            {
                if (_replacedTextures[i].Name == name)
                {
                    texture2D = _replacedTextures[i].ReplaceTexture2D;
                    if (texture2D != null)
                        texture2D = CreateReadableTexture2D(texture2D);

                    return texture2D != null;
                }
            }

            return false;
        }

        /// <summary>
        /// Export atlas.
        /// </summary>
        private void Export()
        {
            Pack();

            Texture2D exported = WriteAtlas();

            string path = AssetDatabase.GetAssetPath(exported);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.SaveAndReimport();

            ISpriteEditorDataProvider spriteDataProvider = GetSpriteEditorDataProvider(exported);

            string prefix = _outputSpritesPrefixName;
            if (_sourceAtlas != null)
                prefix = string.Empty;
            else
                _sourceAtlas = exported;

            // Help to detect duplicated names.
            List<string> verifiedNames = new List<string>();

            // Using data provider.
            for (int i = 0; i < _packedTextures.Count; i++)
            {
                string name = $"{prefix}{_packedTextures[i].Texture.name}";

                Vector2 pivot = Vector2.one / 2f;
                Rect rect = new Rect(_packedTextures[i].X, _packedTextures[i].Y, _packedTextures[i].Width, _packedTextures[i].Height);

                if (spriteDataProvider.Contains(name) && !verifiedNames.Contains(name))
                    spriteDataProvider.Update(name, rect);
                else
                {
                    // Avoid sprites with the same name.
                    int index = 1;
                    while (spriteDataProvider.Contains(name))
                    {
                        name = $"{prefix}{_packedTextures[i].Texture.name}_{index}";
                        index++;
                    }

                    spriteDataProvider.Add(name, rect, pivot);
                }

                verifiedNames.Add(name);
            }

            // Apply the changes made to the data provider
            spriteDataProvider.Apply();

            // Reimport the asset to have the changes applied
            var assetImporter = spriteDataProvider.targetObject as AssetImporter;
            assetImporter.SaveAndReimport();

            _replacedTextures.Clear();

            _packedTextures.Clear();
            _unpackedTextures.Clear();
            _textures.Clear();
            _atlasCache.Clear();

            GC.Collect();

            Debug.Log($"Exported at: {path}");
        }

        /// <summary>
        /// Write atlas to the Resources.
        /// </summary>
        private Texture2D WriteAtlas()
        {
            // If source atlas is null, then generate a new atlas.
            if (_sourceAtlas == null)
            {
                string path = $"{Application.dataPath}/Resources/{_outputName}";

                // Avoid overwrite.
                string finalPath = path;
                int i = 0;
                while (File.Exists($"{finalPath}.png"))
                {
                    finalPath = $"{path} ({i})";
                    i++;
                }

                finalPath = $"{finalPath}.png";

                File.WriteAllBytes(finalPath, _outputTexture.EncodeToPNG());
                AssetDatabase.Refresh();
                Texture2D texture2D = Resources.Load<Texture2D>(Path.GetFileNameWithoutExtension(finalPath));
                return texture2D;
            }
            // Otherwise rewrite the source atlas.
            else
            {
                File.WriteAllBytes(AssetDatabase.GetAssetPath(_sourceAtlas), _outputTexture.EncodeToPNG());
                AssetDatabase.Refresh();
                return _sourceAtlas;
            }
        }

        /// <summary>
        /// Write a texture from the atlas to the Resources.
        /// </summary>
        /// <param name="texture">Target texture.</param>
        private void WriteTexture(Texture2D texture)
        {
            string path = $"{Application.dataPath}/Resources/{texture.name}";

            // Avoid overwrite.
            string finalPath = path;
            int i = 0;
            while (File.Exists($"{finalPath}.png"))
            {
                finalPath = $"{path} ({i})";
                i++;
            }

            finalPath = $"{finalPath}.png";

            File.WriteAllBytes(finalPath, texture.EncodeToPNG());

            AssetDatabase.Refresh();
        }
    }
}