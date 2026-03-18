TEXTURE REFERENCE FILES
======================

Place the following reference images in this directory:

1. khortitskiy_street_ref.jpg   — Street reference for environment layout
2. panelka_wall_ref_1.jpg       — Wall texture for panelka buildings (variant 1)
3. panelka_wall_ref_2.jpg       — Wall texture for panelka buildings (variant 2)
4. vitaliy_ref.jpg              — Character reference: Vitaliy (at laptop)
5. kirill_ref.jpg               — Character reference: Kirill (near camera equipment)
6. uliana_ref.jpg               — Character reference: Uliana (with suitcase)
7. zavkhoz_ref.jpg              — Character reference: Zavkhoz (elderly woman, blue robe)

TEXTURE IMPORT SETTINGS
=======================
- Maximum resolution: 1024x1024
- Compression: Normal quality
- Filter mode: Bilinear
- Wrap mode: Repeat (for wall textures), Clamp (for character textures)
- sRGB: Enabled

After placing textures, assign them to materials:
- panelka_wall_ref_1.jpg → Assets/Materials/PanelkaWall1.mat (_BaseMap)
- panelka_wall_ref_2.jpg → Assets/Materials/PanelkaWall2.mat (_BaseMap)
