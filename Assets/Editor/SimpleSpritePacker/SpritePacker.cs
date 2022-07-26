namespace SimpleSpritePacker
{
    using System.IO;
    using System.Collections.Generic;

    using UnityEngine;

    using UnityEditor;
    using UnityEditor.U2D.Sprites;

    public class SpritePacker : EditorWindow
    {
        [SerializeField] private int _width = 1024;
        [SerializeField] private int _height = 1024;
        [SerializeField] private int _padding = 1;

        [SerializeField] private Vector2Int _minSpriteSize;
        [SerializeField] private Vector2Int _maxSpriteSize = new Vector2Int(512, 512);

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

        private List<TextureRect> _packedTextures = new List<TextureRect>();
        private List<TextureRect> _unpackedTextures = new List<TextureRect>();

        [SerializeField] private string _outputName = "sprite_pack";
        [SerializeField] private string _outputSpritesPrefixName = "";
        private Texture2D _outputTextures2D;

        private Vector2 _scrollPos;
        private GUIStyle _headerStyle;

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

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            // Create preview.
            if (_outputTextures2D != null)
            {
                float width = Mathf.Min(EditorGUIUtility.currentViewWidth, _outputTextures2D.width, 512);
                float height = width * (_outputTextures2D.height / (float)_outputTextures2D.width);

                if(height > 512)
                {
                    float ratio = Mathf.Max(width, height) / Mathf.Min(width, height);
                    height = 512;
                    width = 512 / ratio;
                }

                if (_outputTextures2D != null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    EditorGUI.DrawRect(new Rect(position.width / 2 - width / 2f, 5, width, height), new Color(0, 0, 0, 0.25f));
                    EditorGUI.DrawPreviewTexture(new Rect(position.width / 2 - width / 2f, 5, width, height), _outputTextures2D, new Material(shader));
                }

                GUILayout.Space(height);
            }

            TitleBar("Properties");

            _width = EditorGUILayout.IntField("Width", Mathf.Clamp(_width, 1, 4096));
            _height = EditorGUILayout.IntField("Height", Mathf.Clamp(_height, 1, 4096));
            _padding = EditorGUILayout.IntField("Padding", _padding);

            TitleBar("Conditions");

            _minSpriteSize = EditorGUILayout.Vector2IntField("Min Sprite Size", _minSpriteSize);
            _maxSpriteSize = EditorGUILayout.Vector2IntField("Max Sprite Size", _maxSpriteSize);

            TitleBar("Inputs");

            ScriptableObject target = this;
            SerializedObject serializedObject = new SerializedObject(target);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sourceAtlas"), true);
           
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
                        if(_replaceTextures[i].Name == spriteRects[j].name)
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
                        if(spriteRects[i].name == _replaceTextures[j].Name)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        _replaceTextures.Add(new SpriteMetadata(spriteRects[i].name));
                }

                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Name", new GUIStyle("BoldLabel"), GUILayout.MinWidth(80), GUILayout.MaxWidth(140));
                EditorGUILayout.LabelField("Replaced Texture", new GUIStyle("BoldLabel"), GUILayout.MinWidth(0));
                EditorGUILayout.LabelField(" ", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _replaceTextures.Count; i++)
                {
                    SpriteMetadata data = _replaceTextures[i];

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"- {data.Name}", GUILayout.MinWidth(80), GUILayout.MaxWidth(140));

                    data.ReplaceTexture2D = EditorGUILayout.ObjectField(data.ReplaceTexture2D, typeof(Texture2D), false, GUILayout.MinWidth(0)) as Texture2D;

                    GUI.color = Color.red;

                    if (GUILayout.Button("X", GUILayout.MinWidth(20), GUILayout.MaxWidth(20)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Sprite", "Are you sure? That action can't be undone!", "Remove"))
                        {
                            SpriteDataProviderUtils.Remove(spriteDataProvider, data.Name);
                            Process();
                            Export();

                            GUI.color = Color.white;
                            EditorGUILayout.EndHorizontal();
                            i--;
                            continue;
                        }
                    }

                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();

                    _replaceTextures[i] = data;
                }

                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_textures"), true);

            if (_sourceAtlas == null)
            {
                TitleBar("Export");
                _outputName = EditorGUILayout.TextField("Name", _outputName);
                _outputSpritesPrefixName = EditorGUILayout.TextField("Sprites Prefix", _outputSpritesPrefixName);
            }

            TitleBar("Actions");

            GUI.color = Color.green;

            if (GUILayout.Button("Process (FFDH Algorithm)"))
                Process();
            
            if (_outputTextures2D != null)
                if (GUILayout.Button("Export"))
                    Export();

            GUI.color = Color.white;

            if (_unpackedTextures.Count > 0)
            {
                TitleBar($"Not packed ({_unpackedTextures.Count} from {_textures.Count})");

                for (int i = 0; i < _unpackedTextures.Count; i++)
                    GUILayout.Label($"{_unpackedTextures[i].Texture.name} - ({_unpackedTextures[i].Texture.width}x{_unpackedTextures[i].Texture.height})");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            GUI.changed = false;
        }

        private ISpriteEditorDataProvider GetSpriteEditorDataProvider()
        {
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = null;

            if (_sourceAtlas != null)
            {
                dataProvider = factory.GetSpriteEditorDataProviderFromObject(_sourceAtlas);
                dataProvider.InitSpriteEditorDataProvider();
            }

            return dataProvider;
        }

        /// <summary>
        /// Pack into atlas.
        /// </summary>
        private void Process()
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

                        Texture2D texture2D;
                        if (TryGetReplaceableTexture2D(metadata.name, out Texture2D output))
                            texture2D = output;
                        else
                        {
                            texture2D = new Texture2D((int)metadata.rect.width, (int)metadata.rect.height);
                            texture2D.SetPixels(targetAtlas.GetPixels((int)metadata.rect.x, (int)metadata.rect.y, (int)metadata.rect.width, (int)metadata.rect.height));
                            texture2D.Apply();
                        }

                        texture2D.name = atlasMetadata[i].name;
                        textures.Add(texture2D);
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

            _outputTextures2D = FFDH.Pack(_width, _height, _padding, textures, out _packedTextures, out _unpackedTextures);
        }

        /// <summary>
        /// If input texture is not marked as readable, then create a new through render texture.
        /// </summary>
        private Texture2D CreateReadableTexture2D(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
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
            Texture2D exported = Generate();
            string path = AssetDatabase.GetAssetPath(exported);
            
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.SaveAndReimport();

            ISpriteEditorDataProvider spriteDataProvider = GetSpriteEditorDataProvider();

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

            System.GC.Collect();

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
                return Resources.Load<Texture2D>(_outputName);
            }
            else
            {
                _textures.Clear();
                File.WriteAllBytes(AssetDatabase.GetAssetPath(_sourceAtlas), _outputTextures2D.EncodeToPNG());
                AssetDatabase.Refresh();
                return _sourceAtlas;
            }
        }

        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.box);
                _headerStyle.normal.textColor = Color.white;
                _headerStyle.normal.background = CreateTexture(2, 2, new Color32(62, 62, 62, 255));
            }
        }

        private Texture2D CreateTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private void TitleBar(string title)
        {
            EditorGUILayout.Space(5);

            BeginHorizontalLine();

            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            rect.position = new Vector2(0, rect.position.y);
            GUIStyle titleStyle = new GUIStyle(_headerStyle);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fixedWidth = EditorGUIUtility.currentViewWidth;
            titleStyle.stretchWidth = true;
            EditorGUI.LabelField(rect, title, titleStyle);

            EndHorizontalLine();
        }

        private void BeginHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, -1);
            rect.position = new Vector2(0, rect.position.y);
            rect.width = EditorGUIUtility.currentViewWidth;
            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color32(26, 26, 26, 255));
        }

        private void EndHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.position = new Vector2(0, rect.position.y - 2);
            rect.width = EditorGUIUtility.currentViewWidth;
            rect.height = 1;

            EditorGUI.DrawRect(rect, new Color32(48, 48, 48, 255));
        }
    }
}