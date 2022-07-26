# SimpleSpritePacker (v1.0.0)

Sprite packer for Unity. 

Simply create or edit sprite packers from the Unity's Editor.
It also generates sprite rectangles for input textures.
When editing an existing sprite atlas, references to the existing sprite rectangles will be preserved.

## How to use?
To open the Sprite Packer window, navigate to Unity's main menu and go to:
```Tools/Sprite Packer```


### Properties.
Here you can change the width and height of the output atlas and the padding between the sprites.

```
Important! Use 1px padding to avoid rendering issues.
```

### Conditions.
Here you can set the minimum and maximum size of the sprites that can be added to the atlas.

### Inputs.
```
Source Atlas (optional) - you can edit an existing atlas without losing references.
```

After linking to the original atlas, you will have an array of all existing sprites, 
where you will be able to replace the texture of the sprite and delete it.

```
Textures - an array of textures to be packed.
```

### Export.
```
Name - texture output name.
```

```
Sprites Prefix - add a prefix to each sprite name. (ui_{name}).
```

```
Important! The sprite name will be the same as the texture name (with prefix).
```

### Actions.
```
Process - pack in an atlas.
Export - save output texture.
```

```
Important! If there is a reference to the original atlas, then the export will replace it, otherwise a new texture will be created.
```

## License
[MIT](https://choosealicense.com/licenses/mit/)