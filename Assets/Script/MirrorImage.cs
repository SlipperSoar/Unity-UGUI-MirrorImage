using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Sprites;

[AddComponentMenu("UI/MirrorImage", 20)]
public class MirrorImage : Image
{
    /// <summary>
    /// 原图的类型
    /// </summary>
    public enum ImageType
    {
        // 半个(0, 1,  2, 3)
        /// <summary>上半</summary>
        TopHalf,
        /// <summary>下半</summary>
        BottomHalf,
        
        /// <summary>右半</summary>
        RightHalf,
        /// <summary>左半</summary>
        LeftHalf,
        
        // 1/4(4, 5, 6, 7)
        /// <summary>右上</summary>
        TopRight,
        /// <summary>右下</summary>
        BottomRight,
        /// <summary>左上</summary>
        TopLeft,
        /// <summary>左下</summary>
        BottomLeft,
    }

    /// <summary>
    /// 原图类型
    /// </summary>
    [SerializeField, Tooltip("原图的类型，比如左半就是右侧镜像，下半就是上侧镜像")]
    private ImageType m_imageResourceType = ImageType.LeftHalf;
    
    public ImageType imageResourceType
    {
        get => m_imageResourceType;
        set
        {
            if (m_imageResourceType != value)
            {
                m_imageResourceType = value;
                SetVerticesDirty();
            }
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        Sprite sp = overrideSprite != null ? overrideSprite : sprite;
        if (sp == null)
        {
            base.OnPopulateMesh(toFill);
            return;
        }

        switch (type)
        {
            case Type.Simple:
                GenerateSimpleSprite(toFill, preserveAspect);
                break;
            case Type.Sliced:
                GenerateSlicedSprite(toFill);
                break;
            case Type.Tiled:
                GenerateTiledSprite(toFill);
                break;
            case Type.Filled:
                // TODO
                GenerateFilledSprite(toFill, preserveAspect);
                // base.OnPopulateMesh(toFill);
                break;
        }
    }

    public override void SetNativeSize()
    {
        if (sprite != null)
        {
            float w = sprite.rect.width / pixelsPerUnit;
            float h = sprite.rect.height / pixelsPerUnit;

            if (imageResourceType <= ImageType.BottomHalf)
            {
                h *= 2;
            }
            else if (imageResourceType <= ImageType.LeftHalf)
            {
                w *= 2;
            }
            else
            {
                w *= 2;
                h *= 2;
            }

            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = new Vector2(w, h);
            SetAllDirty();
        }
    }

    /// <summary>
    /// Generate vertices for a simple Image.
    /// </summary>
    void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
    {
        Vector4 v = GetDrawingDimensions(lPreserveAspect);
        var uv = (sprite != null) ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        Vector4 v1 = v;
        //Debug.Log("uv::::" + uv + "  v:" + v+ "  center:"+ rectTransform.rect.center+ "  rect:" + rectTransform.rect);

        switch (imageResourceType)
        {
            case ImageType.TopHalf:
                v.y = (v.w + v.y) / 2;
                break;
            case ImageType.BottomHalf:
                v.w = (v.w + v.y) / 2;
                break;
            case ImageType.RightHalf:
                v.x = (v.z + v.x) / 2;
                break;
            case ImageType.LeftHalf:
                v.z = (v.z + v.x) / 2;
                break;
            case ImageType.TopRight:
                v.x = (v.z + v.x) / 2;
                v.y = (v.w + v.y) / 2;
                break;
            case ImageType.BottomRight:
                v.x = (v.z + v.x) / 2;
                v.w = (v.y + v.w) / 2;
                break;
            case ImageType.TopLeft:
                v.z = (v.z + v.x) / 2;
                v.y = (v.w + v.y) / 2;
                break;
            case ImageType.BottomLeft:
                v.z = (v.z + v.x) / 2;
                v.w = (v.w + v.y) / 2;
                break;
        }

        //v.w = (v.w + v.y) / 2;
        var color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
        
        switch (imageResourceType)
        {
            /// 1,2,5
            /// 0,3,4
            case ImageType.LeftHalf:
                vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.x, uv.w));
                vh.AddTriangle(3, 2, 5);
                vh.AddTriangle(5, 4, 3);
                break;
            /// 5,1,2
            /// 4,0,3
            case ImageType.RightHalf:
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.z, uv.y));
                vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.z, uv.w));
                vh.AddTriangle(4, 5, 1);
                vh.AddTriangle(1, 0, 4);
                break;
            /// 1,2
            /// 0,3
            /// 4,5
            case ImageType.TopHalf:
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.z, uv.w));
                vh.AddTriangle(0, 4, 3);
                vh.AddTriangle(3, 5, 4);
                break;
            /// 4,5
            /// 1,2
            /// 0,3
            case ImageType.BottomHalf:
                vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.z, uv.y));
                vh.AddTriangle(1, 4, 5);
                vh.AddTriangle(5, 2, 1);
                break;
            /// 1,2,5
            /// 0,3,4
            /// 6,7,8
            case ImageType.TopLeft:
                // 4 5
                vh.AddVert(new Vector3(v1.z, v.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v1.z, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddTriangle(3, 2, 5);
                vh.AddTriangle(5, 4, 3);
                // 6 7 8
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.z, v1.y), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.x, uv.w));
                vh.AddTriangle(6, 0, 3);
                vh.AddTriangle(3, 7, 6);
                vh.AddTriangle(7, 3, 4);
                vh.AddTriangle(4, 8, 7);
                break;
            /// 5,1,2
            /// 4,0,3
            /// 6,7,8
            case ImageType.TopRight:
                // 4 5
                vh.AddVert(new Vector3(v1.x, v.y), color32, new Vector2(uv.z, uv.y));
                vh.AddVert(new Vector3(v1.x, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddTriangle(4, 5, 1);
                vh.AddTriangle(1, 0, 4);
                // 6 7 8
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(v.x, v1.y), color32, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.z, uv.w));
                vh.AddTriangle(6, 4, 0);
                vh.AddTriangle(0, 7, 6);
                vh.AddTriangle(7, 0, 3);
                vh.AddTriangle(3, 8, 7);
                break;
            /// 8,7,6
            /// 1,2,5
            /// 0,3,4
            case ImageType.BottomLeft:
                // 4 5
                vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v1.z, v.w), color32, new Vector2(uv.x, uv.w));
                vh.AddTriangle(3, 2, 5);
                vh.AddTriangle(5, 4, 3);
                // 6 7 8
                vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v.z, v1.w), color32, new Vector2(uv.z, uv.y));
                vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.x, uv.y));
                vh.AddTriangle(6, 5, 2);
                vh.AddTriangle(2, 7, 6);
                vh.AddTriangle(7, 2, 1);
                vh.AddTriangle(1, 8, 7);
                break;
            /// 6 7,8
            /// 5 1,2
            /// 4 0,3
            case ImageType.BottomRight:
                // 4 5
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.z, uv.y));
                vh.AddVert(new Vector3(v1.x, v.w), color32, new Vector2(uv.z, uv.w));
                vh.AddTriangle(4, 5, 1);
                vh.AddTriangle(1, 0, 4);
                // 6 7 8
                vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.z, uv.y));
                vh.AddVert(new Vector3(v.x, v1.w), color32, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.z, uv.y));
                vh.AddTriangle(5, 6, 7);
                vh.AddTriangle(7, 1, 5);
                vh.AddTriangle(1, 7, 8);
                vh.AddTriangle(8, 2 ,1);
                break;
            default:
                break;
        }
    }

    static readonly Vector2[] s_VertScratch = new Vector2[4];
    static readonly Vector2[] s_UVScratch = new Vector2[4];

    private void GenerateSlicedSprite(VertexHelper toFill)
    {
        if (!hasBorder)
        {
            GenerateSimpleSprite(toFill, false);
            return;
        }

        Vector4 outer, inner, padding, border;

        if (sprite != null)
        {
            outer = DataUtility.GetOuterUV(sprite);
            inner = DataUtility.GetInnerUV(sprite);
            padding = DataUtility.GetPadding(sprite);
            border = sprite.border;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            padding = Vector4.zero;
            border = Vector4.zero;
        }
        
        Rect rect = GetPixelAdjustedRect();
        
        // border: (x, y, z, w) => left offset, bottom offset, right offset, top offset
        Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
        
        // padding: (x, y, z, w) => left padding, bottom padding, right padding, top padding
        padding = padding / pixelsPerUnit;
        
        s_VertScratch[0] = new Vector2(padding.x, padding.y);
        s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

        s_VertScratch[1].x = adjustedBorders.x;
        s_VertScratch[1].y = adjustedBorders.y;

        s_VertScratch[2].x = rect.width - adjustedBorders.z;
        s_VertScratch[2].y = rect.height - adjustedBorders.w;

        s_UVScratch[0] = new Vector2(outer.x, outer.y);
        s_UVScratch[1] = new Vector2(inner.x, inner.y);
        s_UVScratch[2] = new Vector2(inner.z, inner.w);
        s_UVScratch[3] = new Vector2(outer.z, outer.w);
        
        switch (imageResourceType)
        {
            case ImageType.LeftHalf:
                s_VertScratch[2].x = rect.width - (s_VertScratch[1].x-s_VertScratch[0].x);
                // s_VertScratch[2].y = rect.height - (s_VertScratch[1].y - s_VertScratch[0].y);
                s_UVScratch[2] = new Vector2(inner.x, inner.w);
                s_UVScratch[3] = new Vector2(outer.x, outer.w);
                break;
            case ImageType.RightHalf:
                s_VertScratch[1].x = rect.width - s_VertScratch[2].x;
                // s_VertScratch[1].y = rect.height - s_VertScratch[2].y;
                s_UVScratch[0] = new Vector2(outer.z, outer.y);
                s_UVScratch[1] = new Vector2(inner.z, inner.y);
                break;
            case ImageType.TopHalf:
                // s_VertScratch[2].x = rect.width - (s_VertScratch[1].x - s_VertScratch[0].x);
                s_VertScratch[1].y = rect.height - s_VertScratch[2].y;
                s_UVScratch[0] = new Vector2(outer.x, outer.w);
                s_UVScratch[1] = new Vector2(inner.x, inner.w);
                break;
            case ImageType.BottomHalf:
                // s_VertScratch[2].x = rect.width - (s_VertScratch[1].x - s_VertScratch[0].x);
                s_VertScratch[2].y = rect.height - (s_VertScratch[1].y - s_VertScratch[0].y);
                s_UVScratch[2] = new Vector2(inner.z, inner.y);
                s_UVScratch[3] = new Vector2(outer.z, outer.y);
                break;
            case ImageType.BottomLeft:
                s_VertScratch[2].x = rect.width - (s_VertScratch[1].x - s_VertScratch[0].x);
                s_VertScratch[2].y = rect.height - (s_VertScratch[1].y - s_VertScratch[0].y);
                s_UVScratch[2] = new Vector2(inner.x, inner.y);
                s_UVScratch[3] = new Vector2(outer.x, outer.y);
                break;
            case ImageType.BottomRight:
                s_VertScratch[1].x = rect.width - s_VertScratch[2].x;
                s_VertScratch[2].y = rect.height - (s_VertScratch[1].y - s_VertScratch[0].y);
                s_UVScratch[0] = new Vector2(outer.z, outer.y);
                s_UVScratch[1] = new Vector2(inner.z, inner.y);
                s_UVScratch[2] = new Vector2(inner.z, inner.y);
                s_UVScratch[3] = new Vector2(outer.z, outer.y);
                break;
            case ImageType.TopLeft:
                s_VertScratch[2].x = rect.width - (s_VertScratch[1].x - s_VertScratch[0].x);
                s_VertScratch[1].y = rect.height - s_VertScratch[2].y;
                s_UVScratch[0] = new Vector2(outer.x, outer.w);
                s_UVScratch[1] = new Vector2(inner.x, inner.w);
                s_UVScratch[2] = new Vector2(inner.x, inner.w);
                s_UVScratch[3] = new Vector2(outer.x, outer.w);
                break;
            case ImageType.TopRight:
                s_VertScratch[1].x = rect.width - s_VertScratch[2].x;
                s_VertScratch[1].y = rect.height - s_VertScratch[2].y;
                s_UVScratch[0] = new Vector2(outer.z, outer.w);
                s_UVScratch[1] = new Vector2(inner.z, inner.w);
                break;
            default:
                break;
        }

        for (int i = 0; i < 4; ++i)
        {
            s_VertScratch[i].x += rect.x;
            s_VertScratch[i].y += rect.y;
        }
        toFill.Clear();

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                if (!fillCenter && x == 1 && y == 1)
                    continue;

                int y2 = y + 1;


                AddQuad(toFill,
                    new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                    new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                    color,
                    new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                    new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
            }
        }
    }

    /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
    private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
    {
        var padding = sprite == null ? Vector4.zero : DataUtility.GetPadding(sprite);
        var size = sprite == null ? Vector2.zero : new Vector2(sprite.rect.width, sprite.rect.height);

        Rect r = GetPixelAdjustedRect();
        

        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);

        var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

        if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
        {
            var spriteRatio = size.x / size.y;
            var rectRatio = r.width / r.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

        v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
                );

        return v;
    }

    private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
    {
        Rect originalRect = rectTransform.rect;

        for (int axis = 0; axis <= 1; axis++)
        {
            float borderScaleRatio;

            // The adjusted rect (adjusted for pixel correctness)
            // may be slightly larger than the original rect.
            // Adjust the border to match the adjustedRect to avoid
            // small gaps between borders (case 833201).
            if (originalRect.size[axis] != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }

            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            // TODO: 可能会有问题？
            switch (imageResourceType)
            {
                case ImageType.LeftHalf:
                case ImageType.RightHalf:
                    if(axis==0)
                    {
                        combinedBorders = border[axis] + border[axis];
                    }
                    break;
                case ImageType.TopHalf:
                case ImageType.BottomHalf:
                    if (axis == 1)
                    {
                        combinedBorders = border[axis] + border[axis];
                    }
                    break;
                case ImageType.BottomLeft:
                case ImageType.BottomRight:
                case ImageType.TopLeft:
                case ImageType.TopRight:
                    combinedBorders = border[axis] + border[axis];
                    break;
                default:
                    break;
            }
            if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }

    static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
    {
        int startIndex = vertexHelper.currentVertCount;

        for (int i = 0; i < 4; ++i)
            vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
    
    static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
    {
        //Debug.Log($"posMin:{posMin},posMax:{posMax},uvMin:{uvMin},uvMax:{uvMax}");
        int startIndex = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
        vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    /// <summary>
    /// Generate vertices for a tiled Image.
    /// </summary>
    void GenerateTiledSprite(VertexHelper toFill)
    {
        Vector4 outer, inner, border;
        Vector2 spriteSize;

        if (sprite != null)
        {
            outer = DataUtility.GetOuterUV(sprite);
            inner = DataUtility.GetInnerUV(sprite);
            border = sprite.border;
            spriteSize = sprite.rect.size;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            border = Vector4.zero;
            spriteSize = Vector2.one * 100;
        }

        Rect rect = GetPixelAdjustedRect();
        float tileWidth = (spriteSize.x - border.x - border.z) / pixelsPerUnit;
        float tileHeight = (spriteSize.y - border.y - border.w) / pixelsPerUnit;
        border = GetAdjustedBorders(border / pixelsPerUnit, rect);

        var uvMin = new Vector2(inner.x, inner.y);
        var uvMax = new Vector2(inner.z, inner.w);

        // Min to max max range for tiled region in coordinates relative to lower left corner.
        float xMin = border.x;
        float xMax = rect.width - border.z;
        float yMin = border.y;
        float yMax = rect.height - border.w;

        toFill.Clear();
        var clipped = uvMax;

        // if either width is zero we cant tile so just assume it was the full width.
        if (tileWidth <= 0)
            tileWidth = xMax - xMin;

        if (tileHeight <= 0)
            tileHeight = yMax - yMin;

        if (sprite != null && (hasBorder || sprite.packed || sprite.texture.wrapMode != TextureWrapMode.Repeat))
        {
            // Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
            // We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

            // Evaluate how many vertices we will generate. Limit this number to something sane,
            // especially since meshes can not have more than 65000 vertices.

            long nTilesW = 0;
            long nTilesH = 0;
            if (fillCenter)
            {
                nTilesW = (long)Mathf.Ceil((xMax - xMin) / tileWidth);
                nTilesH = (long)Mathf.Ceil((yMax - yMin) / tileHeight);

                double nVertices = 0;
                if (hasBorder)
                {
                    nVertices = (nTilesW + 2.0) * (nTilesH + 2.0) * 4.0; // 4 vertices per tile
                }
                else
                {
                    nVertices = nTilesW * nTilesH * 4.0; // 4 vertices per tile
                }

                if (nVertices > 65000.0)
                {
                    //Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                    double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                    double imageRatio;
                    if (hasBorder)
                    {
                        imageRatio = (nTilesW + 2.0) / (nTilesH + 2.0);
                    }
                    else
                    {
                        imageRatio = (double)nTilesW / nTilesH;
                    }

                    float targetTilesW = Mathf.Sqrt((float)(maxTiles / imageRatio));
                    float targetTilesH = (float)(targetTilesW * imageRatio);
                    if (hasBorder)
                    {
                        targetTilesW -= 2;
                        targetTilesH -= 2;
                    }

                    nTilesW = (long)Mathf.Floor(targetTilesW);
                    nTilesH = (long)Mathf.Floor(targetTilesH);
                    tileWidth = (xMax - xMin) / nTilesW;
                    tileHeight = (yMax - yMin) / nTilesH;
                }
            }
            else
            {
                if (hasBorder)
                {
                    // Texture on the border is repeated only in one direction.
                    nTilesW = (long)Mathf.Ceil((xMax - xMin) / tileWidth);
                    nTilesH = (long)Mathf.Ceil((yMax - yMin) / tileHeight);
                    double nVertices = (nTilesH + nTilesW + 2.0 /*corners*/) * 2.0 /*sides*/ * 4.0 /*vertices per tile*/;
                    if (nVertices > 65000.0)
                    {
                        //Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                        double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                        double imageRatio = (double)nTilesW / nTilesH;
                        float targetTilesW = (float)((maxTiles - 4 /*corners*/) / (2 * (1.0 + imageRatio)));
                        float targetTilesH = (float)(targetTilesW * imageRatio);

                        nTilesW = (long)Mathf.Floor(targetTilesW);
                        nTilesH = (long)Mathf.Floor(targetTilesH);
                        tileWidth = (xMax - xMin) / nTilesW;
                        tileHeight = (yMax - yMin) / nTilesH;
                    }
                }
                else
                {
                    nTilesH = nTilesW = 0;
                }
            }

            if (fillCenter)
            {
                // TODO: we could share vertices between quads. If vertex sharing is implemented. update the computation for the number of vertices accordingly.
                for (long j = 0; j < nTilesH; j++)
                {
                    float y1 = yMin + j * tileHeight;
                    float y2 = yMin + (j + 1) * tileHeight;
                    float y2e = y2;
                    if (y2 > yMax)
                    {
                        clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                        y2 = yMax;
                    }
                    clipped.x = uvMax.x;
                    for (long i = 0; i < nTilesW; i++)
                    {
                        float x1 = xMin + i * tileWidth;
                        float x2 = xMin + (i + 1) * tileWidth;
                        float x2e = x2;
                        if (x2 > xMax)
                        {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }

                        var uvMin1 = uvMin;
                        var clipped1 = clipped;
                        //Debug.Log("i::" + i + "  j:::" + j);
                        // 由于原本Image就是基于左下角的平铺，所以这里就不做处理了
                        switch (imageResourceType)
                        {
                            case ImageType.LeftHalf:
                            case ImageType.RightHalf:
                            {
                                if (i % 2 == 1)
                                {
                                    float offsetX = 0;
                                    if (x2e > xMax)
                                    {
                                        offsetX = uvMax.x - (uvMax.x - uvMin.x) * (xMax - x1) / (x2e - x1);
                                    }

                                    uvMin1 = new Vector2(uvMax.x, uvMin.y);
                                    //clipped1 = new Vector2(uvMin.x, clipped.y);
                                    clipped1 = new Vector2(offsetX, clipped.y);
                                }
                            }
                                break;
                            case ImageType.TopHalf:
                            case ImageType.BottomHalf:
                            {
                                if (j % 2 == 1)
                                {
                                    float offsetY = 0;
                                    if (y2e > yMax)
                                    {
                                        offsetY = uvMax.y - (uvMax.y - uvMin.y) * (yMax - y1) / (y2e - y1);
                                    }

                                    //uvMin1 = new Vector2(uvMin.x, clipped.y);
                                    uvMin1 = new Vector2(uvMin.x, uvMax.y);
                                    //clipped1 = new Vector2(clipped.x, uvMin.y);
                                    clipped1 = new Vector2(clipped.x, offsetY);

                                }
                            }
                                break;
                            case ImageType.TopLeft:
                            case ImageType.TopRight:
                            case ImageType.BottomLeft:
                            case ImageType.BottomRight:
                                if(j % 2 == 1&& i % 2 == 1)
                                {

                                    float offsetX = uvMin.x;
                                    if (x2e > xMax)
                                    {
                                        offsetX = uvMax.x - (uvMax.x - uvMin.x) * (xMax - x1) / (x2e - x1);
                                    }

                                    float offsetY = uvMin.y;
                                    if (y2e > yMax)
                                    {
                                        offsetY = uvMax.y - (uvMax.y - uvMin.y) * (yMax - y1) / (y2e - y1);
                                    }

                                    clipped1 = new Vector2(offsetX,offsetY) ;
                                    uvMin1 = uvMax;
                                }
                                else if (j % 2 == 1)
                                {
                                    float offsetY = 0;
                                    if (y2e > yMax)
                                    {
                                        offsetY = uvMax.y - (uvMax.y - uvMin.y) * (yMax - y1) / (y2e - y1);
                                    }
                                    //uvMin1 = new Vector2(uvMin.x, clipped.y);
                                    uvMin1 = new Vector2(uvMin.x, uvMax.y);
                                    //clipped1 = new Vector2(clipped.x, uvMin.y);
                                    clipped1 = new Vector2(clipped.x, offsetY);
                                }
                                else if (i % 2 == 1)
                                {
                                    float offsetX = 0;
                                    if (x2e > xMax)
                                    {
                                        offsetX = uvMax.x - (uvMax.x - uvMin.x) * (xMax - x1) / (x2e - x1);
                                    }
                                    uvMin1 = new Vector2(uvMax.x, uvMin.y);
                                    //clipped1 = new Vector2(uvMin.x, clipped.y);
                                    clipped1 = new Vector2(offsetX, clipped.y);
                                }
                                break;
                            default:
                                break;
                        }
                        
                        AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin1, clipped1);
                    }
                }
            }
            if (hasBorder)
            {
                clipped = uvMax;
                for (long j = 0; j < nTilesH; j++)
                {
                    float y1 = yMin + j * tileHeight;
                    float y2 = yMin + (j + 1) * tileHeight;
                    if (y2 > yMax)
                    {
                        clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                        y2 = yMax;
                    }
                    AddQuad(toFill,
                        new Vector2(0, y1) + rect.position,
                        new Vector2(xMin, y2) + rect.position,
                        color,
                        new Vector2(outer.x, uvMin.y),
                        new Vector2(uvMin.x, clipped.y));
                    AddQuad(toFill,
                        new Vector2(xMax, y1) + rect.position,
                        new Vector2(rect.width, y2) + rect.position,
                        color,
                        new Vector2(uvMax.x, uvMin.y),
                        new Vector2(outer.z, clipped.y));
                }

                // Bottom and top tiled border
                clipped = uvMax;
                for (long i = 0; i < nTilesW; i++)
                {
                    float x1 = xMin + i * tileWidth;
                    float x2 = xMin + (i + 1) * tileWidth;
                    if (x2 > xMax)
                    {
                        clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                        x2 = xMax;
                    }
                    AddQuad(toFill,
                        new Vector2(x1, 0) + rect.position,
                        new Vector2(x2, yMin) + rect.position,
                        color,
                        new Vector2(uvMin.x, outer.y),
                        new Vector2(clipped.x, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(x1, yMax) + rect.position,
                        new Vector2(x2, rect.height) + rect.position,
                        color,
                        new Vector2(uvMin.x, uvMax.y),
                        new Vector2(clipped.x, outer.w));
                }

                // Corners
                AddQuad(toFill,
                    new Vector2(0, 0) + rect.position,
                    new Vector2(xMin, yMin) + rect.position,
                    color,
                    new Vector2(outer.x, outer.y),
                    new Vector2(uvMin.x, uvMin.y));
                AddQuad(toFill,
                    new Vector2(xMax, 0) + rect.position,
                    new Vector2(rect.width, yMin) + rect.position,
                    color,
                    new Vector2(uvMax.x, outer.y),
                    new Vector2(outer.z, uvMin.y));
                AddQuad(toFill,
                    new Vector2(0, yMax) + rect.position,
                    new Vector2(xMin, rect.height) + rect.position,
                    color,
                    new Vector2(outer.x, uvMax.y),
                    new Vector2(uvMin.x, outer.w));
                AddQuad(toFill,
                    new Vector2(xMax, yMax) + rect.position,
                    new Vector2(rect.width, rect.height) + rect.position,
                    color,
                    new Vector2(uvMax.x, uvMax.y),
                    new Vector2(outer.z, outer.w));
            }
        }
        else
        {
            // Texture has no border, is in repeat mode and not packed. Use texture tiling.
            Vector2 uvScale = new Vector2((xMax - xMin) / tileWidth, (yMax - yMin) / tileHeight);

            if (fillCenter)
            {
                AddQuad(toFill, new Vector2(xMin, yMin) + rect.position, new Vector2(xMax, yMax) + rect.position, color, Vector2.Scale(uvMin, uvScale), Vector2.Scale(uvMax, uvScale));
            }
        }
    }
    
    static readonly Vector3[] s_Xy = new Vector3[4];
    static readonly Vector3[] s_Uv = new Vector3[4];

    /// <summary>
    /// Generate vertices for a filled Image.
    /// </summary>
    void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
    {
        toFill.Clear();

        if (fillAmount < 0.001f)
            return;

        Vector4 v = GetDrawingDimensions(preserveAspect);
        var v1 = v;
        Vector4 outer = sprite != null ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        UIVertex uiv = UIVertex.simpleVert;
        uiv.color = color;

        float tx0 = outer.x;
        float ty0 = outer.y;
        float tx1 = outer.z;
        float ty1 = outer.w;

        // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
        if (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical)
        {
            if (fillMethod == FillMethod.Horizontal)
            {
                float fill = (tx1 - tx0) * fillAmount;

                // right
                if (fillOrigin == 1)
                {
                    v.x = v.z - (v.z - v.x) * fillAmount;
                    tx0 = tx1 - fill;
                }
                // left
                else
                {
                    v.z = v.x + (v.z - v.x) * fillAmount;
                    tx1 = tx0 + fill;
                }
            }
            else if (fillMethod == FillMethod.Vertical)
            {
                float fill = (ty1 - ty0) * fillAmount;

                // top
                if (fillOrigin == 1)
                {
                    v.y = v.w - (v.w - v.y) * fillAmount;
                    ty0 = ty1 - fill;
                }
                // bottom
                else
                {
                    v.w = v.y + (v.w - v.y) * fillAmount;
                    ty1 = ty0 + fill;
                }
            }
        }

        s_Xy[0] = new Vector2(v.x, v.y);
        s_Xy[1] = new Vector2(v.x, v.w);
        s_Xy[2] = new Vector2(v.z, v.w);
        s_Xy[3] = new Vector2(v.z, v.y);

        s_Uv[0] = new Vector2(tx0, ty0);
        s_Uv[1] = new Vector2(tx0, ty1);
        s_Uv[2] = new Vector2(tx1, ty1);
        s_Uv[3] = new Vector2(tx1, ty0);

        // 全显示时 fillAmount=1，全不显示时 fillAmount=0
        switch (fillMethod)
        {
            case FillMethod.Horizontal:
            {
                switch (imageResourceType)
                {
                    case ImageType.TopHalf:
                        s_Xy[0].y = (v.y + v.w) / 2;
                        s_Xy[3].y = s_Xy[0].y;
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        s_Xy[1].y = s_Xy[0].y;
                        s_Xy[2].y = s_Xy[0].y;
                        s_Xy[0].y = v.y;
                        s_Xy[3].y = v.y;
                        s_Uv[0] = new Vector2(tx0, ty1);
                        s_Uv[1] = new Vector2(tx0, ty0);
                        s_Uv[2] = new Vector2(tx1, ty0);
                        s_Uv[3] = new Vector2(tx1, ty1);
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        break;
                    case ImageType.BottomHalf:
                        s_Xy[1].y = (v.y + v.w) / 2;
                        s_Xy[2].y = s_Xy[1].y;
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        s_Xy[0].y = s_Xy[1].y;
                        s_Xy[3].y = s_Xy[1].y;
                        s_Xy[1].y = v.w;
                        s_Xy[2].y = v.w;
                        s_Uv[0] = new Vector2(tx0, ty1);
                        s_Uv[1] = new Vector2(tx0, ty0);
                        s_Uv[2] = new Vector2(tx1, ty0);
                        s_Uv[3] = new Vector2(tx1, ty1);
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        break;
                    case ImageType.RightHalf:
                        // right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 1), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 1), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                    case ImageType.LeftHalf:
                        // left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 1), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 1), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                    case ImageType.TopRight:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), false, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), true, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                    case ImageType.BottomRight:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), false, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), true, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                    case ImageType.TopLeft:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), true, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), false, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                    case ImageType.BottomLeft:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), true, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), true, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), false, true, fillAmount,
                            true, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), false, false, fillAmount,
                            true, fillOrigin, toFill, color);
                        break;
                }
            }
                break;
            case FillMethod.Vertical:
            {
                switch (imageResourceType)
                {
                    case ImageType.TopHalf:
                        // top
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 1, 1), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 1, 0.5f), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                    case ImageType.BottomHalf:
                        // bottom
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 1, 0.5f), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // top
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 1, 1), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                    case ImageType.RightHalf:
                        s_Xy[0].x = (v.x + v.z) / 2;
                        s_Xy[1].x = s_Xy[0].x;
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        s_Xy[2].x = s_Xy[0].x;
                        s_Xy[3].x = s_Xy[0].x;
                        s_Xy[0].x = v.x;
                        s_Xy[1].x = v.x;
                        s_Uv[0] = new Vector2(tx1, ty0);
                        s_Uv[1] = new Vector2(tx1, ty1);
                        s_Uv[2] = new Vector2(tx0, ty1);
                        s_Uv[3] = new Vector2(tx0, ty0);
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        break;
                    case ImageType.LeftHalf:
                        s_Xy[2].x = (v.x + v.z) / 2;
                        s_Xy[3].x = s_Xy[2].x;
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        s_Xy[0].x = s_Xy[2].x;
                        s_Xy[1].x = s_Xy[2].x;
                        s_Xy[2].x = v.z;
                        s_Xy[3].x = v.z;
                        s_Uv[0] = new Vector2(tx1, ty0);
                        s_Uv[1] = new Vector2(tx1, ty1);
                        s_Uv[2] = new Vector2(tx0, ty1);
                        s_Uv[3] = new Vector2(tx0, ty0);
                        AddQuad(toFill, s_Xy, color, s_Uv);
                        break;
                    case ImageType.TopRight:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), true, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), true, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                    case ImageType.BottomRight:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), true, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), true, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                    case ImageType.TopLeft:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), true, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), true, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                    case ImageType.BottomLeft:
                        // top right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0.5f, 1, 1), true, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom right
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0.5f, 0, 1, 0.5f), true, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        // top left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0.5f, 0.5f, 1), false, true, fillAmount,
                            false, fillOrigin, toFill, color);
                        // bottom left
                        AddRectQuad(outer, s_Xy, s_Uv, v1, new Vector4(0, 0, 0.5f, 0.5f), false, false, fillAmount,
                            false, fillOrigin, toFill, color);
                        break;
                }
            }
                break;
            case FillMethod.Radial90:
            {
                // fillOrigin: 0 bottom left（右->上） | 1 top left（下->右） | 2 top right（左->下） | 3 bottom right（上->左）
                // 这里的fillOrigin指圆心位置
                switch (imageResourceType)
                {
                    case ImageType.TopHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.RightHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.LeftHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom left
                            case 0:
                                break;
                            // top left
                            case 1:
                                break;
                            // top right
                            case 2:
                                break;
                            // bottom right
                            case 3:
                                break;
                        }
                    }
                        break;
                }
                if (RadialCut(s_Xy, s_Uv, fillAmount, fillClockwise, fillOrigin))
                    AddQuad(toFill, s_Xy, color, s_Uv);
            }
                break;
            case FillMethod.Radial180:
            {
                // fillOrigin: 0 bottom | 1 left | 2 top | 3 right
                // 这里的fillOrigin指圆心位置
                switch (imageResourceType)
                {
                    case ImageType.TopHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.RightHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.LeftHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // left
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // right
                            case 3:
                                break;
                        }
                    }
                        break;
                }
                for (int side = 0; side < 2; ++side)
                {
                    float fx0, fx1, fy0, fy1;
                    int even = fillOrigin > 1 ? 1 : 0;

                    if (fillOrigin == 0 || fillOrigin == 2)
                    {
                        fy0 = 0f;
                        fy1 = 1f;
                        if (side == even)
                        {
                            fx0 = 0f;
                            fx1 = 0.5f;
                        }
                        else
                        {
                            fx0 = 0.5f;
                            fx1 = 1f;
                        }
                    }
                    else
                    {
                        fx0 = 0f;
                        fx1 = 1f;
                        if (side == even)
                        {
                            fy0 = 0.5f;
                            fy1 = 1f;
                        }
                        else
                        {
                            fy0 = 0f;
                            fy1 = 0.5f;
                        }
                    }

                    s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                    s_Xy[1].x = s_Xy[0].x;
                    s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                    s_Xy[3].x = s_Xy[2].x;

                    s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                    s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                    s_Xy[2].y = s_Xy[1].y;
                    s_Xy[3].y = s_Xy[0].y;

                    s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                    s_Uv[1].x = s_Uv[0].x;
                    s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                    s_Uv[3].x = s_Uv[2].x;

                    s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                    s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                    s_Uv[2].y = s_Uv[1].y;
                    s_Uv[3].y = s_Uv[0].y;

                    float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                    if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4)))
                    {
                        AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                }
            }
                break;
            case FillMethod.Radial360:
            {
                // fillOrigin: 0 bottom | 1 right | 2 top | 3 left
                // 这里的fillOrigin指起始半径的位置
                switch (imageResourceType)
                {
                    case ImageType.TopHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.RightHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.LeftHalf:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomRight:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.TopLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                    case ImageType.BottomLeft:
                    {
                        switch (fillOrigin)
                        {
                            // bottom
                            case 0:
                                break;
                            // right
                            case 1:
                                break;
                            // top
                            case 2:
                                break;
                            // left
                            case 3:
                                break;
                        }
                    }
                        break;
                }
                for (int corner = 0; corner < 4; ++corner)
                {
                    float fx0, fx1, fy0, fy1;

                    if (corner < 2)
                    {
                        fx0 = 0f;
                        fx1 = 0.5f;
                    }
                    else
                    {
                        fx0 = 0.5f;
                        fx1 = 1f;
                    }

                    if (corner == 0 || corner == 3)
                    {
                        fy0 = 0f;
                        fy1 = 0.5f;
                    }
                    else
                    {
                        fy0 = 0.5f;
                        fy1 = 1f;
                    }

                    s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                    s_Xy[1].x = s_Xy[0].x;
                    s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                    s_Xy[3].x = s_Xy[2].x;

                    s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                    s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                    s_Xy[2].y = s_Xy[1].y;
                    s_Xy[3].y = s_Xy[0].y;

                    s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                    s_Uv[1].x = s_Uv[0].x;
                    s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                    s_Uv[3].x = s_Uv[2].x;

                    s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                    s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                    s_Uv[2].y = s_Uv[1].y;
                    s_Uv[3].y = s_Uv[0].y;

                    float val = fillClockwise ?
                        fillAmount * 4f - ((corner + fillOrigin) % 4) :
                        fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                    if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4)))
                        AddQuad(toFill, s_Xy, color, s_Uv);
                }
            }
                break;
        }
    }

    /// <summary>
    /// 添加局部矩形
    /// </summary>
    /// <param name="outer">uv</param>
    /// <param name="xy">矩形顶点</param>
    /// <param name="uv">矩形uv</param>
    /// <param name="v1">未经裁剪的左下右上顶点位置</param>
    /// <param name="respectPos">期望的顶点位置（左下右上）</param>
    /// <param name="horizontalRevert">是否水平翻转</param>
    /// <param name="verticalRevert">是否竖直翻转</param>
    /// <param name="fillAmount">显示区域百分比</param>
    /// <param name="isHorizontalDirection">是否是水平方向裁切</param>
    /// <param name="fillOrigin">对齐方向</param>
    /// <param name="vertexHelper">vh</param>
    /// <param name="color">顶点颜色</param>
    static void AddRectQuad(Vector4 outer, Vector3[] xy, Vector3[] uv, Vector4 v1, Vector4 respectPos, bool horizontalRevert, bool verticalRevert, float fillAmount, bool isHorizontalDirection, int fillOrigin, VertexHelper vertexHelper, Color32 color)
    {
        var outer1 = outer;
        // fillOrigin：水平下1是右 0是左，竖直下1是顶，0是底
        // 顶点相对位置计算
        if (isHorizontalDirection)
        {
            if (fillOrigin == 1)
            {
                fillAmount = 1 - fillAmount;
                outer.x = outer1.x + (outer1.z - outer1.x) * ((Mathf.Clamp(fillAmount, respectPos.x, respectPos.z) - respectPos.x) / (respectPos.z - respectPos.x));
                respectPos.x = Mathf.Max(fillAmount, respectPos.x);
                respectPos.z = Mathf.Max(fillAmount, respectPos.z);
            }
            else
            {
                outer.z = outer1.z - (outer1.z - outer1.x) * ((respectPos.z - Mathf.Clamp(fillAmount, respectPos.x, respectPos.z))/ (respectPos.z - respectPos.x));
                respectPos.x = Mathf.Min(fillAmount, respectPos.x);
                respectPos.z = Mathf.Min(fillAmount, respectPos.z);
            }
        }
        else
        {
            if (fillOrigin == 1)
            {
                fillAmount = 1 - fillAmount;
                outer.y = outer1.y + (outer1.w - outer1.y) * ((Mathf.Clamp(fillAmount, respectPos.y, respectPos.w) - respectPos.y) / (respectPos.w - respectPos.y));
                respectPos.y = Mathf.Max(fillAmount, respectPos.y);
                respectPos.w = Mathf.Max(fillAmount, respectPos.w);
            }
            else
            {
                outer.w = outer1.w - (outer1.w - outer1.y) * ((respectPos.w - Mathf.Clamp(fillAmount, respectPos.y, respectPos.w)) / (respectPos.w - respectPos.y));
                respectPos.y = Mathf.Min(fillAmount, respectPos.y);
                respectPos.w = Mathf.Min(fillAmount, respectPos.w);
            }
        }

        xy[1].x = xy[0].x = Mathf.Lerp(v1.x, v1.z, respectPos.x);
        xy[3].y = xy[0].y = Mathf.Lerp(v1.y, v1.w, respectPos.y);
        xy[3].x = xy[2].x = Mathf.Lerp(v1.x, v1.z, respectPos.z);
        xy[2].y = xy[1].y = Mathf.Lerp(v1.y, v1.w, respectPos.w);
        
        // uv计算
        if (horizontalRevert)
        {
            uv[1].x = uv[0].x = outer1.z - (outer.x - outer1.x);
            uv[3].x = uv[2].x = outer1.x + (outer1.z - outer.z);
        }
        else
        {
            uv[1].x = uv[0].x = outer.x;
            uv[3].x = uv[2].x = outer.z;
        }

        if (verticalRevert)
        {
            uv[3].y = uv[0].y = outer1.w - (outer.y - outer1.y);
            uv[2].y = uv[1].y = outer1.y + (outer1.w - outer.w);
        }
        else
        {
            uv[3].y = uv[0].y = outer.y;
            uv[2].y = uv[1].y = outer.w;
        }
        
        AddQuad(vertexHelper, xy, color, uv);
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>
    /// <param name="xy">顶点</param>
    /// <param name="uv">uv</param>
    /// <param name="fill">显示的内容量 百分比</param>
    /// <param name="invert">是否顺时针</param>
    /// <param name="corner">对齐方式</param>
    static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
    {
        // Nothing to fill
        if (fill < 0.001f) return false;

        // Even corners invert the fill direction
        if ((corner & 1) == 1) invert = !invert;

        // Nothing to adjust
        if (!invert && fill > 0.999f) return true;

        // Convert 0-1 value into 0 to 90 degrees angle in radians
        float angle = Mathf.Clamp01(fill);
        if (invert) angle = 1f - angle;
        angle *= 90f * Mathf.Deg2Rad;

        // Calculate the effective X and Y factors
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        RadialCut(xy, cos, sin, invert, corner);
        RadialCut(uv, cos, sin, invert, corner);
        return true;
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>
    static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
    {
        int i0 = corner;
        int i1 = ((corner + 1) % 4);
        int i2 = ((corner + 2) % 4);
        int i3 = ((corner + 3) % 4);

        if ((corner & 1) == 1)
        {
            if (sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if (invert)
                {
                    xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i2].x = xy[i1].x;
                }
            }
            else if (cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if (!invert)
                {
                    xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i3].y = xy[i2].y;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
        }
        else
        {
            if (cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if (!invert)
                {
                    xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i2].y = xy[i1].y;
                }
            }
            else if (sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if (invert)
                {
                    xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i3].x = xy[i2].x;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
        }
    }
}

