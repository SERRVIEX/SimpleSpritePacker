# SimpleSpritePacker (v1.0.3)

This is very simple tool for creating or editing sprite packers from the Unity's Editor. It also generates sprite rectangles for input textures like in Unity's Sprite Editor (splitting).
When editing an existing multiple mode sprite, references to the existing subsprites will be preserved.
Texture packing at runtime is also supported, but creating sprite rectangles is available only in editor.

Two algorithms are available for packing: FFDH and Binary which is better.

![alt text](https://github.com/SERRVIEX/SimpleSpritePacker/blob/main/github_assets/asset_0.png)

## How to use?
To open the Sprite Packer window, navigate to Unity's main menu and go to:
```Tools/Sprite Packer```

### Properties.
Change the width and height of the output atlas and the spacing between the sprites.

```
Important! Use 2px spacing to avoid rendering issues.
```

### Conditions.
Set the minimum and maximum size of the sprites that can be added to the atlas.

### Inputs.

1. Source Atlas (optional) - you can edit an existing atlas without losing references.
```
After linking to the original atlas, you will have an array of all existing sprites, 
where you will be able to replace the texture of the sprite and delete it.
```

2. Textures - an array of textures to be packed.

### Export.
1. Name - texture output name.
2. Sprites Prefix - add a prefix to each sprite name. (ui_{name}).

```
Important! The sprite name will be the same as the texture name (with prefix).
```

### Actions.
1. Pack - pack in an atlas.
2. Export - save output texture.

```
Important! If there is a reference to the original atlas, then the export will replace it, otherwise a new texture will be created.
```

## License
[MIT](https://choosealicense.com/licenses/mit/)
