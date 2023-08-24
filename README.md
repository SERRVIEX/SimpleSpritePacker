# SimpleSpritePacker

![Version](https://img.shields.io/badge/Version-v1.0.4-brightgreen.svg)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/SERRVIEX/SimpleWindows/blob/main/LICENSE) 
[![Contact](https://img.shields.io/badge/LinkedIn-blue.svg?logo=LinkedIn)](https://www.linkedin.com/in/sergiu-ciornii-466395220/)

## Requirements
[![Unity 2021.3.27+](https://img.shields.io/badge/unity-2021.3.27+-black.svg?style=flat&logo=unity&cacheSeconds=2592000)](https://unity3d.com/get-unity/download/archive)
![.NET 4.x Scripting Runtime](https://img.shields.io/badge/.NET-4.x-blueviolet.svg?style=flat&cacheSeconds=2592000)

## Description
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
MIT License

Copyright (c) 2023 SERRVIEX

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
