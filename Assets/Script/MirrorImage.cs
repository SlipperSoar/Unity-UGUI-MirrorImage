using System;
using System.Collections.Generic;
using System.Linq;
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
                if (fillAmount >= 0.999f)
                {
                    GenerateSimpleSprite(toFill, preserveAspect);
                }
                else
                {
                    GenerateFilledSprite(toFill, preserveAspect);
                }
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
        Color32 color32 = color;
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
                vh.AddVert(new Vector3(v1.x, v1.y), color32, new Vector2(uv.x, uv.w));
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
                AddRectRadial90(outer, v1, fillAmount, fillOrigin, fillClockwise, imageResourceType, toFill, color);
            }
                break;
            case FillMethod.Radial180:
            {
                AddRectRadial180(outer, v1, fillAmount, fillOrigin, fillClockwise, imageResourceType, toFill, color);
            }
                break;
            case FillMethod.Radial360:
            {
                AddRectRadial360(outer, v1, fillAmount, fillOrigin, fillClockwise, imageResourceType, toFill, color);
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
    /// <param name="expectPos">期望的顶点位置（左下右上）</param>
    /// <param name="horizontalRevert">是否水平翻转</param>
    /// <param name="verticalRevert">是否竖直翻转</param>
    /// <param name="fillAmount">显示区域百分比</param>
    /// <param name="isHorizontalDirection">是否是水平方向裁切</param>
    /// <param name="fillOrigin">对齐方向</param>
    /// <param name="vertexHelper">vh</param>
    /// <param name="color">顶点颜色</param>
    static void AddRectQuad(Vector4 outer, Vector3[] xy, Vector3[] uv, Vector4 v1, Vector4 expectPos, bool horizontalRevert, bool verticalRevert, float fillAmount, bool isHorizontalDirection, int fillOrigin, VertexHelper vertexHelper, Color32 color)
    {
        var outer1 = outer;
        // fillOrigin：水平下1是右 0是左，竖直下1是顶，0是底
        // 顶点相对位置计算
        if (isHorizontalDirection)
        {
            if (fillOrigin == 1)
            {
                fillAmount = 1 - fillAmount;
                outer.x = outer1.x + (outer1.z - outer1.x) * ((Mathf.Clamp(fillAmount, expectPos.x, expectPos.z) - expectPos.x) / (expectPos.z - expectPos.x));
                expectPos.x = Mathf.Max(fillAmount, expectPos.x);
                expectPos.z = Mathf.Max(fillAmount, expectPos.z);
            }
            else
            {
                outer.z = outer1.z - (outer1.z - outer1.x) * ((expectPos.z - Mathf.Clamp(fillAmount, expectPos.x, expectPos.z))/ (expectPos.z - expectPos.x));
                expectPos.x = Mathf.Min(fillAmount, expectPos.x);
                expectPos.z = Mathf.Min(fillAmount, expectPos.z);
            }
        }
        else
        {
            if (fillOrigin == 1)
            {
                fillAmount = 1 - fillAmount;
                outer.y = outer1.y + (outer1.w - outer1.y) * ((Mathf.Clamp(fillAmount, expectPos.y, expectPos.w) - expectPos.y) / (expectPos.w - expectPos.y));
                expectPos.y = Mathf.Max(fillAmount, expectPos.y);
                expectPos.w = Mathf.Max(fillAmount, expectPos.w);
            }
            else
            {
                outer.w = outer1.w - (outer1.w - outer1.y) * ((expectPos.w - Mathf.Clamp(fillAmount, expectPos.y, expectPos.w)) / (expectPos.w - expectPos.y));
                expectPos.y = Mathf.Min(fillAmount, expectPos.y);
                expectPos.w = Mathf.Min(fillAmount, expectPos.w);
            }
        }

        xy[1].x = xy[0].x = Mathf.Lerp(v1.x, v1.z, expectPos.x);
        xy[3].y = xy[0].y = Mathf.Lerp(v1.y, v1.w, expectPos.y);
        xy[3].x = xy[2].x = Mathf.Lerp(v1.x, v1.z, expectPos.z);
        xy[2].y = xy[1].y = Mathf.Lerp(v1.y, v1.w, expectPos.w);
        
        // uv计算
        SetUV(uv, outer, outer1, horizontalRevert, verticalRevert);
        
        AddQuad(vertexHelper, xy, color, uv);
    }

    static void SetUV(Vector3[] uv, Vector4 outer, Vector4 outer1, bool horizontalRevert, bool verticalRevert)
    {
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
    }

    /// <summary>
    /// 添加三角形
    /// </summary>
    /// <param name="normalizedXY">归一化顶点坐标</param>
    /// <param name="vertexHelper">顶点信息</param>
    /// <param name="color">顶点颜色</param>
    /// <param name="v">原始顶点位置</param>
    /// <param name="outer">原始uv</param>
    /// <param name="xRange">归一化x范围</param>
    /// <param name="yRange">归一化y范围</param>
    /// <param name="horizontalRevert">是否水平翻转</param>
    /// <param name="verticalRevert">是否竖直翻转</param>
    static void AddTriangle(Vector2[] normalizedXY, VertexHelper vertexHelper, Color32 color, Vector4 v, Vector4 outer, Vector2 xRange, Vector2 yRange, bool horizontalRevert, bool verticalRevert)
    {
        if (normalizedXY == null || normalizedXY.Length == 0)
        {
            return;
        }

        var vertCount = vertexHelper.currentVertCount;
        // 当前进行到的三角顶点索引
        var triangleIndex = 0;
        var triangleStartIndex = vertCount;
        // 先把第一个顶点算出来
        var firstVt = GetUIVertex(color, normalizedXY[triangleIndex++], v, outer, xRange, yRange, horizontalRevert, verticalRevert);
        vertexHelper.AddVert(firstVt);
        UIVertex? vt = null;
        while (triangleIndex < normalizedXY.Length)
        {
            if (!vt.HasValue)
            {
                vt = GetUIVertex(color, normalizedXY[triangleIndex++], v, outer, xRange, yRange, horizontalRevert,
                    verticalRevert);
                vertexHelper.AddVert(vt.Value);
            }

            vt = GetUIVertex(color, normalizedXY[triangleIndex++], v, outer, xRange, yRange, horizontalRevert,
                verticalRevert);
            vertexHelper.AddVert(vt.Value);
            vertCount = vertexHelper.currentVertCount;
            vertexHelper.AddTriangle(triangleStartIndex, vertCount - 2, vertCount - 1);
        }
    }

    static UIVertex GetUIVertex(Color32 color, Vector2 normalizedXY, Vector4 v, Vector4 outer, Vector2 xRange, Vector2 yRange, bool horizontalRevert, bool verticalRevert)
    {
        var uv = new Vector4(outer.x + (outer.z - outer.x) * (normalizedXY.x - xRange.x) / (xRange.y - xRange.x), outer.y + (outer.w - outer.y) * (normalizedXY.y - yRange.x) / (yRange.y - yRange.x));
        // 计算翻转
        if (horizontalRevert) uv.x = outer.z - (uv.x - outer.x);
        if (verticalRevert) uv.y = outer.w - (uv.y - outer.y);
        // Debug.Log($"<color=yellow> xy: {normalizedXY}, uv: {uv}, xRange: {xRange}, yRange: {yRange}, horizontalRevert: {horizontalRevert}, verticalRevert: {verticalRevert} </color>");
        // uv计算用的x
        return new UIVertex()
        {
            color = color,
            position = new Vector3(Mathf.Lerp(v.x, v.z, normalizedXY.x), Mathf.Lerp(v.y, v.w, normalizedXY.y)),
            uv0 = uv
        };
    }

    /// <summary>
    /// 切割矩形
    /// </summary>
    /// <param name="normalizedXY">归一化矩形顶点</param>
    /// <param name="divideCenterX">是否沿X中线切割</param>
    /// <param name="divideCenterY">是否沿Y中线切割</param>
    /// <param name="isRevertX">是否会翻转X切割的顶点顺序</param>
    /// <param name="isRevertY">是否会翻转Y切割的顶点顺序</param>
    /// <returns>切割后的顶点信息（每一组不一定是4个顶点位置）</returns>
    static Vector2[][] DivideRect(Vector2[] normalizedXY, bool divideCenterX, bool divideCenterY, out bool[] isRevertX, out bool isRevertY)
    {
        var rectCount = 1 * (divideCenterX ? 2 : 1) * (divideCenterY ? 2 : 1);
        var result = new Vector2[rectCount][];
        result[0] = normalizedXY;
        isRevertX = new []{ false, false };
        isRevertY = false;

        if (rectCount == 1)
        {
            return result;
        }

        var divideCount = 1;
        if (divideCenterY)
        {
            // 通过索引来区分顶点所处的半边
            var centers = new Tuple<Vector2, float>[2];
            var centerIndex = 0;
            var tempRects = new List<List<Vector2>>();
            for (int i = 0; i < divideCount; i++)
            {
                var rect = result[i];
                // 空的切割成两个空
                if (rect.Length == 0)
                {
                    tempRects.Add(new List<Vector2>());
                    tempRects.Add(new List<Vector2>());
                    continue;
                }
                // 首先检查是否会有交点
                if (rect.All(p => p.y >= 0.5f))
                {
                    // 中线位于矩形下方
                    tempRects.Add(new List<Vector2>());
                    tempRects.Add(new List<Vector2>(rect));
                    continue;
                }
                else if (rect.All(p => p.y <= 0.5f))
                {
                    // 中线位于矩形上方
                    tempRects.Add(new List<Vector2>(rect));
                    tempRects.Add(new List<Vector2>());
                    continue;
                }
                else
                {
                    for (int j = 0; j < rect.Length; j++)
                    {
                        var nextIndex = (j + 1) % rect.Length;
                        if (rect[j].y <= 0.5f && rect[nextIndex].y >= 0.5f)
                        {
                            var percent = (0.5f - rect[j].y) / (rect[nextIndex].y - rect[j].y);
                            centers[centerIndex++] = new Tuple<Vector2, float>(Vector2.Lerp(rect[j], rect[nextIndex], percent), j + 0.5f);
                            if (j == 3) isRevertY = true;
                        }
                        else if (rect[nextIndex].y <= 0.5f && rect[j].y >= 0.5f)
                        {
                            var percent = (0.5f - rect[nextIndex].y) / (rect[j].y - rect[nextIndex].y);
                            centers[centerIndex++] = new Tuple<Vector2, float>(Vector2.Lerp(rect[nextIndex], rect[j], percent), j + 0.5f);
                        }
                    }
                }
                
                // 理论上这里已经得到两个Center了
                var centerCount = 0;
                // 从索引0开始的顶点列表
                var newRect1 = new List<Vector2>();
                // 非0开始的顶点列表
                var newRect2 = new List<Vector2>();
                
                var isDividing = false;
                for (int j = 0; j < rect.Length; j++)
                {
                    if (centerCount < 2 && centers[centerCount].Item2 < j)
                    {
                        var centerPos = centers[centerCount];
                        if (isDividing)
                        {
                            newRect2.Add(centerPos.Item1);
                            newRect1.Add(centerPos.Item1);
                            newRect1.Add(rect[j]);
                            isDividing = false;
                        }
                        else
                        {
                            newRect1.Add(centerPos.Item1);
                            newRect2.Add(centerPos.Item1);
                            newRect2.Add(rect[j]);
                            isDividing = true;
                        }
                        
                        centerCount++;
                    }
                    else
                    {
                        if (isDividing)
                        {
                            newRect2.Add(rect[j]);
                        }
                        else
                        {
                            newRect1.Add(rect[j]);
                        }
                    }
                }

                // 说明最后一个交点在0-3之间，没有被遍历到
                if (centerCount < 2)
                {
                    newRect1.Add(centers[centerCount].Item1);
                    newRect2.Add(centers[centerCount].Item1);
                }
                
                tempRects.Add(newRect1);
                tempRects.Add(newRect2);
                centerIndex = 0;
            }
            
            divideCount *= 2;

            for (int i = 0; i < divideCount; i++)
            {
                result[i] = tempRects[i].ToArray();
            }
        }

        if (divideCenterX)
        {
            // 通过索引来区分顶点所处的半边
            var centers = new Tuple<Vector2, float>[2];
            var centerIndex = 0;
            var tempRects = new List<List<Vector2>>();
            for (int i = 0; i < divideCount; i++)
            {
                var rect = result[i];
                // 空的切割成两个空
                if (rect.Length == 0)
                {
                    tempRects.Add(new List<Vector2>());
                    tempRects.Add(new List<Vector2>());
                    continue;
                }
                
                // 以x值最小的点作为起点重新排序，以免y切割后出现起点不是偏左下的情况
                var minIndex = 0;
                var rectList = new List<Vector2>(rect);
                var minX = float.MaxValue;
                for (int j = 0; j < rect.Length; j++)
                {
                    var v = rect[j];
                    if (v.x < minX)
                    {
                        minX = v.x;
                        minIndex = j;
                    }
                }

                for (int j = 0; j < rect.Length; j++)
                {
                    rect[j] = rectList[(minIndex + j) % rect.Length];
                }

                // 首先检查是否会有交点
                if (rect.All(p => p.x >= 0.5f))
                {
                    // 中线位于矩形左方
                    tempRects.Add(new List<Vector2>());
                    tempRects.Add(new List<Vector2>(rect));
                    continue;
                }
                else if (rect.All(p => p.x <= 0.5f))
                {
                    // 中线位于矩形右方
                    tempRects.Add(new List<Vector2>(rect));
                    tempRects.Add(new List<Vector2>());
                    continue;
                }
                else
                {
                    for (int j = 0; j < rect.Length; j++)
                    {
                        var nextIndex = (j + 1) % rect.Length;
                        if (rect[j].x <= 0.5f && rect[nextIndex].x >= 0.5f)
                        {
                            var percent = (0.5f - rect[j].x) / (rect[nextIndex].x - rect[j].x);
                            centers[centerIndex++] =
                                new Tuple<Vector2, float>(Vector2.Lerp(rect[j], rect[nextIndex], percent), j + 0.5f);
                        }
                        else if (rect[nextIndex].x <= 0.5f && rect[j].x >= 0.5f)
                        {
                            var percent = (0.5f - rect[nextIndex].x) / (rect[j].x - rect[nextIndex].x);
                            centers[centerIndex++] =
                                new Tuple<Vector2, float>(Vector2.Lerp(rect[nextIndex], rect[j], percent), j + 0.5f);
                            // [X] 当0在右1在左时被x中线切割，0属于右侧，此时认为x发生了翻转
                            if (j == 0) isRevertX[i] = true;
                        }
                    }
                }
                
                // 理论上这里已经得到两个Center了
                var centerCount = 0;
                // 从索引0开始的顶点列表
                var newRect1 = new List<Vector2>();
                // 非0开始的顶点列表
                var newRect2 = new List<Vector2>();

                var isDividing = false;
                for (int j = 0; j < rect.Length; j++)
                {
                    if (centerCount < 2 && centers[centerCount].Item2 < j)
                    {
                        var centerPos = centers[centerCount];
                        if (isDividing)
                        {
                            newRect2.Add(centerPos.Item1);
                            newRect1.Add(centerPos.Item1);
                            newRect1.Add(rect[j]);
                            isDividing = false;
                        }
                        else
                        {
                            newRect1.Add(centerPos.Item1);
                            newRect2.Add(centerPos.Item1);
                            newRect2.Add(rect[j]);
                            isDividing = true;
                        }
                        
                        centerCount++;
                    }
                    else
                    {
                        if (isDividing)
                        {
                            newRect2.Add(rect[j]);
                        }
                        else
                        {
                            newRect1.Add(rect[j]);
                        }
                    }
                }
                
                // 说明最后一个交点在0-3之间，没有被遍历到
                if (centerCount < 2)
                {
                    newRect1.Add(centers[centerCount].Item1);
                    newRect2.Add(centers[centerCount].Item1);
                }
                
                tempRects.Add(newRect1);
                tempRects.Add(newRect2);
                centerIndex = 0;
            }
            
            divideCount *= 2;

            for (int i = 0; i < divideCount; i++)
            {
                result[i] = tempRects[i].ToArray();
            }
        }

        return result;
    }
    
    /// <summary>
    /// 添加90度扇形矩形
    /// </summary>
    /// <param name="outer">原uv</param>
    /// <param name="v1">未经裁剪的矩形顶点信息</param>
    /// <param name="fillAmount"></param>
    /// <param name="fillOrigin">对齐方式（顶点位置）</param>
    /// <param name="clockwise">是否逆时针</param>
    /// <param name="imageType"></param>
    /// <param name="vertexHelper"></param>
    /// <param name="color"></param>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="ImageType"/> 超出范围</exception>
    static void AddRectRadial90(Vector4 outer, Vector4 v1, float fillAmount, int fillOrigin, bool clockwise, ImageType imageType, VertexHelper vertexHelper, Color32 color)
    {
        // 先计算外部总顶点
        var outerPoses = new Vector2[]
        {
            Vector2.zero,
            new Vector2(0, 1),
            Vector2.one,
            new Vector2(1, 0)
        };
        // fillOrigin: 0 bottom left（右->上） | 1 top left（下->右） | 2 top right（左->下） | 3 bottom right（上->左）
        // 这里的fillOrigin指圆心位置，括号为逆时针
        var radAngle = (1 - fillAmount) * Mathf.PI / 2;
        switch (fillOrigin)
        {
            // bottom left
            case 0:
                if (clockwise)
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[3].y = Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[3].y = 1;
                        outerPoses[3].x = outerPoses[2].x = 1 / Mathf.Tan(radAngle);
                    }
                }
                else
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[1].x = Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[1].x = 1;
                        outerPoses[1].y = outerPoses[2].y = 1 / Mathf.Tan(radAngle);
                    }
                }
                break;
            // top left
            case 1:
                if (clockwise)
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[0].x = Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[0].x = 1;
                        outerPoses[0].y = outerPoses[3].y = 1 - 1 / Mathf.Tan(radAngle);
                    }
                }
                else
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[2].y = 1 - Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[2].y = 0;
                        outerPoses[2].x = outerPoses[3].x = 1 / Mathf.Tan(radAngle);
                    }
                }
                break;
            // top right
            case 2:
                if (clockwise)
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[1].y = 1 - Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[1].y = 0;
                        outerPoses[0].x = outerPoses[1].x = 1 - 1 / Mathf.Tan(radAngle);
                    }
                }
                else
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[3].x = 1 - Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[3].x = 0;
                        outerPoses[3].y = outerPoses[0].y = 1 - 1 / Mathf.Tan(radAngle);
                    }
                }
                break;
            // bottom right
            case 3:
                if (clockwise)
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[2].x = 1 - Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[2].x = 0;
                        outerPoses[2].y = outerPoses[1].y = 1 / Mathf.Tan(radAngle);
                    }
                }
                else
                {
                    // 45度
                    if (fillAmount > 0.5f)
                    {
                        outerPoses[0].y = Mathf.Tan(radAngle);
                    }
                    else
                    {
                        outerPoses[0].y = 1;
                        outerPoses[0].x = outerPoses[1].x = 1 - 1 / Mathf.Tan(radAngle);
                    }
                }
                break;
        }
        
        // 顶点计算
        // uv计算，关键在于判断是否翻转，这里通过imageType+index来判断
        // 固定以左 -> 右， 下 -> 上， 左下 -> 右下 -> 左上 -> 右上 的顺序（先上下分，再左右分）
        switch (imageType)
        {
            case ImageType.TopHalf:
            case ImageType.BottomHalf:
            {
                var verticalRevert = imageType == ImageType.BottomHalf;
                var rects = DivideRect(outerPoses, false, true, out var isRevertX, out var isRevertY);
                var xRange = new Vector2(0, 1);
                // top
                AddTriangle(rects[isRevertY ? 0 : 1], vertexHelper, color, v1, outer, xRange, new Vector2(0.5f, 1), false, verticalRevert);
                // bottom
                AddTriangle(rects[isRevertY ? 1 : 0], vertexHelper, color, v1, outer, xRange, new Vector2(0, 0.5f), false, !verticalRevert);
            }
                break;
            case ImageType.RightHalf:
            case ImageType.LeftHalf:
            {
                var horizontalRevert = imageType == ImageType.RightHalf;
                var rects = DivideRect(outerPoses, true, false, out var isRevertX, out var isRevertY);
                var yRange = new Vector2(0, 1);
                // left
                AddTriangle(rects[isRevertX[0] ? 1 : 0], vertexHelper, color, v1, outer, new Vector2(0, 0.5f), yRange, horizontalRevert, false);
                // right
                AddTriangle(rects[isRevertX[0] ? 0 : 1], vertexHelper, color, v1, outer, new Vector2(0.5f, 1), yRange, !horizontalRevert, false);
            }
                break;
            case ImageType.TopRight:
            case ImageType.BottomRight:
            case ImageType.TopLeft:
            case ImageType.BottomLeft:
            {
                var verticalRevert = imageType == ImageType.BottomRight || imageType == ImageType.BottomLeft;
                var horizontalRevert = imageType == ImageType.BottomRight || imageType == ImageType.TopRight;
                var rects = DivideRect(outerPoses, true, true, out var isRevertX, out var isRevertY);
                var topRange = new Vector2(0.5f, 1);
                var bottomRange = new Vector2(0, 0.5f);
                var leftRange = new Vector2(0, 0.5f);
                var rightRange = new Vector2(0.5f, 1);
                // 当y翻转时，0 1为上， 2 3为下，当x翻转时，+0为右， +1为左
                var topStartIndex = isRevertY ? 0 : 2;
                var bottomStartIndex = isRevertY ? 2 : 0;
                var isTopXRevert = isRevertY ? isRevertX[0] : isRevertX[1];
                var isBottomXRevert = isRevertY ? isRevertX[1] : isRevertX[0];
                // top right
                AddTriangle(rects[topStartIndex + (isTopXRevert ? 0 : 1)], vertexHelper, color, v1, outer, rightRange, topRange, !horizontalRevert, verticalRevert);
                // bottom right
                AddTriangle(rects[bottomStartIndex + (isBottomXRevert ? 0 : 1)], vertexHelper, color, v1, outer, rightRange, bottomRange, !horizontalRevert, !verticalRevert);
                // top left
                AddTriangle(rects[topStartIndex + (isTopXRevert ? 1 : 0)], vertexHelper, color, v1, outer, leftRange, topRange, horizontalRevert, verticalRevert);
                // bottom left
                AddTriangle(rects[bottomStartIndex + (isBottomXRevert ? 1 : 0)], vertexHelper, color, v1, outer, leftRange, bottomRange, horizontalRevert, !verticalRevert);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null);
        }
    }

    /// <summary>
    /// 添加180度扇形矩形
    /// </summary>
    /// <param name="outer">原uv</param>
    /// <param name="v1">未经裁剪的矩形顶点信息</param>
    /// <param name="fillAmount"></param>
    /// <param name="fillOrigin">对齐方式（顶点位置）</param>
    /// <param name="clockwise">是否逆时针</param>
    /// <param name="imageType"></param>
    /// <param name="vertexHelper"></param>
    /// <param name="color"></param>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="ImageType"/> 超出范围</exception>
    static void AddRectRadial180(Vector4 outer, Vector4 v1, float fillAmount, int fillOrigin, bool clockwise,
        ImageType imageType, VertexHelper vertexHelper, Color32 color)
    {
        // 计算外部总顶点，顺序为逆时针，底部中点为起点
        var vs = new Vector2[]
        {
            new Vector2(0, 0),//0
            new Vector2(0, 1),//1
            new Vector2(1, 1),//2
            new Vector2(1, 0),//3
        };
        
        // 矩形列表
        var rects = new List<Vector2[]>();

        var fill = 1 - fillAmount;
        // fillOrigin: 0 bottom | 1 left | 2 top | 3 right
        // 这里的fillOrigin指圆心位置
        var radAngle = fill * Mathf.PI;
        if (!Mathf.Approximately(fill % 0.5f, 0))
        {
            // TODO: 计算外部的所有顶点
            // 先按照逆时针方向计算（勾上clockwise），然后再根据情况翻转
            switch (fillOrigin)
            {
                // bottom
                case 0:
                {
                    if (fill >= 0.5f)
                    {
                        radAngle -= Mathf.PI / 2;
                        var rect = new[] { vs[0], vs[1], new Vector2(0.5f, 1), new Vector2(0.5f, 0) };

                        rect[2].x -= Mathf.Tan(radAngle);
                        if (rect[2].x < 0)
                        {
                            rect[2].x = rect[1].x;
                            rect[2].y = rect[1].y = 0.5f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rect = MirrorRect(rect, xAxis: 0.5f);
                        }
                        
                        rects.Add(rect);
                    }
                    else
                    {
                        var leftRect = new[] { vs[0], vs[1], new Vector2(0.5f, 1), new Vector2(0.5f, 0) };
                        var rightRect = new[] { new Vector2(0.5f, 0), new Vector2(0.5f, 1), vs[2], vs[3] };
                        
                        rightRect[3].y += 0.5f * Mathf.Tan(radAngle);
                        if (rightRect[3].y > 1)
                        {
                            rightRect[3].y = rightRect[2].y;
                            rightRect[3].x = rightRect[2].x = rightRect[1].x + 1f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rightRect = MirrorRect(rightRect, xAxis: 0.5f);
                            leftRect = MirrorRect(leftRect, xAxis: 0.5f);
                        }

                        rects.Add(rightRect);
                        rects.Add(leftRect);
                    }
                }
                    break;
                // left
                case 1:
                {
                    if (fill >= 0.5f)
                    {
                        radAngle -= Mathf.PI / 2;
                        var rect = new[] { new Vector2(0, 0.5f), vs[1], vs[2], new Vector2(1, 0.5f) };

                        rect[3].y += Mathf.Tan(radAngle);
                        if (rect[3].y > 1)
                        {
                            rect[3].y = rect[2].y;
                            rect[3].x = rect[2].x = rect[1].x + 0.5f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rect = MirrorRect(rect, yAxis: 0.5f);
                        }
                        
                        rects.Add(rect);
                    }
                    else
                    {
                        var bottomRect = new[] { vs[0], new Vector2(0, 0.5f), new Vector2(1, 0.5f), vs[3] };
                        var topRect = new[] { new Vector2(0, 0.5f), vs[1], vs[2], new Vector2(1, 0.5f) };
                        
                        bottomRect[0].x += 0.5f * Mathf.Tan(radAngle);
                        if (bottomRect[0].x > 1)
                        {
                            bottomRect[0].x = bottomRect[3].x;
                            bottomRect[0].y = bottomRect[3].y = bottomRect[2].y - 1f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            topRect = MirrorRect(topRect, yAxis: 0.5f);
                            bottomRect = MirrorRect(bottomRect, yAxis: 0.5f);
                        }

                        rects.Add(topRect);
                        rects.Add(bottomRect);
                    }
                }
                    break;
                // top
                case 2:
                {
                    if (fill >= 0.5f)
                    {
                        radAngle -= Mathf.PI / 2;
                        var rect = new[] { new Vector2(0.5f, 0), new Vector2(0.5f, 1), vs[2], vs[3] };

                        rect[0].x += Mathf.Tan(radAngle);
                        if (rect[0].x > 1)
                        {
                            rect[0].x = rect[3].x;
                            rect[0].y = rect[3].y = rect[2].y - 0.5f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rect = MirrorRect(rect, xAxis: 0.5f);
                        }
                        
                        rects.Add(rect);
                    }
                    else
                    {
                        var leftRect = new[] { vs[0], vs[1], new Vector2(0.5f, 1), new Vector2(0.5f, 0) };
                        var rightRect = new[] { new Vector2(0.5f, 0), new Vector2(0.5f, 1), vs[2], vs[3] };
                        
                        leftRect[1].y -= 0.5f * Mathf.Tan(radAngle);
                        if (leftRect[1].y < 0)
                        {
                            leftRect[1].y = leftRect[0].y;
                            leftRect[1].x = leftRect[0].x = leftRect[3].x - 1f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rightRect = MirrorRect(rightRect, xAxis: 0.5f);
                            leftRect = MirrorRect(leftRect, xAxis: 0.5f);
                        }

                        rects.Add(rightRect);
                        rects.Add(leftRect);
                    }
                }
                    break;
                // right
                case 3:
                {
                    if (fill >= 0.5f)
                    {
                        radAngle -= Mathf.PI / 2;
                        var rect = new[] { vs[0], new Vector2(0, 0.5f), new Vector2(1, 0.5f), vs[3] };

                        rect[1].y -= Mathf.Tan(radAngle);
                        if (rect[1].y < 0)
                        {
                            rect[1].y = rect[0].y;
                            rect[1].x = rect[0].x = rect[3].x - 0.5f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            rect = MirrorRect(rect, yAxis: 0.5f);
                        }
                        
                        rects.Add(rect);
                    }
                    else
                    {
                        var bottomRect = new[] { vs[0], new Vector2(0, 0.5f), new Vector2(1, 0.5f), vs[3] };
                        var topRect = new[] { new Vector2(0, 0.5f), vs[1], vs[2], new Vector2(1, 0.5f) };
                        
                        topRect[2].x -= 0.5f * Mathf.Tan(radAngle);
                        if (topRect[2].x < 0)
                        {
                            topRect[2].x = topRect[1].x;
                            topRect[2].y = topRect[1].y = topRect[0].y + 1f / Mathf.Tan(radAngle);
                        }

                        if (!clockwise)
                        {
                            topRect = MirrorRect(topRect, yAxis: 0.5f);
                            bottomRect = MirrorRect(bottomRect, yAxis: 0.5f);
                        }

                        rects.Add(topRect);
                        rects.Add(bottomRect);
                    }
                }
                    break;
            }
        }

        var leftHalfRange = new Vector2(0, 0.5f);
        var rightHalfRange = new Vector2(0.5f, 1);
        var allRange = new Vector2(0, 1);
        
        // 固定以左 -> 右， 下 -> 上， 左下 -> 右下 -> 左上 -> 右上 的顺序（先上下分，再左右分）
        switch (imageType)
        {
            case ImageType.TopHalf:
            case ImageType.BottomHalf:
            {
                var verticalRevert = imageType == ImageType.BottomHalf;
                for (var i = 0; i < rects.Count; i++)
                {
                    var rect = rects[i];
                    var divideRects = DivideRect(rect, false, true, out var isRevertX, out var isRevertY);
                    // top
                    AddTriangle(divideRects[isRevertY ? 0 : 1], vertexHelper, color, v1, outer, allRange, rightHalfRange, false, verticalRevert);
                    // bottom
                    AddTriangle(divideRects[isRevertY ? 1 : 0], vertexHelper, color, v1, outer, allRange, leftHalfRange, false, !verticalRevert);
                }
            }
                break;
            case ImageType.RightHalf:
            case ImageType.LeftHalf:
            {
                var horizontalRevert = imageType == ImageType.RightHalf;
                for (var i = 0; i < rects.Count; i++)
                {
                    var rect = rects[i];
                    var divideRects = DivideRect(rect, true, false, out var isRevertX, out var isRevertY);
                    // left
                    AddTriangle(divideRects[isRevertX[0] ? 1 : 0], vertexHelper, color, v1, outer, leftHalfRange, allRange, horizontalRevert, false);
                    // right
                    AddTriangle(divideRects[isRevertX[0] ? 0 : 1], vertexHelper, color, v1, outer, rightHalfRange, allRange, !horizontalRevert, false);
                }
            }
                break;
            case ImageType.TopRight:
            case ImageType.BottomRight:
            case ImageType.TopLeft:
            case ImageType.BottomLeft:
            {
                var verticalRevert = imageType == ImageType.BottomRight || imageType == ImageType.BottomLeft;
                var horizontalRevert = imageType == ImageType.BottomRight || imageType == ImageType.TopRight;
                for (var i = 0; i < rects.Count; i++)
                {
                    var rect = rects[i];
                    var divideRects = DivideRect(rect, true, true, out var isRevertX, out var isRevertY);
                    // 当y翻转时，0 1为上， 2 3为下，当x翻转时，+0为右， +1为左
                    var topStartIndex = isRevertY ? 0 : 2;
                    var bottomStartIndex = isRevertY ? 2 : 0;
                    var isTopXRevert = isRevertY ? isRevertX[0] : isRevertX[1];
                    var isBottomXRevert = isRevertY ? isRevertX[1] : isRevertX[0];
                    // top right
                    AddTriangle(divideRects[topStartIndex + (isTopXRevert ? 0 : 1)], vertexHelper, color, v1, outer, rightHalfRange, rightHalfRange, !horizontalRevert, verticalRevert);
                    // bottom right
                    AddTriangle(divideRects[bottomStartIndex + (isBottomXRevert ? 0 : 1)], vertexHelper, color, v1, outer, rightHalfRange, leftHalfRange, !horizontalRevert, !verticalRevert);
                    // top left
                    AddTriangle(divideRects[topStartIndex + (isTopXRevert ? 1 : 0)], vertexHelper, color, v1, outer, leftHalfRange, rightHalfRange, horizontalRevert, verticalRevert);
                    // bottom left
                    AddTriangle(divideRects[bottomStartIndex + (isBottomXRevert ? 1 : 0)], vertexHelper, color, v1, outer, leftHalfRange, leftHalfRange, horizontalRevert, !verticalRevert);
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null);
        }
    }

    /// <summary>
    /// 翻转矩形
    /// </summary>
    /// <param name="rect">原始矩形</param>
    /// <param name="isHorizontal">是否是水平反转</param>
    /// <returns></returns>
    static Vector2[] FlipRect(Vector2[] rect, bool isHorizontal)
    {
        var result = new Vector2[4];
        if (isHorizontal)
        {
            result[0] = rect[3];
            result[1] = rect[2];
            result[2] = rect[1];
            result[3] = rect[0];
        }
        else
        {
            result[0] = rect[1];
            result[1] = rect[0];
            result[2] = rect[3];
            result[3] = rect[2];
        }

        return result;
    }
    
    /// <summary>
    /// 按照轴来镜像矩形
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="xAxis">x镜像轴</param>
    /// <param name="yAxis">y镜像轴</param>
    /// <returns></returns>
    static Vector2[] MirrorRect(Vector2[] rect, float? xAxis = null, float? yAxis = null)
    {
        if (!xAxis.HasValue && !yAxis.HasValue)
        {
            return rect;
        }
        
        var result = rect;
        if (xAxis.HasValue)
        {
            // 确保顶点顺序正确（左下起的顺时针）
            result = FlipRect(rect, true);
            var x = xAxis.Value;
            for (int i = 0; i < 4; i++)
            {
                result[i].x = x + x - result[i].x;
            }
        }

        if (yAxis.HasValue)
        {
            // 确保顶点顺序正确（左下起的顺时针）
            result = FlipRect(result, false);
            var y = yAxis.Value;
            for (int i = 0; i < 4; i++)
            {
                result[i].y = y + y - result[i].y;
            }
        }

        return result;
    }
    
    /// <summary>
    /// 添加360度扇形矩形
    /// </summary>
    /// <param name="outer">原uv</param>
    /// <param name="v1">未经裁剪的矩形顶点信息</param>
    /// <param name="fillAmount"></param>
    /// <param name="fillOrigin">对齐方式（顶点位置）</param>
    /// <param name="clockwise">是否逆时针</param>
    /// <param name="imageType"></param>
    /// <param name="vertexHelper"></param>
    /// <param name="color"></param>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="ImageType"/> 超出范围</exception>
    static void AddRectRadial360(Vector4 outer, Vector4 v1, float fillAmount, int fillOrigin, bool clockwise,
        ImageType imageType, VertexHelper vertexHelper, Color32 color)
    {
        // 中心，也是扇形圆心
        var center = new Vector2(0.5f, 0.5f);
        // 计算外部总顶点，顺序为逆时针，底部中点为起点
        var vs = new Vector2[]
        {
            new Vector2(0.5f, 0),//0
            new Vector2(1, 0),//1
            new Vector2(1, 0.5f),//2
            new Vector2(1, 1),//3
            new Vector2(0.5f, 1),//4
            new Vector2(0, 1),//5
            new Vector2(0, 0.5f),//6
            new Vector2(0, 0),//7
        };

        int CheckIndex(int i)
        {
            if (i < 0)
            {
                i += vs.Length;
            }
            else if (i >= vs.Length)
            {
                i %= vs.Length;
            }

            return i;
        }

        // 相交于边缘的位置
        var crossPos = new Vector2();
        // 默认-1，不相交。
        float crossIndex = -1;

        // 遍历时的索引步进，同时也作为计算时的符号翻转用的系数
        var indexStep = clockwise ? 1 : -1;
        var startIndex = 0;
        var fill = 1 - fillAmount;
        // 显示使用的顶点数（不含中心点），最大为8（因为头尾相接时要用两次）
        var skipCount = Mathf.FloorToInt(fill / 0.125f);
        var vCount = 8 - skipCount;
        startIndex = fillOrigin * 2;
        // 当角度为45度的整数倍时，不计算交点（因为交点就在8个顶点之中）
        if (!Mathf.Approximately(fill % 0.125f, 0))
        {
            // fillOrigin: 0 bottom | 1 right | 2 top | 3 left
            // 这里的fillOrigin指起始半径的位置
            var radAngle = fill * 2 * Mathf.PI;
            switch (fillOrigin)
            {
                // bottom
                case 0:
                    // ±45度
                    if (fill < 0.125f || fill > 0.875f)
                    {
                        crossPos.x = 0.5f + 0.5f * Mathf.Tan(radAngle) * indexStep;
                        crossPos.y = 0;
                    }
                    // 45 - 135度
                    else if (fill > 0.125f && fill < 0.375f)
                    {
                        crossPos.x = 0.5f + 0.5f * indexStep;
                        crossPos.y = 0.5f - 0.5f / Mathf.Tan(radAngle);
                    }
                    // 135 - 225度
                    else if (fill > 0.375f && fill < 0.625f)
                    {
                        crossPos.y = 1;
                        crossPos.x = 0.5f - 0.5f * Mathf.Tan(radAngle) * indexStep;
                    }
                    // 225 - 315度
                    else if (fill > 0.625f && fill < 0.875f)
                    {
                        crossPos.x = 0.5f - 0.5f * indexStep;
                        crossPos.y = 0.5f + 0.5f / Mathf.Tan(radAngle);
                    }
                    break;
                // right
                case 1:
                    // ±45度
                    if (fill < 0.125f || fill > 0.875f)
                    {
                        crossPos.x = 1;
                        crossPos.y = 0.5f + 0.5f * Mathf.Tan(radAngle) * indexStep;
                    }
                    // 45 - 135度
                    else if (fill > 0.125f && fill < 0.375f)
                    {
                        crossPos.x = 0.5f + 0.5f / Mathf.Tan(radAngle);
                        crossPos.y = indexStep;
                    }
                    // 135 - 225度
                    else if (fill > 0.375f && fill < 0.625f)
                    {
                        crossPos.y = 0.5f - 0.5f * Mathf.Tan(radAngle) * indexStep;
                        crossPos.x = 0;
                    }
                    // 225 - 315度
                    else if (fill > 0.625f && fill < 0.875f)
                    {
                        crossPos.x = 0.5f - 0.5f / Mathf.Tan(radAngle);
                        crossPos.y = 0.5f - 0.5f * indexStep;
                    }
                    break;
                // top
                case 2:
                    // ±45度
                    if (fill < 0.125f || fill > 0.875f)
                    {
                        crossPos.x = 0.5f - 0.5f * Mathf.Tan(radAngle) * indexStep;
                        crossPos.y = 1;
                    }
                    // 45 - 135度
                    else if (fill > 0.125f && fill < 0.375f)
                    {
                        crossPos.x = 0.5f - 0.5f * indexStep;
                        crossPos.y = 0.5f + 0.5f / Mathf.Tan(radAngle);
                    }
                    // 135 - 225度
                    else if (fill > 0.375f && fill < 0.625f)
                    {
                        crossPos.y = 0;
                        crossPos.x = 0.5f + 0.5f * Mathf.Tan(radAngle) * indexStep;
                    }
                    // 225 - 315度
                    else if (fill > 0.625f && fill < 0.875f)
                    {
                        crossPos.x = 0.5f + 0.5f * indexStep;
                        crossPos.y = 0.5f - 0.5f / Mathf.Tan(radAngle);
                    }
                    break;
                // left
                case 3:
                    // ±45度
                    if (fill < 0.125f || fill > 0.875f)
                    {
                        crossPos.x = 0;
                        crossPos.y = 0.5f - 0.5f * Mathf.Tan(radAngle) * indexStep;
                    }
                    // 45 - 135度
                    else if (fill > 0.125f && fill < 0.375f)
                    {
                        crossPos.x = 0.5f - 0.5f / Mathf.Tan(radAngle);
                        crossPos.y = 0.5f - 0.5f * indexStep;
                    }
                    // 135 - 225度
                    else if (fill > 0.375f && fill < 0.625f)
                    {
                        crossPos.y = 0.5f + 0.5f * Mathf.Tan(radAngle) * indexStep;
                        crossPos.x = 1;
                    }
                    // 225 - 315度
                    else if (fill > 0.625f && fill < 0.875f)
                    {
                        crossPos.x = 0.5f + 0.5f / Mathf.Tan(radAngle);
                        crossPos.y = 0.5f + 0.5f * indexStep;
                    }
                    break;
            }
            
            // 计算索引
            crossIndex = startIndex + (skipCount + 0.5f) * indexStep;
            if (crossIndex >= vs.Length)
            {
                crossIndex %= vs.Length;
            }
            else if (crossIndex < 0)
            {
                crossIndex += vs.Length;
            }

            startIndex = CheckIndex(clockwise ? Mathf.CeilToInt(crossIndex) : Mathf.FloorToInt(crossIndex));
        }
        else
        {
            vCount = 9 - (int)(fill / 0.125f + 0.1f);
            startIndex = CheckIndex(startIndex + skipCount * indexStep);
        }

        // Debug.Log($"<color=yellow> fillAmount: {fillAmount}, vCount: {vCount}, startIndex: {startIndex}, skipCount: {skipCount}, step: {indexStep}, tan θ: {Mathf.Tan(fill * 2 * Mathf.PI)}, crossIndex: {crossIndex}, cross pos: {crossPos} </color>");
        // 5: 0-1, 4: 0.5-1, 6:0-0.5
        switch (imageType)
        {
            case ImageType.TopHalf:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[5], vs[4], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, vs[5], crossPos.y > 0.5f ? vs[4] : vs[6], false, crossPos.y <= 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, vs[5], v.y > 0.5f ? vs[4] : vs[6], false, v.y <= 0.5f));
                    j++;
                }
                break;
            case ImageType.BottomHalf:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[5], vs[6], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, vs[5], crossPos.y > 0.5f ? vs[4] : vs[6], false, crossPos.y > 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, vs[5], v.y > 0.5f ? vs[4] : vs[6], false, v.y > 0.5f));
                    j++;
                }
                break;
            case ImageType.RightHalf:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[4], vs[5], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], vs[5], crossPos.x <= 0.5f, false));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], vs[5], v.x <= 0.5f, false));
                    j++;
                }
                break;
            case ImageType.LeftHalf:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[6], vs[5], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], vs[5], crossPos.x > 0.5f, false));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], vs[5], v.x > 0.5f, false));
                    j++;
                }
                break;
            case ImageType.TopRight:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[4], vs[4], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], crossPos.y > 0.5f ? vs[4] : vs[6], crossPos.x <= 0.5f, crossPos.y <= 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], v.y > 0.5f ? vs[4] : vs[6], v.x <= 0.5f, v.y <= 0.5f));
                    j++;
                }
                break;
            case ImageType.BottomRight:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[4], vs[6], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], crossPos.y > 0.5f ? vs[4] : vs[6], crossPos.x <= 0.5f, crossPos.y > 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);
                    
                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], v.y > 0.5f ? vs[4] : vs[6], v.x <= 0.5f, v.y > 0.5f));
                    j++;
                }
                break;
            case ImageType.TopLeft:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[6], vs[4], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], crossPos.y > 0.5f ? vs[4] : vs[6], crossPos.x > 0.5f, crossPos.y <= 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);

                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], v.y > 0.5f ? vs[4] : vs[6], v.x > 0.5f, v.y <= 0.5f));
                    j++;
                }
                break;
            case ImageType.BottomLeft:
                vertexHelper.AddVert(GetUIVertex(color, center, v1, outer, vs[6], vs[6], false, false));
                if (crossIndex > 0) vertexHelper.AddVert(GetUIVertex(color, crossPos, v1, outer, crossPos.x > 0.5f ? vs[4] : vs[6], crossPos.y > 0.5f ? vs[4] : vs[6], crossPos.x > 0.5f, crossPos.y > 0.5f));
                for (int i = startIndex, j = 0; j < vCount; i += indexStep)
                {
                    i = CheckIndex(i);

                    var v = vs[i];

                    vertexHelper.AddVert(GetUIVertex(color, v, v1, outer, v.x > 0.5f ? vs[4] : vs[6], v.y > 0.5f ? vs[4] : vs[6], v.x > 0.5f, v.y > 0.5f));
                    j++;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null);
        }
        
        // 添加三角形
        var vertCount = vertexHelper.currentVertCount;
        for (int i = 2; i < vertCount; i++)
        {
            vertexHelper.AddTriangle(0, i - 1, i);
        }
    }
}

