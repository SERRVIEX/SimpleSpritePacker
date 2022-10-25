namespace SimpleSpritePacker
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;

    using UnityEditor;
    using UnityEditor.U2D.Sprites;

    using Algorithms;
    using Algorithms.BinaryTree;
    using Algorithms.FirstFitDecreasingHeight;

    public class SpritePacker : EditorWindow
    {
        private SerializedObject _serializedObject;

        [SerializeField] private float _filledArea;

        [SerializeField] private int _width = 1024;
        [SerializeField] private int _height = 1024;
        [SerializeField] private int _spacing = 2;

        [SerializeField] private Vector2Int _minSpriteSize;
        [SerializeField] private Vector2Int _maxSpriteSize = new Vector2Int(1024, 1024);

        /// <summary>
        /// Optional.
        /// If reference is not null, then packing will
        /// override the referenced atlas instead of creating a new pack.
        /// </summary>
        [SerializeField] private Texture2D _sourceAtlas;

        /// <summary>
        /// Textures which need to pack.
        /// </summary>
        [SerializeField, NonReorderable] private List<Texture2D> _textures = new List<Texture2D>();
        [SerializeField, NonReorderable] private List<SpriteMetadata> _replaceTextures = new List<SpriteMetadata>();

        /// <summary>
        /// Which algorithm will be use for packing.
        /// </summary>
        [SerializeField] private AlgorithmType _algorithmType = AlgorithmType.Binary;

        private List<TextureNode> _packedTextures = new List<TextureNode>();
        private List<TextureNode> _unpackedTextures = new List<TextureNode>();

        [SerializeField] private string _outputName = "sprite_pack";
        [SerializeField] private string _outputSpritesPrefixName = "";
        private Texture2D _outputTextures2D;

        private bool _areSpritesFoldout;
        private Sprite[] _spritesForPreviews;

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
            window._outputTextures2D = new Texture2D(window._width, window._height);
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
            SimpleEditor.Initialize();

            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            DrawAtlasPreview();
            DrawMainProperties();
            DrawInputs();

            if (_sourceAtlas == null)
            {
                SimpleEditor.Header("Export");
                _outputName = EditorGUILayout.TextField("Name", _outputName);
                _outputSpritesPrefixName = EditorGUILayout.TextField("Sprites Prefix", _outputSpritesPrefixName);
            }
            else
            {
                if (_spritesForPreviews == null || _spritesForPreviews.Length == 0)
                    CollectSpritesForPreview();
            }

            SimpleEditor.Header("Actions");

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_algorithmType"), true);

            GUI.color = Color.green;

            if (GUILayout.Button("Pack"))
                Pack();

            GUI.color = Color.white;

            if (_outputTextures2D != null)
                if (GUILayout.Button("Export"))
                    Export();

            if (_unpackedTextures.Count > 0)
            {
                SimpleEditor.Header($"Not packed ({_unpackedTextures.Count} from {_textures.Count})");

                for (int i = 0; i < _unpackedTextures.Count; i++)
                    GUILayout.Label($"{_unpackedTextures[i].Texture.name} - ({_unpackedTextures[i].Texture.width}x{_unpackedTextures[i].Texture.height})");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            _serializedObject.ApplyModifiedProperties();
            GUI.changed = false;
        }

        private void DrawAtlasPreview()
        {
            if (_outputTextures2D != null)
            {
                (float width, float height) textureSize = GetTextureSize(_outputTextures2D.width, _outputTextures2D.height, EditorGUIUtility.currentViewWidth - 50, 512);

                if (_outputTextures2D != null)
                {
                    EditorGUI.DrawRect(new Rect(position.width / 2 - textureSize.width / 2f, 5, textureSize.width, textureSize.height), new Color(0, 0, 0, 0.25f));
                    EditorGUI.DrawPreviewTexture(new Rect(position.width / 2 - textureSize.width / 2f, 5, textureSize.width, textureSize.height), _outputTextures2D, _textureMaterial);
                }

                GUILayout.Space(textureSize.height + 20 * .75f);
            }

            EditorGUILayout.LabelField($"Area Filled {_filledArea}%", new GUIStyle("helpBox"));
        }

        private void DrawMainProperties()
        {
            SimpleEditor.Header("Properties");

            _width = EditorGUILayout.IntField("Width", Mathf.Clamp(_width, 1, 4096));
            _height = EditorGUILayout.IntField("Height", Mathf.Clamp(_height, 1, 4096));
            _spacing = EditorGUILayout.IntField("Spacing", _spacing);

            SimpleEditor.Header("Conditions");

            _minSpriteSize = EditorGUILayout.Vector2IntField("Min Sprite Size", _minSpriteSize);
            _maxSpriteSize = EditorGUILayout.Vector2IntField("Max Sprite Size", _maxSpriteSize);
        }

        private void DrawInputs()
        {
            SimpleEditor.Header("Inputs");

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_sourceAtlas"), true);

            EditorGUILayout.LabelField($"W: {_sourceAtlas.width} H: {_sourceAtlas.height}", new GUIStyle("helpBox"));

            if (_sourceAtlas != null)
            {
                EditorGUI.indentLevel += 2;

                ISpriteEditorDataProvider spriteDataProvider = GetSpriteEditorDataProvider();
                SpriteRect[] spriteRects = spriteDataProvider.GetSpriteRects();

                // If data provider does not contains sprite that is
                // defined in the replace array then remove it.
                for (int i = _replaceTextures.Count - 1; i >= 0; i--)
                {
                    bool found = false;
                    for (int j = 0; j < spriteRects.Length; j++)
                    {
                        if (_replaceTextures[i].Id == spriteRects[j].spriteID)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        _replaceTextures.RemoveAt(i);
                }

                // If replace array does not contains sprite that is 
                // defined in the data provider then add it.
                for (int i = 0; i < spriteRects.Length; i++)
                {
                    bool found = false;
                    for (int j = 0; j < _replaceTextures.Count; j++)
                    {
                        if (spriteRects[i].spriteID == _replaceTextures[j].Id)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        _replaceTextures.Add(new SpriteMetadata(spriteRects[i].spriteID, spriteRects[i].name));
                }

                Func<string, Sprite> getSprite = value =>
                {
                    for (int i = 0; i < _spritesForPreviews.Length; i++)
                        if (_spritesForPreviews[i].name == value)
                            return _spritesForPreviews[i];
                    return null;
                };

                _areSpritesFoldout = EditorGUILayout.Foldout(_areSpritesFoldout, "Sprites");
                if (_areSpritesFoldout)
                {
                    for (int i = 0; i < _replaceTextures.Count; i++)
                    {
                        SpriteMetadata spriteMetadata = _replaceTextures[i];
                        SpriteRect spriteRect = spriteDataProvider.GetSpriteRect(spriteMetadata.Id);

                        EditorGUILayout.BeginHorizontal();

                        Sprite sprite = getSprite(spriteRect.name);
                        var controlRect = EditorGUILayout.GetControlRect(true, GUILayout.MaxWidth(0));
                        Texture2D texture = AssetPreview.GetAssetPreview(sprite);
                        EditorGUI.DrawRect(new Rect(controlRect.x, controlRect.y, 20, 20), new Color(0, 0, 0, 0.25f));

                        if (texture != null)
                        {
                            (float w, float h) textureSize = GetTextureSize(texture.width, texture.height, 20, 20);
                            EditorGUI.DrawPreviewTexture(new Rect(controlRect.x, controlRect.y, textureSize.w, textureSize.h), texture, _textureMaterial);
                        }

                        spriteMetadata.Name = EditorGUILayout.TextField($"{spriteMetadata.Name}", GUILayout.MinWidth(80), GUILayout.MaxWidth(140), GUILayout.MinHeight(20));

                        if (spriteMetadata.Name != spriteRect.name)
                        {
                            if (GUILayout.Button("RENAME", GUILayout.MinWidth(72), GUILayout.MaxWidth(72)))
                            {
                                if (spriteDataProvider.Contains(spriteMetadata.Name))
                                {
                                    if (EditorUtility.DisplayDialog("Rename Sprite", "The same name already exists!", "OK"))
                                        spriteMetadata.Name = spriteRect.name;
                                }
                                else
                                {
                                    spriteDataProvider.SetSpriteName(spriteRect, spriteMetadata.Name);
                                    var assetImporter = spriteDataProvider.targetObject as AssetImporter;
                                    assetImporter.SaveAndReimport();
                                }
                            }
                        }
                        else
                            GUILayout.Space(75);

                        spriteMetadata.ReplaceTexture2D = EditorGUILayout.ObjectField(spriteMetadata.ReplaceTexture2D, typeof(Texture2D), false, GUILayout.MinWidth(0)) as Texture2D;

                        GUI.color = Color.red;

                        if (GUILayout.Button("X", GUILayout.MinWidth(20), GUILayout.MaxWidth(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Sprite", "Are you sure? That action can't be undone!", "Remove"))
                            {
                                SpriteDataProviderUtils.Remove(spriteDataProvider, spriteRect.name);
                                Pack();
                                Export();

                                GUI.color = Color.white;
                                EditorGUILayout.EndHorizontal();
                                i--;
                                continue;
                            }
                        }

                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();

                        _replaceTextures[i] = spriteMetadata;
                    }
                }

                EditorGUI.indentLevel -= 2;
            }

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

        private void CollectSpritesForPreview()
        {
            string spriteSheet = AssetDatabase.GetAssetPath(_sourceAtlas);
            _spritesForPreviews = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
        }

        /// <summary>
        /// Pack into atlas.
        /// </summary>
        private void Pack()
        {
            // Clear previous data.
            _packedTextures.Clear();
            _unpackedTextures.Clear();

            List<Texture2D> textures = new List<Texture2D>();

            // Get sprites metadata from atlas.
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
                if (_textures[i].width > _maxSpriteSize.x || _textures[i].height > _maxSpriteSize.y ||
                    _textures[i].width < _minSpriteSize.x || _textures[i].height < _minSpriteSize.y)
                    continue;

                _textures[i] = CreateReadableTexture2D(_textures[i]);
                textures.Add(_textures[i]);
            }

            switch (_algorithmType)
            {
                case AlgorithmType.FFDH:
                    _outputTextures2D = FFDHPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures);
                    break;

                case AlgorithmType.Binary:
                    _outputTextures2D = BinaryPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures);
                    break;

                default:
                    _outputTextures2D = FFDHPacking.Pack(_width, _height, _spacing, textures, out _packedTextures, out _unpackedTextures);
                    break;
            }

            float maxArea = _width * _height;
            float area = 0;
            for (int i = 0; i < _packedTextures.Count; i++)
                area += _packedTextures[i].Width * _packedTextures[i].Height;

            _filledArea = area / maxArea * 100f;

            CollectSpritesForPreview();
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

            for (int i = 0; i < _replaceTextures.Count; i++)
            {
                if (_replaceTextures[i].Name == name)
                {
                    texture2D = _replaceTextures[i].ReplaceTexture2D;
                    if (texture2D != null)
                        texture2D = CreateReadableTexture2D(texture2D);

                    return texture2D != null;
                }
            }

            return false;
        }

        private void Export()
        {
            Pack();

            Texture2D exported = Generate();
            string path = AssetDatabase.GetAssetPath(exported);
            Debug.Log($"{path}");
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.SaveAndReimport();

            ISpriteEditorDataProvider spriteDataProvider = GetSpriteEditorDataProvider(exported);

            string prefix = _outputSpritesPrefixName;
            if (_sourceAtlas != null)
                prefix = string.Empty;

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

            _replaceTextures.Clear();

            _packedTextures.Clear();
            _unpackedTextures.Clear();

            if(_sourceAtlas != null)
                _textures.Clear();

            GC.Collect();

            Debug.Log($"Exported at: {path}");
        }

        private Texture2D Generate()
        {
            if (_sourceAtlas == null)
            {
                string path = $"{Application.streamingAssetsPath.Replace("StreamingAssets", "Resources")}/{_outputName}";

                // Avoid overwrite.
                string finalPath = path;
                int i = 0;
                while (File.Exists($"{finalPath}.png"))
                {
                    finalPath = $"{path} ({i})";
                    i++;
                }

                finalPath = $"{finalPath}.png";

                File.WriteAllBytes(finalPath, _outputTextures2D.EncodeToPNG());
                AssetDatabase.Refresh();
                return Resources.Load<Texture2D>(Path.GetFileNameWithoutExtension(finalPath));
            }
            else
            {
                _textures.Clear();
                File.WriteAllBytes(AssetDatabase.GetAssetPath(_sourceAtlas), _outputTextures2D.EncodeToPNG());
                AssetDatabase.Refresh();
                return _sourceAtlas;
            }
        }
    }
}