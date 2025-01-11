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
                //GenerateFilledSprite(toFill, m_PreserveAspect);
                base.OnPopulateMesh(toFill);
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

        // switch (mirrorType)
        // {
        //     /// 1,2,5
        //     /// 0,3,4
        //     case MirrorType.Horizontal:
        //         vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.x, uv.y));
        //         vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.x, uv.w));
        //         vh.AddTriangle(3, 2, 5);
        //         vh.AddTriangle(5, 4, 3);
        //         break;
        //     /// 4,5
        //     /// 1,2
        //     /// 0,3
        //     case MirrorType.Vertical:
        //         vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.x, uv.y));
        //         vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.z, uv.y));
        //         vh.AddTriangle(1, 4, 5);
        //         vh.AddTriangle(5, 2, 1);
        //         break;
        //     /// 8,7,6
        //     /// 1,2,5
        //     /// 0,3,4
        //     case MirrorType.Quarter:
        //         vh.AddVert(new Vector3(v1.z, v1.y), color32, new Vector2(uv.x, uv.y));
        //         vh.AddVert(new Vector3(v1.z, v.w), color32, new Vector2(uv.x, uv.w));
        //         vh.AddTriangle(3, 2, 5);
        //         vh.AddTriangle(5, 4, 3);
        //         vh.AddVert(new Vector3(v1.z, v1.w), color32, new Vector2(uv.x, uv.y));
        //         vh.AddVert(new Vector3(v.z, v1.w), color32, new Vector2(uv.z, uv.y));
        //         vh.AddVert(new Vector3(v1.x, v1.w), color32, new Vector2(uv.x, uv.y));
        //         vh.AddTriangle(6, 5, 2);
        //         vh.AddTriangle(2, 7, 6);
        //         vh.AddTriangle(7, 2, 1);
        //         vh.AddTriangle(1, 8, 7);
        //         break;
        //     default:
        //         break;
        // }
        
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
                        float x2e=x2;
                        if (x2 > xMax)
                        {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }

                        var uvMin1 = uvMin;
                        var clipped1 = clipped;
                        //Debug.Log("i::" + i + "  j:::" + j);
                        switch (imageResourceType)
                        {
                            case ImageType.LeftHalf:
                            case ImageType.RightHalf:
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
                                break;
                            case ImageType.TopHalf:
                            case ImageType.BottomHalf:
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
}

