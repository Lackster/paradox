﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
using System;
using SiliconStudio.Core.Mathematics;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    public partial class SamplerState
    {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        private TextureWrapMode textureWrapR;
#endif
        private TextureWrapMode textureWrapS;
        private TextureWrapMode textureWrapT;

        private TextureMinFilter minFilter;
        private TextureMagFilter magFilter;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        private TextureMinFilter minFilterNoMipmap;
#endif

        private float[] borderColor;

        private DepthFunction compareFunc;

        private SamplerState(GraphicsDevice device, SamplerStateDescription samplerStateDescription) : base(device)
        {
            Description = samplerStateDescription;

            textureWrapS = samplerStateDescription.AddressU.ToOpenGL();
            textureWrapT = samplerStateDescription.AddressV.ToOpenGL();
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            textureWrapR = samplerStateDescription.AddressW.ToOpenGL();
#endif
            compareFunc = samplerStateDescription.CompareFunction.ToOpenGLDepthFunction();
            borderColor = samplerStateDescription.BorderColor.ToArray();
            // TODO: How to do MipLinear vs MipPoint?
            switch (samplerStateDescription.Filter)
            {
                case TextureFilter.ComparisonMinMagLinearMipPoint:
                case TextureFilter.MinMagLinearMipPoint:
                    minFilter = TextureMinFilter.Linear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.Anisotropic:
                case TextureFilter.Linear:
                    minFilter = TextureMinFilter.LinearMipmapLinear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.MinPointMagMipLinear:
                case TextureFilter.ComparisonMinPointMagMipLinear:
                    minFilter = TextureMinFilter.NearestMipmapLinear;
                    magFilter = TextureMagFilter.Linear;
                    break;
                case TextureFilter.Point:
                    minFilter = TextureMinFilter.Nearest;
                    magFilter = TextureMagFilter.Nearest;
                    break;
                default:
                    throw new NotImplementedException();
            }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            // On OpenGL ES, we need to choose the appropriate min filter ourself if the texture doesn't contain mipmaps (done at PreDraw)
            minFilterNoMipmap = minFilter;
            if (minFilterNoMipmap == TextureMinFilter.LinearMipmapLinear)
                minFilterNoMipmap = TextureMinFilter.Linear;
            else if (minFilterNoMipmap == TextureMinFilter.NearestMipmapLinear)
                minFilterNoMipmap = TextureMinFilter.Nearest;
#endif
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();
            return true;
        }

        internal void Apply(bool hasMipmap, SamplerState oldSamplerState)
        {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            if (Description.MinMipLevel != oldSamplerState.Description.MinMipLevel)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, Description.MinMipLevel);
            if (Description.MaxMipLevel != oldSamplerState.Description.MaxMipLevel)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, Description.MaxMipLevel);
            if (textureWrapR != oldSamplerState.textureWrapR)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)textureWrapR);
            if (borderColor != oldSamplerState.borderColor)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            if (compareFunc != oldSamplerState.compareFunc)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)compareFunc);
            if (Description.MipMapLevelOfDetailBias != oldSamplerState.Description.MipMapLevelOfDetailBias)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, Description.MipMapLevelOfDetailBias);

            if (minFilter != oldSamplerState.minFilter)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
#else
            // On OpenGL ES, we need to choose the appropriate min filter ourself if the texture doesn't contain mipmaps (done at PreDraw)
            if (minFilter != oldSamplerState.minFilter)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, hasMipmap ? (int)minFilter : (int)minFilterNoMipmap);
#endif

#if !SILICONSTUDIO_PLATFORM_IOS
            if (Description.MaxAnisotropy != oldSamplerState.Description.MaxAnisotropy)
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OpenTK.Graphics.ES20.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, Description.MaxAnisotropy);
#endif
            if (magFilter != oldSamplerState.magFilter)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            if (textureWrapS != oldSamplerState.textureWrapS)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)textureWrapS);
            if (textureWrapT != oldSamplerState.textureWrapT)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)textureWrapT);
        }
    }
} 
#endif 
